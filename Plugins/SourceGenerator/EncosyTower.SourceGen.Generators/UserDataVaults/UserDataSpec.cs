using System;
using System.Collections.Generic;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace EncosyTower.SourceGen.Generators.UserDataVaults
{
    using static Helpers;

    internal partial struct UserDataSpec : IEquatable<UserDataSpec>
    {
        public LocationInfo location;
        public string openingSource;
        public string closingSource;
        public string hintName;
        public string typeName;
        public string typeKeyword;
        public bool generateInterface;
        public MemberDefinition memberId;
        public MemberDefinition memberVersion;

        public readonly bool IsValid
            => string.IsNullOrEmpty(typeName) == false;

        public static UserDataSpec Extract(
              GeneratorAttributeSyntaxContext context
            , CancellationToken token
        )
        {
            token.ThrowIfCancellationRequested();

            if (context.TargetNode is not TypeDeclarationSyntax syntax
                || syntax.TypeParameterList is not null
                || context.TargetSymbol is not INamedTypeSymbol symbol
                || symbol.IsAbstract
                || symbol.IsUnboundGenericType
                || symbol.TypeKind is not (TypeKind.Class or TypeKind.Struct)
            )
            {
                return default;
            }

            GetMemberDefinitions(symbol, token, out var memberId, out var memberVersion);
            var generateInterface = symbol.InheritsFromInterface(IUSER_DATA, false, token) == false;

            if (generateInterface == false
                && memberId.ShouldGenerate == false
                && memberVersion.ShouldGenerate == false
            )
            {
                return default;
            }

            TypeCreationHelpers.GenerateOpeningAndClosingSource(
                  syntax
                , token
                , out var openingSource
                , out var closingSource
                , printAdditionalUsings: PrintAdditionalUsings
            );

            var syntaxTree = syntax.SyntaxTree;
            var fileTypeName = symbol.ToFileName();
            var hintName = syntaxTree.GetHintName(syntax, fileTypeName);

            return new UserDataSpec {
                location = LocationInfo.From(syntax.GetLocation()),
                typeName = symbol.Name,
                typeKeyword = symbol.ToPartialTypeKeyword(),
                openingSource = openingSource,
                closingSource = closingSource,
                hintName = hintName,
                generateInterface = generateInterface,
                memberId = memberId,
                memberVersion = memberVersion,
            };
        }

        private static void GetMemberDefinitions(
              INamedTypeSymbol symbol
            , CancellationToken token
            , out MemberDefinition memberId
            , out MemberDefinition memberVersion
        )
        {
            token.ThrowIfCancellationRequested();

            memberId = memberVersion = default;

            GetMembers(symbol, false, token, ref memberId, ref memberVersion);

            if (HasBoth(memberId, memberVersion))
            {
                return;
            }

            var baseType = symbol.BaseType;

            while (baseType is { TypeKind: TypeKind.Class })
            {
                token.ThrowIfCancellationRequested();
                GetMembers(baseType, true, token, ref memberId, ref memberVersion);

                if (HasBoth(memberId, memberVersion))
                {
                    return;
                }

                baseType = baseType.BaseType;
            }

            static bool HasBoth(MemberDefinition memberId, MemberDefinition memberVersion)
                => memberId.IsValid && memberVersion.IsValid;

            static void GetMembers(
                  ITypeSymbol type
                , bool isBaseTypeSearch
                , CancellationToken token
                , ref MemberDefinition memberId
                , ref MemberDefinition memberVersion
            )
            {
                var members = type.GetMembers();
                var fields = new HashSet<IFieldSymbol>(SymbolEqualityComparer.Default);

                foreach (var member in members)
                {
                    token.ThrowIfCancellationRequested();

                    if (HasBoth(memberId, memberVersion))
                    {
                        return;
                    }

                    if (member is IFieldSymbol field)
                    {
                        fields.Add(field);
                    }

                    if (member is not IPropertySymbol property
                        || property.Type.SpecialType != SpecialType.System_String
                    )
                    {
                        continue;
                    }

                    if (isBaseTypeSearch)
                    {
                        if (property.DeclaredAccessibility is not (Accessibility.Public or Accessibility.Protected))
                        {
                            continue;
                        }
                    }
                    else if (property.DeclaredAccessibility != Accessibility.Public)
                    {
                        continue;
                    }

                    if (memberId.IsValid == false && property.Name == "Id")
                    {
                        memberId = new MemberDefinition {
                            name = property.Name,
                            isField = false,
                            type = isBaseTypeSearch
                                ? property.IsAbstract
                                    ? MemberDefinitionType.DefinedInBaseTypeAsAbstract
                                    : MemberDefinitionType.DefinedInBaseType
                                : MemberDefinitionType.Defined,
                        };
                    }
                    else if (memberVersion.IsValid == false && property.Name == "Version")
                    {
                        memberVersion = new MemberDefinition {
                            name = property.Name,
                            isField = false,
                            type = isBaseTypeSearch
                                ? property.IsAbstract
                                    ? MemberDefinitionType.DefinedInBaseTypeAsAbstract
                                    : MemberDefinitionType.DefinedInBaseType
                                : MemberDefinitionType.Defined,
                        };
                    }
                }

                foreach (var field in fields)
                {
                    token.ThrowIfCancellationRequested();

                    if (HasBoth(memberId, memberVersion))
                    {
                        return;
                    }

                    if (field.DeclaredAccessibility is not (Accessibility.Public or Accessibility.Protected)
                        || field.Type.SpecialType != SpecialType.System_String
                    )
                    {
                        continue;
                    }

                    if (memberId.IsValid == false && field.Name is ("id" or "_id" or "m_id"))
                    {
                        memberId = new MemberDefinition {
                            name = field.Name,
                            isField = true,
                            type = isBaseTypeSearch
                                ? MemberDefinitionType.DefinedInBaseType
                                : MemberDefinitionType.Defined,
                        };
                    }
                    else if (memberVersion.IsValid == false && field.Name is ("version" or "_version" or "m_version"))
                    {
                        memberVersion = new MemberDefinition {
                            name = field.Name,
                            isField = true,
                            type = isBaseTypeSearch
                                ? MemberDefinitionType.DefinedInBaseType
                                : MemberDefinitionType.Defined,
                        };
                    }
                }
            }
        }

        private static void PrintAdditionalUsings(ref Printer p)
        {
            p.PrintEndLine();
            p.Print("#pragma warning disable CS0105 // Using directive appeared previously in this namespace").PrintEndLine();
            p.PrintEndLine();
            p.PrintLine("using SCDC = global::System.CodeDom.Compiler;");
            p.PrintLine("using SDCA = global::System.Diagnostics.CodeAnalysis;");
            p.PrintLine("using ETUV = global::EncosyTower.UserDataVaults;");
            p.PrintEndLine();
            p.Print("#pragma warning restore CS0105 // Using directive appeared previously in this namespace").PrintEndLine();
            p.PrintEndLine();
        }

        public readonly bool Equals(UserDataSpec other)
            => string.Equals(typeName, other.typeName, StringComparison.Ordinal)
            && string.Equals(typeKeyword, other.typeKeyword, StringComparison.Ordinal)
            && generateInterface == other.generateInterface
            && memberId.Equals(other.memberId)
            && memberVersion.Equals(other.memberVersion);

        public readonly override bool Equals(object obj)
            => obj is UserDataSpec other && Equals(other);

        public readonly override int GetHashCode()
            => HashValue.Combine(typeName, typeKeyword, generateInterface, memberId, memberVersion);
    }

    internal struct MemberDefinition : IEquatable<MemberDefinition>
    {
        public string name;
        public MemberDefinitionType type;
        public bool isField;

        public readonly bool IsValid
            => type != MemberDefinitionType.Undefined && string.IsNullOrEmpty(name) == false;

        public readonly bool ShouldGenerate
            => isField || type is MemberDefinitionType.Undefined or MemberDefinitionType.DefinedInBaseTypeAsAbstract;

        public readonly bool Equals(MemberDefinition other)
            => string.Equals(name, other.name, StringComparison.Ordinal)
            && isField == other.isField
            && type == other.type;

        public readonly override bool Equals(object obj)
            => obj is MemberDefinition other && Equals(other);

        public readonly override int GetHashCode()
            => HashValue.Combine(name, type, isField);
    }

    internal enum MemberDefinitionType : byte
    {
        Undefined = 0,
        Defined,
        DefinedInBaseType,
        DefinedInBaseTypeAsAbstract,
    }
}
