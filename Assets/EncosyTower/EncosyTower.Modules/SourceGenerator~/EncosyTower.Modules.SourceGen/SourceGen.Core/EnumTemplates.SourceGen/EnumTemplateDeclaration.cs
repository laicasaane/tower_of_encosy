using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using EncosyTower.Modules.EnumExtensions.SourceGen;
using EncosyTower.Modules.SourceGen;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace EncosyTower.Modules.EnumTemplates.SourceGen
{
    internal partial class EnumTemplateDeclaration
    {
        private const string MEMBERS_FROM_ENUM_ATTRIBUTE = "global::EncosyTower.Modules.EnumExtensions.EnumTemplateMembersFromEnumAttribute";
        private const string MEMBER_FROM_TYPE_NAME_ATTRIBUTE = "global::EncosyTower.Modules.EnumExtensions.EnumTemplateMemberFromTypeNameAttribute";

        public StructDeclarationSyntax Syntax { get; }

        public string TemplateFullName { get; }

        public string EnumName { get; }

        public string UnderlyingTypeName { get; }

        public Accessibility Accessibility { get; }

        public List<EnumMemberRef> MemberRefs { get; }

        public Dictionary<INamedTypeSymbol, IndexOrIndices> MemberIndexMap { get; }

        public References References { get; }

        public EnumExtensionsDeclaration ExtensionsRef { get; }

        public EnumTemplateDeclaration(
              SourceProductionContext context
            , TemplateCandidate templateCandidate
            , ImmutableArray<KindCandidate> kindCandidates
            , References references
        )
        {
            Syntax = templateCandidate.syntax;
            References = references;

            var templateSymbol = templateCandidate.symbol;
            TemplateFullName = templateSymbol.ToFullName();
            Accessibility = templateSymbol.DeclaredAccessibility;

            if (TryGetEnumName(templateSymbol.Name, out var enumName) == false)
            {
                context.ReportDiagnostic(NotEndWithTemplateSuffix, Syntax);
            }

            EnumName = enumName;

            var sortedKindCandidates = new List<KindCandidate>(kindCandidates);
            TryGetCandidates(context, templateSymbol, sortedKindCandidates);
            sortedKindCandidates.Sort(Compare);

            var symbolComparer = SymbolEqualityComparer.Default;
            var memberRefs = MemberRefs = new(sortedKindCandidates.Count);
            var types = new HashSet<INamedTypeSymbol>(symbolComparer);
            var tempMemberRefs = new List<EnumMemberRef>(sortedKindCandidates.Count);
            var uniqueMembers = new Dictionary<string, INamedTypeSymbol>(sortedKindCandidates.Count);
            ulong currentBaseOrder = 0;
            var maxByteCount = 0;
            var isDisplayAttributeUsed = false;

            foreach (var candidate in sortedKindCandidates)
            {
                if (symbolComparer.Equals(templateSymbol, candidate.templateSymbol) == false)
                {
                    continue;
                }

                var typeSymbol = candidate.typeSymbol;

                if (candidate.enumMembers)
                {
                    if (typeSymbol.EnumUnderlyingType is not INamedTypeSymbol underlyingType
                        || IsSupportEnum(underlyingType.SpecialType) == false
                    )
                    {
                        context.ReportDiagnostic(
                              NotSupportUnderlyingType
                            , candidate.attributeData.ApplicationSyntaxReference.GetSyntax()
                        );
                        continue;
                    }
                }
                else if (typeSymbol.IsUnboundGenericType)
                {
                    context.ReportDiagnostic(
                          NotSupportUnboundGenericType
                        , candidate.attributeData.ApplicationSyntaxReference.GetSyntax()
                        , typeSymbol.ToSimpleName()
                    );
                    continue;
                }

                if (types.Contains(typeSymbol))
                {
                    context.ReportDiagnostic(
                          TypeAlreadyDeclared
                        , candidate.attributeData.ApplicationSyntaxReference.GetSyntax()
                        , typeSymbol.ToSimpleName()
                    );
                    continue;
                }

                if (candidate.order > currentBaseOrder)
                {
                    currentBaseOrder = candidate.order;
                }

                memberRefs.Add(new EnumMemberRef {
                    typeSymbol = typeSymbol,
                    isComment = true,
                    member = new EnumMemberDeclaration {
                        name = typeSymbol.ToFullName(),
                    },
                });

                if (candidate.enumMembers == false)
                {
                    var typeName = typeSymbol.ToSimpleValidIdentifier();

                    memberRefs.Add(new EnumMemberRef {
                        typeSymbol = typeSymbol,
                        baseOrder = currentBaseOrder,
                        member = new EnumMemberDeclaration {
                            name = typeName,
                            order = currentBaseOrder,
                            displayName = typeName,
                        },
                    });

                    types.Add(typeSymbol);
                    currentBaseOrder += 1;
                    continue;
                }

                var members = typeSymbol.GetMembers();
                var baseOrder = currentBaseOrder;

                foreach (var member in members)
                {
                    if (member is not IFieldSymbol field || field.ConstantValue is null)
                    {
                        continue;
                    }

                    var memberName = field.Name;

                    if (uniqueMembers.TryGetValue(memberName, out var otherEnum))
                    {
                        context.ReportDiagnostic(
                              SameMemberIsIgnored
                            , candidate.attributeData.ApplicationSyntaxReference.GetSyntax()
                            , memberName
                            , otherEnum.ToSimpleName()
                        );
                        continue;
                    }

                    uniqueMembers.Add(memberName, typeSymbol);

                    var displayName = GetDisplayName(field);
                    isDisplayAttributeUsed = string.IsNullOrEmpty(displayName) == false;

                    var value = field.ConstantValue is object constValue
                        ? Convert.ToUInt64(constValue) : 0;

                    var order = currentBaseOrder + value;
                    var nameByteCount = GetByteCount(memberName);
                    maxByteCount = Math.Max(nameByteCount, maxByteCount);

                    tempMemberRefs.Add(new EnumMemberRef {
                        typeSymbol = typeSymbol,
                        baseOrder = currentBaseOrder,
                        value = value,
                        fromEnumType = true,
                        member = new EnumMemberDeclaration {
                            name = memberName,
                            order = order,
                            displayName = displayName,
                        },
                    });

                    if (order > baseOrder)
                    {
                        baseOrder = order;
                    }
                }

                if (tempMemberRefs.Count > 0)
                {
                    types.Add(typeSymbol);
                }

                currentBaseOrder = baseOrder + 1;
                tempMemberRefs.Sort(Compare);
                memberRefs.AddRange(tempMemberRefs);
                tempMemberRefs.Clear();
            }

            UnderlyingTypeName = ToUnderlyingTypeName(ItemCountToSize(currentBaseOrder));

            TryGetEnumName(TemplateFullName, out var fullyQualifiedEnumName);

            ExtensionsRef = new EnumExtensionsDeclaration(references.unityCollections, maxByteCount) {
                GeneratedCode = GENERATED_CODE,
                Name = EnumName,
                ExtensionsName = $"{EnumName}Extensions",
                ExtensionsWrapperName = $"{EnumName}ExtensionsWrapper",
                ParentIsNamespace = templateCandidate.syntax.Parent is NamespaceDeclarationSyntax,
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
            var memberIndexMap = MemberIndexMap = new(count, symbolComparer);

            for (var i = 0; i < count; i++)
            {
                var memberRef = memberRefs[i];

                if (memberRef.isComment)
                {
                    continue;
                }

                var typeSymbol = memberRef.typeSymbol;

                if (memberIndexMap.TryGetValue(typeSymbol, out var indices) == false)
                {
                    memberIndexMap[typeSymbol] = indices = memberRef.fromEnumType ? new(new List<int>()) : new(i);
                }

                indices.Indices?.Add(i);
            }
        }

        private static void TryGetCandidates(
              SourceProductionContext context
            , INamedTypeSymbol symbol
            , List<KindCandidate> output
        )
        {
            var attributes = symbol.GetAttributes(MEMBERS_FROM_ENUM_ATTRIBUTE, MEMBER_FROM_TYPE_NAME_ATTRIBUTE);

            foreach (var attrib in attributes)
            {
                if (attrib == null)
                {
                    continue;
                }

                if (attrib.ConstructorArguments.Length < 2)
                {
                    context.ReportDiagnostic(
                          MustSpecifyTypeAndOrder
                        , attrib.ApplicationSyntaxReference.GetSyntax()
                    );
                    continue;
                }

                var args = attrib.ConstructorArguments;
                var typeArg = args[0];

                if (typeArg.Kind != TypedConstantKind.Type
                    || typeArg.Value is not INamedTypeSymbol typeSymbol
                )
                {
                    context.ReportDiagnostic(
                          MustBeTypeOfExpression
                        , attrib.ApplicationSyntaxReference.GetSyntax()
                    );
                    continue;
                }

                var candidate = new KindCandidate {
                    typeSymbol = typeSymbol,
                    templateSymbol = symbol,
                    attributeData = attrib,
                    enumMembers = typeSymbol.TypeKind == TypeKind.Enum,
                };

                var orderArg = args[1];

                if (orderArg.Kind == TypedConstantKind.Primitive
                    && orderArg.Value is ulong value
                )
                {
                    candidate.order = value;
                }

                output.Add(candidate);
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

        private static string ToUnderlyingTypeName(int value)
            => value switch {
                <= 1 => "byte",
                <= 2 => "ushort",
                <= 4 => "uint",
                _ => "ulong",
            };

        private static int ItemCountToSize(ulong itemCount)
            => itemCount switch {
                <= byte.MaxValue => 1,
                <= ushort.MaxValue => 2,
                <= uint.MaxValue => 4,
                _ => 8,
            };

        private static int Compare(KindCandidate a, KindCandidate b)
        {
            return a.order.CompareTo(b.order);
        }

        private static int Compare(EnumMemberRef a, EnumMemberRef b)
        {
            return a.member.order.CompareTo(b.member.order);
        }

        private static bool IsSupportEnum(SpecialType type)
        {
            return type switch {
                SpecialType.System_Byte => true,
                SpecialType.System_UInt16 => true,
                SpecialType.System_UInt32 => true,
                SpecialType.System_UInt64 => true,
                _ => false,
            };
        }

        private static int GetByteCount(string value)
        {
            if (value == null)
                return 0;

            return Encoding.UTF8.GetByteCount(value);
        }

        private static string GetDisplayName(IFieldSymbol field)
        {
            string displayName = null;

            foreach (var attribute in field.GetAttributes())
            {
                var attributeName = attribute.AttributeClass?.Name ?? string.Empty;

                switch (attributeName)
                {
                    case nameof(System.ObsoleteAttribute):
                    {
                        goto RETURN;
                    }

                    case "DisplayAttribute":
                    case "DisplayNameAttribute":
                    case "InspectorNameAttribute":
                    {
                        if (attribute.ConstructorArguments.Length > 0)
                        {
                            var arg = attribute.ConstructorArguments[0];

                            if (arg.Kind == TypedConstantKind.Primitive && arg.Value?.ToString() is string dn)
                            {
                                displayName = dn;
                                goto RETURN;
                            }
                        }

                        if (attribute.NamedArguments.Length > 0)
                        {
                            foreach (var arg in attribute.NamedArguments)
                            {
                                if (arg.Key == "Name"
                                    && arg.Value.Kind == TypedConstantKind.Primitive
                                    && arg.Value.Value?.ToString() is string dn
                                )
                                {
                                    displayName = dn;
                                    goto RETURN;
                                }
                            }
                        }

                        break;
                    }
                }
            }

            RETURN:
            return displayName;
        }

        public static readonly DiagnosticDescriptor NotEndWithTemplateSuffix = new(
              id: "ENUM_TEMPLATE_0001"
            , title: "Not end with template suffix"
            , messageFormat: "The name of a union enum template must end with either \"_EnumTemplate\" or \"_Template\" suffix"
            , category: "EnumTemplateGenerator"
            , defaultSeverity: DiagnosticSeverity.Warning
            , isEnabledByDefault: true
            , description: "The name of a union enum template must end with either \"_EnumTemplate\" or \"_Template\" suffix"
        );

        public static readonly DiagnosticDescriptor NotSupportUnderlyingType = new(
              id: "ENUM_TEMPLATE_0002"
            , title: "Not support underlying type"
            , messageFormat: "Only enums whose underlying type is either byte, ushort, uint or ulong are supported"
            , category: "EnumTemplateGenerator"
            , defaultSeverity: DiagnosticSeverity.Warning
            , isEnabledByDefault: true
            , description: "Only enums whose underlying type is either byte, ushort, uint or ulong are supported"
        );

        public static readonly DiagnosticDescriptor TypeAlreadyDeclared = new(
              id: "ENUM_TEMPLATE_0003"
            , title: "Type has already be declared"
            , messageFormat: "Type \"{0}\" has already be declared"
            , category: "EnumTemplateGenerator"
            , defaultSeverity: DiagnosticSeverity.Warning
            , isEnabledByDefault: true
            , description: "Type has already be declared"
        );

        public static readonly DiagnosticDescriptor MustSpecifyTypeAndOrder = new(
              id: "ENUM_TEMPLATE_0004"
            , title: "Must specify type and order"
            , messageFormat: "Must specify type and order"
            , category: "EnumTemplateGenerator"
            , defaultSeverity: DiagnosticSeverity.Error
            , isEnabledByDefault: true
            , description: "Must specify type and order"
        );

        public static readonly DiagnosticDescriptor MustBeTypeOfExpression = new(
              id: "ENUM_TEMPLATE_0005"
            , title: "First argument must be type of expression"
            , messageFormat: "First argument must be type of expression"
            , category: "EnumTemplateGenerator"
            , defaultSeverity: DiagnosticSeverity.Error
            , isEnabledByDefault: true
            , description: "First argument must be type of expression"
        );

        public static readonly DiagnosticDescriptor SameMemberIsIgnored = new(
              id: "ENUM_TEMPLATE_0006"
            , title: "A member of the same name will be ignored"
            , messageFormat: "Member \"{0}\" has already been defined in another enum \"{1}\""
            , category: "EnumTemplateGenerator"
            , defaultSeverity: DiagnosticSeverity.Warning
            , isEnabledByDefault: true
            , description: "A member of the same name will be ignored"
        );

        public static readonly DiagnosticDescriptor NotSupportUnboundGenericType = new(
              id: "ENUM_TEMPLATE_0007"
            , title: "Not support unbound generic type"
            , messageFormat: "\"{0}\" must be a non-generic type or a closed generic type"
            , category: "EnumTemplateGenerator"
            , defaultSeverity: DiagnosticSeverity.Warning
            , isEnabledByDefault: true
            , description: "Only non-generic types or fully closed generic types are supported"
        );

        public struct EnumMemberRef
        {
            public INamedTypeSymbol typeSymbol;
            public ulong baseOrder;
            public ulong value;
            public bool isComment;
            public bool fromEnumType;
            public EnumMemberDeclaration member;
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
