using System;
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
        public MemberDefinitionType propertyId;
        public MemberDefinitionType propertyVersion;

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

            var propertyId = GetMemberDefinitionType(symbol, "Id", token);
            var propertyVersion = GetMemberDefinitionType(symbol, "Version", token);
            var generateInterface = symbol.InheritsFromInterface(IUSER_DATA, false) == false;

            if (generateInterface == false
                && RequiresGeneratedProperty(propertyId) == false
                && RequiresGeneratedProperty(propertyVersion) == false
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
                propertyId = propertyId,
                propertyVersion = propertyVersion,
            };
        }

        private static bool RequiresGeneratedProperty(MemberDefinitionType type)
            => type is MemberDefinitionType.Undefined or MemberDefinitionType.DefinedInBaseTypeAsAbstract;

        private static MemberDefinitionType GetMemberDefinitionType(
              INamedTypeSymbol symbol
            , string memberName
            , CancellationToken token
        )
        {
            var members = symbol.GetMembers(memberName);

            for (var i = 0; i < members.Length; i++)
            {
                token.ThrowIfCancellationRequested();

                if (members[i] is IPropertySymbol)
                {
                    return MemberDefinitionType.Defined;
                }
            }

            var baseType = symbol.BaseType;

            while (baseType is { TypeKind: TypeKind.Class })
            {
                token.ThrowIfCancellationRequested();

                members = baseType.GetMembers(memberName);

                for (var i = 0; i < members.Length; i++)
                {
                    token.ThrowIfCancellationRequested();

                    if (members[i] is IPropertySymbol property)
                    {
                        return property.IsAbstract
                            ? MemberDefinitionType.DefinedInBaseTypeAsAbstract
                            : MemberDefinitionType.DefinedInBaseType;
                    }
                }

                baseType = baseType.BaseType;
            }

            return MemberDefinitionType.Undefined;
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
            && propertyId == other.propertyId
            && propertyVersion == other.propertyVersion;

        public readonly override bool Equals(object obj)
            => obj is UserDataSpec other && Equals(other);

        public readonly override int GetHashCode()
            => HashValue.Combine(typeName, typeKeyword, generateInterface, propertyId, propertyVersion);
    }

    internal enum MemberDefinitionType : byte
    {
        Undefined = 0,
        Defined,
        DefinedInBaseType,
        DefinedInBaseTypeAsAbstract,
    }
}
