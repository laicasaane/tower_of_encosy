using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using EncosyTower.SourceGen.Generators.EnumExtensions;
using Microsoft.CodeAnalysis;

namespace EncosyTower.SourceGen.Generators.EnumTemplates
{
    internal partial class EnumTemplateDeclaration
    {
        /// <summary>Simple identifier text of the template struct (e.g. <c>"MyEnum_EnumTemplate"</c>).</summary>
        public string TemplateSimpleName { get; }

        public string TemplateFullName { get; }

        public string NamespaceName { get; }

        public EquatableArray<string> ContainingTypes { get; }

        public string EnumName { get; }

        public string UnderlyingTypeName { get; }

        public Accessibility Accessibility { get; }

        public List<EnumMemberRef> MemberRefs { get; }

        public Dictionary<string, IndexOrIndices> MemberIndexMap { get; }

        public References References { get; }

        public EnumExtensionsDeclaration ExtensionsRef { get; }

        internal EnumTemplateDeclaration(
              in EnumTemplateCandidate templateCandidate
            , ImmutableArray<TemplateMemberCandidate> externalMembers
            , References references
        )
        {
            References = references;

            TemplateSimpleName = templateCandidate.templateSimpleName;
            TemplateFullName = templateCandidate.templateFullName;
            NamespaceName = templateCandidate.namespaceName;
            ContainingTypes = templateCandidate.containingTypes;
            Accessibility = templateCandidate.accessibility;

            // Silently fall back to the struct name when the naming convention is not followed.
            // EnumTemplateAnalyzer reports the proper diagnostic.
            TryGetEnumName(templateCandidate.templateSimpleName, out var enumName);
            EnumName = enumName;

            // Merge source-1 inline members (pre-extracted in EnumTemplateCandidate.Extract)
            // with source-2 external members from the kind provider, filtered to this template.
            var mergedCandidates = new List<TemplateMemberCandidate>(
                templateCandidate.inlineMembers.Count + externalMembers.Length
            );

            foreach (var m in templateCandidate.inlineMembers)
            {
                mergedCandidates.Add(m);
            }

            foreach (var m in externalMembers)
            {
                if (string.Equals(m.templateFullName, templateCandidate.templateFullName, StringComparison.Ordinal))
                {
                    mergedCandidates.Add(m);
                }
            }

            mergedCandidates.Sort(static (a, b) => a.order.CompareTo(b.order));

            var memberRefs = MemberRefs = new(mergedCandidates.Count);
            var types = new HashSet<string>(StringComparer.Ordinal);
            var tempMemberRefs = new List<EnumMemberRef>(mergedCandidates.Count);
            var uniqueMembers = new HashSet<string>(StringComparer.Ordinal);
            ulong currentBaseOrder = 0;
            var maxByteCount = 0;
            var isDisplayAttributeUsed = false;

            foreach (var candidate in mergedCandidates)
            {
                if (candidate.IsValid == false)
                {
                    continue;
                }

                // Silently skip invalid underlying type — EnumTemplateAnalyzer reports it.
                if (candidate.enumMembers && IsSupportEnum(candidate.underlyingType) == false)
                {
                    continue;
                }

                // Silently skip duplicates — EnumTemplateAnalyzer reports them.
                if (types.Contains(candidate.typeFullName))
                {
                    continue;
                }

                if (candidate.order > currentBaseOrder)
                {
                    currentBaseOrder = candidate.order;
                }

                memberRefs.Add(new EnumMemberRef {
                    typeFullName = candidate.typeFullName,
                    isComment = true,
                    member = new EnumMemberDeclaration {
                        name = candidate.typeFullName,
                    },
                });

                if (candidate.enumMembers == false)
                {
                    var name = string.IsNullOrEmpty(candidate.alternateName)
                        ? candidate.typeSimpleName
                        : candidate.alternateName.ToValidIdentifier();

                    var displayName = string.IsNullOrEmpty(candidate.displayName) ? name : candidate.displayName;

                    memberRefs.Add(new EnumMemberRef {
                        typeFullName = candidate.typeFullName,
                        baseOrder = currentBaseOrder,
                        attributes = ImmutableArray<AttributeInfo>.Empty,
                        member = new EnumMemberDeclaration {
                            name = name,
                            order = currentBaseOrder,
                            displayName = displayName,
                        },
                    });

                    types.Add(candidate.typeFullName);
                    currentBaseOrder += 1;
                    continue;
                }

                var baseOrder = currentBaseOrder;

                foreach (var entry in candidate.enumEntries)
                {
                    var memberName = entry.name;

                    // Silently skip duplicate member names — EnumTemplateAnalyzer reports them.
                    if (uniqueMembers.Add(memberName) == false)
                    {
                        continue;
                    }

                    var entryDisplayName = string.IsNullOrEmpty(entry.displayName) ? null : entry.displayName;

                    if (entryDisplayName != null && isDisplayAttributeUsed == false)
                    {
                        isDisplayAttributeUsed = true;
                    }

                    var order = currentBaseOrder + entry.value;
                    var nameByteCount = GetByteCount(memberName);
                    maxByteCount = Math.Max(nameByteCount, maxByteCount);

                    tempMemberRefs.Add(new EnumMemberRef {
                        typeFullName = candidate.typeFullName,
                        baseOrder = currentBaseOrder,
                        value = entry.value,
                        fromEnumType = true,
                        underlyingType = candidate.underlyingType,
                        attributes = entry.attributes.AsImmutableArray(),
                        member = new EnumMemberDeclaration {
                            name = memberName,
                            order = order,
                            displayName = entryDisplayName,
                        },
                    });

                    if (order > baseOrder)
                    {
                        baseOrder = order;
                    }
                }

                if (tempMemberRefs.Count > 0)
                {
                    types.Add(candidate.typeFullName);
                }

                currentBaseOrder = baseOrder + 1;
                tempMemberRefs.Sort(static (a, b) => a.member.order.CompareTo(b.member.order));
                memberRefs.AddRange(tempMemberRefs);
                tempMemberRefs.Clear();
            }

            UnderlyingTypeName = GeneratorHelpers.GetEnumUnderlyingTypeFromMemberCount(currentBaseOrder);

            TryGetEnumName(TemplateFullName, out var fullyQualifiedEnumName);

            ExtensionsRef = new EnumExtensionsDeclaration(references.unityCollections, maxByteCount) {
                GeneratedCode = GENERATED_CODE,
                Name = EnumName,
                ExtensionsName = EnumExtensionsDeclaration.GetNameExtensionsClass(EnumName),
                StructName = EnumExtensionsDeclaration.GetNameExtendedStruct(EnumName),
                ParentIsNamespace = templateCandidate.parentIsNamespace,
                FullyQualifiedName = fullyQualifiedEnumName,
                UnderlyingTypeName = UnderlyingTypeName,
                Accessibility = Accessibility,
                IsDisplayAttributeUsed = isDisplayAttributeUsed,
                Members = memberRefs
                    .Where(static x => x.isComment == false)
                    .Select(static x => x.member)
                    .ToList(),
            };

            var count = memberRefs.Count;
            var memberIndexMap = MemberIndexMap = new Dictionary<string, IndexOrIndices>(count, StringComparer.Ordinal);

            for (var i = 0; i < count; i++)
            {
                var memberRef = memberRefs[i];

                if (memberRef.isComment)
                {
                    continue;
                }

                var typeFullName = memberRef.typeFullName;

                if (memberIndexMap.TryGetValue(typeFullName, out var indices) == false)
                {
                    memberIndexMap[typeFullName] = indices = memberRef.fromEnumType ? new(new List<int>()) : new(i);
                }

                indices.Indices?.Add(i);
            }
        }


        private static bool TryGetEnumName(string templateName, out string enumName)
        {
            var templateIndex = templateName.IndexOf("_EnumTemplate", StringComparison.Ordinal);

            if (templateIndex > 0)
            {
                enumName = templateName.Substring(0, templateIndex);
                return true;
            }

            templateIndex = templateName.IndexOf("_Template", StringComparison.Ordinal);

            if (templateIndex > 0)
            {
                enumName = templateName.Substring(0, templateIndex);
                return true;
            }

            enumName = templateName;
            return false;
        }

        internal static bool IsSupportEnum(SpecialType type)
        {
            return type switch {
                SpecialType.System_Byte => true,
                SpecialType.System_UInt16 => true,
                SpecialType.System_UInt32 => true,
                SpecialType.System_UInt64 => true,
                _ => false,
            };
        }

        internal static string ToTypeName(SpecialType type)
        {
            return type switch {
                SpecialType.System_Byte => "byte",
                SpecialType.System_UInt16 => "ushort",
                SpecialType.System_UInt32 => "uint",
                SpecialType.System_UInt64 => "ulong",
                _ => "uint",
            };
        }

        private static int GetByteCount(string value)
        {
            if (value == null)
                return 0;

            return Encoding.UTF8.GetByteCount(value);
        }

        /// <summary>
        /// Working struct used during code generation. Not a pipeline model — not required to
        /// be cache-friendly.
        /// </summary>
        public struct EnumMemberRef
        {
            /// <summary>Fully qualified name of the type contributing this member.</summary>
            public string typeFullName;
            public ulong baseOrder;
            public ulong value;
            public SpecialType underlyingType;
            public bool isComment;
            public bool fromEnumType;
            public EnumMemberDeclaration member;
            public ImmutableArray<AttributeInfo> attributes;
        }

        public readonly struct IndexOrIndices
        {
            public readonly int? Index;
            public readonly List<int> Indices;

            public IndexOrIndices(int index)
            {
                Index = index;
                Indices = null;
            }

            public IndexOrIndices(List<int> indices)
            {
                Index = default;
                Indices = indices;
            }
        }
    }
}
