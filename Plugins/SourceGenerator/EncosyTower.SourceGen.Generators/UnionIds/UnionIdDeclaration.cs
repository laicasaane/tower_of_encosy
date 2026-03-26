using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using EncosyTower.SourceGen.Common.UnionIds;
using EncosyTower.SourceGen.Generators.EnumExtensions;
using Microsoft.CodeAnalysis;

namespace EncosyTower.SourceGen.Generators.UnionIds
{
    internal partial class UnionIdDeclaration
    {
        public bool IsInvalid { get; }

        public string SimpleName { get; }

        public string NamespaceName { get; }

        public EquatableArray<string> ContainingTypes { get; }

        public Accessibility Accessibility { get; }

        public bool ParentIsNamespace { get; }

        public UnionIdSize Size { get; }

        public bool PreserveIdKindOrder { get; }

        public string DisplayNameForId { get; }

        public string DisplayNameForKind { get; }

        public string RawTypeName { get; }

        public string IdRawUnsignedTypeName { get; }

        public string IdRawSignedTypeName { get; }

        public bool KindEnumIsEmpty { get; }

        public string KindRawTypeName { get; }

        public char Separator { get; }

        public int TypeSize { get; }

        public int KindFieldOffset { get; }

        public ParsableStructConverterSettings ConverterSettings { get; }

        public List<KindRef> KindRefs { get; }

        public References References { get; }

        public EnumExtensionsDeclaration KindExtensionsRef { get; }

        public List<EnumExtensionsDeclaration> IdEnumExtensionsRefs { get; }

        public string FixedStringType { get; }

        public UnionIdDeclaration(
              IdDeclaration id
            , ImmutableArray<KindDeclaration> externalKinds
            , References references
        )
        {
            SimpleName = id.simpleName;
            NamespaceName = id.namespaceName;
            ContainingTypes = id.containingTypes;
            Accessibility = id.accessibility;
            ParentIsNamespace = id.parentIsNamespace;
            Size = id.size;
            Separator = id.separator;
            PreserveIdKindOrder = id.kindSettings.HasFlag(UnionIdKindSettings.PreserveOrder);
            ConverterSettings = id.converterSettings;
            DisplayNameForId = string.IsNullOrWhiteSpace(id.displayNameForId) ? string.Empty : id.displayNameForId;
            DisplayNameForKind = string.IsNullOrWhiteSpace(id.displayNameForKind) ? string.Empty : id.displayNameForKind;
            References = references;

            // Merge inline kinds + external kinds that target this id
            var allKinds = new List<KindDeclaration>(id.inlineKinds.Count + externalKinds.Length);

            foreach (var k in id.inlineKinds)
            {
                allKinds.Add(k);
            }

            foreach (var k in externalKinds)
            {
                if (string.Equals(k.idFullName, id.fullName, StringComparison.Ordinal))
                    allKinds.Add(k);
            }

            var kindRefs = KindRefs = new List<KindRef>(allKinds.Count);
            var enumMembers = new List<EnumMemberDeclaration>(allKinds.Count);
            var seenFullNames = new HashSet<string>(StringComparer.Ordinal);
            var uniqueKindNames = new Dictionary<string, string>(allKinds.Count, StringComparer.Ordinal);
            var idExtensionsRefs = IdEnumExtensionsRefs = new List<EnumExtensionsDeclaration>(allKinds.Count);
            var removeSuffix = id.kindSettings.HasFlag(UnionIdKindSettings.RemoveSuffix);
            var maxIdSize = 0;
            var kindFixedStringBytes = 0;
            var idFixedStringBytes = 0;
            var kindHasDisplayName = false;
            ulong maxKeyOrder = 0;

            foreach (var candidate in allKinds)
            {
                // Skip if kind is the id itself
                if (string.Equals(candidate.kindFullName, id.fullName, StringComparison.Ordinal))
                    continue;

                // Skip duplicates (same kind type)
                if (seenFullNames.Contains(candidate.kindFullName))
                    continue;

                var customName = candidate.name.ToValidIdentifier();
                var hasCustomName = string.IsNullOrEmpty(customName) == false;
                var kindName = hasCustomName ? customName : candidate.kindSimpleName;

                if (uniqueKindNames.ContainsKey(kindName))
                    continue;

                seenFullNames.Add(candidate.kindFullName);
                uniqueKindNames.Add(kindName, candidate.kindFullName);

                if (hasCustomName == false && removeSuffix)
                    kindName = RemoveTypeKindSuffix(kindName);

                var order = candidate.order;
                var size = candidate.kindUnmanagedSize;
                var nameByteCount = kindName.GetByteCount();
                kindFixedStringBytes = Math.Max(kindFixedStringBytes, nameByteCount);

                var toStringMethods = candidate.toStringMethods;
                var enumExtensionsName = string.Empty;
                var nestedEnumExtensions = false;
                var tryParseSpan = candidate.tryParseSpan;
                var equality = candidate.equality;

                if (candidate.isEnum)
                {
                    // Build nested EnumExtensionsDeclaration from precomputed data
                    var memList = new List<EnumMemberDeclaration>(candidate.kindEnumValues.Count);

                    foreach (var m in candidate.kindEnumValues)
                        memList.Add(m);

                    var extensions = new EnumExtensionsDeclaration(references.unityCollections, candidate.kindEnumFixedStringBytes) {
                        GeneratedCode = GENERATED_CODE,
                        Name = kindName,
                        ExtensionsName = EnumExtensionsDeclaration.GetNameExtensionsClass(kindName),
                        StructName = EnumExtensionsDeclaration.GetNameExtendedStruct(kindName),
                        ParentIsNamespace = false,
                        FullyQualifiedName = candidate.kindFullName,
                        UnderlyingTypeName = candidate.kindEnumUnderlyingTypeName,
                        Members = memList,
                        Accessibility = Accessibility.Private,
                        IsDisplayAttributeUsed = candidate.kindEnumIsDisplayAttributeUsed,
                        HasFlags = candidate.kindEnumHasFlags,
                        OnlyNames = true,
                        NoDocumentation = true,
                        OnlyClass = true,
                    };

                    if (candidate.hasExternalEnumExtensions)
                    {
                        enumExtensionsName = candidate.externalEnumExtensionsFullName;
                    }
                    else
                    {
                        enumExtensionsName = extensions.ExtensionsName;
                        idExtensionsRefs.Add(extensions);
                        nestedEnumExtensions = true;
                    }

                    idFixedStringBytes = Math.Max(idFixedStringBytes, extensions.FixedStringBytes);

                    if (references.unityCollections)
                    {
                        toStringMethods |= ToStringMethods.ToFixedString | ToStringMethods.ToDisplayString;
                    }
                }
                else if (candidate.isKindAlsoUnionId)
                {
                    idFixedStringBytes = 128;
                }
                else if (references.unityCollections)
                {
                    if (candidate.hasToDisplayFixedString)
                    {
                        toStringMethods |= ToStringMethods.ToDisplayFixedString;
                        idFixedStringBytes = Math.Max(idFixedStringBytes, candidate.toDisplayFixedStringBytes);
                    }

                    if (candidate.hasToFixedString)
                    {
                        toStringMethods |= ToStringMethods.ToFixedString;
                        idFixedStringBytes = Math.Max(idFixedStringBytes, candidate.toFixedStringBytes);
                    }
                }

                kindRefs.Add(new KindRef {
                    name = kindName,
                    fullName = candidate.kindFullName,
                    fullNameFromNullable = candidate.kindFullNameFromNullable,
                    displayName = candidate.displayName,
                    enumExtensionsName = enumExtensionsName,
                    order = order,
                    size = size,
                    isEnum = candidate.isEnum,
                    signed = candidate.signed,
                    toStringMethods = toStringMethods,
                    tryParseSpan = tryParseSpan,
                    equality = equality,
                    nestedEnumExtensions = nestedEnumExtensions,
                });

                enumMembers.Add(new EnumMemberDeclaration {
                    name = kindName,
                    order = order,
                    displayName = candidate.displayName,
                });

                if (string.IsNullOrWhiteSpace(candidate.displayName) == false)
                    kindHasDisplayName = true;

                if (size > maxIdSize)
                    maxIdSize = size;

                if (order > maxKeyOrder)
                    maxKeyOrder = order;

                if (maxIdSize >= (int)UnionIdSize.ULong)
                {
                    IsInvalid = true;
                    return;
                }
            }

            kindRefs.Sort(Compare);

            var allowEmptyKind = id.kindSettings.HasFlag(UnionIdKindSettings.AllowEmpty);
            KindEnumIsEmpty = allowEmptyKind && kindRefs.Count <= 1;

            var idSize = NormalizeSize(maxIdSize);

            if (KindEnumIsEmpty)
            {
                var typeSize = TypeSize = ToSize(Size, idSize);
                RawTypeName = ToUnsignedTypeName(typeSize);
                IdRawUnsignedTypeName = kindRefs.Count > 0 ? kindRefs[0].fullName : ToUnsignedTypeName(idSize);
                IdRawSignedTypeName = kindRefs.Count > 0 ? kindRefs[0].fullName : ToSignedTypeName(idSize);
                KindRawTypeName = RawTypeName;
                KindFieldOffset = 0;
            }
            else
            {
                var kindSizeCandidate = ItemCountToSize(Math.Max((ulong)kindRefs.Count, maxKeyOrder));
                var typeSize = TypeSize = Math.Max(GetTypeSize(idSize + kindSizeCandidate, Size), 2);
                var kindSizeCapacity = typeSize - idSize;
                var kindSize = kindSizeCandidate <= kindSizeCapacity
                    ? kindSizeCandidate
                    : NormalizeSize(kindSizeCapacity);

                var removeCount = kindRefs.Count - SizeToIntCount(kindSize);

                for (; removeCount >= 0; removeCount--)
                    kindRefs.RemoveAt(kindRefs.Count - 1);

                RawTypeName = ToUnsignedTypeName(typeSize);
                IdRawUnsignedTypeName = ToUnsignedTypeName(idSize);
                IdRawSignedTypeName = ToSignedTypeName(idSize);
                KindRawTypeName = ToUnsignedTypeName(kindSize);
                KindFieldOffset = typeSize - kindSize;
            }

            var kindEnumName = $"{id.simpleName.ToValidIdentifier()}_IdKind";

            KindExtensionsRef = new EnumExtensionsDeclaration(references.unityCollections, kindFixedStringBytes) {
                GeneratedCode = GENERATED_CODE,
                Name = kindEnumName,
                ExtensionsName = EnumExtensionsDeclaration.GetNameExtensionsClass(kindEnumName),
                StructName = EnumExtensionsDeclaration.GetNameExtendedStruct(kindEnumName),
                ParentIsNamespace = id.parentIsNamespace,
                FullyQualifiedName = $"{id.fullName}.IdKind",
                UnderlyingTypeName = KindRawTypeName,
                Members = enumMembers,
                Accessibility = id.accessibility,
                IsDisplayAttributeUsed = kindHasDisplayName,
            };

            int fixedStringBytes;

            if (id.fixedStringBytes.HasValue)
            {
                fixedStringBytes = id.fixedStringBytes.Value;
            }
            else
            {
                fixedStringBytes = kindFixedStringBytes + idFixedStringBytes;
            }

            FixedStringType = GeneratorHelpers.GetFixedStringFullyQualifiedTypeName(fixedStringBytes);
        }

        private static string RemoveTypeKindSuffix(string name)
        {
            while (name.Length > 4 && name.EndsWith("Type") || name.EndsWith("Kind"))
                name = name.Remove(name.Length - 4, 4);

            return name;
        }

        private static string ToUnsignedTypeName(int value)
            => value switch {
                <= 0 => string.Empty,
                <= 1 => "byte",
                <= 2 => "ushort",
                <= 4 => "uint",
                <= 8 => "ulong",
                <= 12 => "global::EncosyTower.UnionIds.Types.UnionId_UInt3",
                <= 16 => "global::EncosyTower.UnionIds.Types.UnionId_ULong2",
                _ => "ulong",
            };

        private static string ToSignedTypeName(int value)
            => value switch {
                <= 0 => string.Empty,
                <= 1 => "sbyte",
                <= 2 => "short",
                <= 4 => "int",
                <= 8 => "ulong",
                <= 12 => "global::EncosyTower.UnionIds.Types.UnionId_UInt3",
                <= 16 => "global::EncosyTower.UnionIds.Types.UnionId_ULong2",
                _ => "long",
            };

        private static int NormalizeSize(int value)
            => value switch {
                <= 1 => 1,
                <= 2 => 2,
                <= 4 => 4,
                <= 8 => 8,
                <= 12 => 12,
                <= 16 => 16,
                _ => 8,
            };

        private static int ItemCountToSize(ulong itemCount)
            => itemCount switch {
                <= byte.MaxValue => 1,
                <= ushort.MaxValue => 2,
                _ => 4,
            };

        private static int SizeToIntCount(int size)
            => size switch {
                <= 1 => byte.MaxValue,
                <= 2 => ushort.MaxValue,
                _ => int.MaxValue,
            };

        private static int ToSize(UnionIdSize value)
            => value switch {
                UnionIdSize.UShort => 2,
                UnionIdSize.UInt => 4,
                UnionIdSize.ULong => 8,
                UnionIdSize.UInt3 => 12,
                UnionIdSize.ULong2 => 16,
                _ => 4,
            };

        private static int ToSize(UnionIdSize value, int candidate)
            => value switch {
                UnionIdSize.UShort => 2,
                UnionIdSize.UInt => 4,
                UnionIdSize.ULong => 8,
                UnionIdSize.UInt3 => 12,
                UnionIdSize.ULong2 => 16,
                _ => NormalizeSize(candidate),
            };

        private static int GetTypeSize(int candidateSize, UnionIdSize inputSize)
        {
            var sizeCandidate = NormalizeSize(candidateSize);
            var sizeInput = ToSize(inputSize);
            return sizeCandidate > sizeInput ? sizeCandidate : sizeInput;
        }

        private static int Compare(KindRef a, KindRef b)
        {
            var comp = a.order.CompareTo(b.order);
            return comp != 0 ? comp : string.CompareOrdinal(a.name, b.name);
        }

        public struct KindRef
        {
            public string name;
            public string fullName;
            public string fullNameFromNullable;
            public string displayName;
            public string enumExtensionsName;
            public ulong order;
            public int size;
            public bool isEnum;
            public bool signed;
            public ToStringMethods toStringMethods;
            public MemberExistence tryParseSpan;
            public Equality equality;
            public bool nestedEnumExtensions;
        }
    }
}
