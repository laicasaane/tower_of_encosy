using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace EncosyTower.SourceGen.Generators.Databases
{
    using static EncosyTower.SourceGen.Generators.Databases.Helpers;

    public partial struct DatabaseSpec : IEquatable<DatabaseSpec>
    {
        public LocationInfo location;

        public string typeName;
        public bool isStruct;
        public NameCasing nameCasing;
        public string assetName;
        public bool withInstanceAPI;
        public string openingSource;
        public string closingSource;
        public string hintName;
        public EquatableArray<TableSpec> tables;

        public readonly bool IsValid
            => string.IsNullOrEmpty(typeName) == false;

        public static DatabaseSpec Extract(
              GeneratorAttributeSyntaxContext context
            , CancellationToken token
        )
        {
            token.ThrowIfCancellationRequested();

            if (context.TargetNode is not TypeDeclarationSyntax typeSyntax
                || (typeSyntax.IsKind(SyntaxKind.ClassDeclaration) == false
                    && typeSyntax.IsKind(SyntaxKind.StructDeclaration) == false
                )
                || typeSyntax.Modifiers.Any(SyntaxKind.StaticKeyword)
                || context.TargetSymbol is not INamedTypeSymbol typeSymbol
            )
            {
                return default;
            }

            var compilation = context.SemanticModel.Compilation;
            var typeName = typeSyntax.Identifier.Text;
            var isStruct = typeSymbol.IsValueType;
            var attribute = context.Attributes[0];
            var nameCasing = NameCasing.Pascal;

            foreach (var arg in attribute.ConstructorArguments)
            {
                token.ThrowIfCancellationRequested();

                if (arg.Kind != TypedConstantKind.Array && arg.Value != null)
                {
                    nameCasing = arg.Value.ToNameCasing();
                    break;
                }
            }

            token.ThrowIfCancellationRequested();

            var assetName = $"DatabaseAsset_{typeName}";
            var withInstanceAPI = false;

            foreach (var arg in attribute.NamedArguments)
            {
                token.ThrowIfCancellationRequested();

                if (arg.Key == "AssetName"
                    && arg.Value.Kind == TypedConstantKind.Primitive
                    && arg.Value.Value?.ToString() is string name
                    && string.IsNullOrWhiteSpace(name) == false
                )
                {
                    assetName = name;
                    continue;
                }

                if (arg.Key == "WithInstanceAPI"
                    && arg.Value.Kind == TypedConstantKind.Primitive
                    && arg.Value.Value is bool withAPI
                )
                {
                    withInstanceAPI = withAPI;
                    continue;
                }
            }

            token.ThrowIfCancellationRequested();

            var propList = new List<IPropertySymbol>();
            var dedupTableAssetMap = new Dictionary<string, bool>(StringComparer.Ordinal);
            var members = typeSymbol.GetMembers();

            foreach (var member in members)
            {
                token.ThrowIfCancellationRequested();

                if (member is not IPropertySymbol property)
                {
                    continue;
                }

                if (property.Type is not INamedTypeSymbol propType)
                {
                    continue;
                }

                var tableAttrib = member.GetAttribute(TABLE_ATTRIBUTE, token);

                if (tableAttrib == null)
                {
                    continue;
                }

                if (propType.IsAbstract
                    || propType.IsGenericType
                    || propType.BaseType == null
                    || propType.TryGetGenericType(DATA_TABLE_ASSET, 3, 2, out _, token) == false
                )
                {
                    continue;
                }

                propList.Add(property);

                var tableTypeSimpleName = propType.Name;
                var existingTableAsset = dedupTableAssetMap.ContainsKey(tableTypeSimpleName);
                dedupTableAssetMap[tableTypeSimpleName] = existingTableAsset;
            }

            var tableList = new List<TableSpec>();

            foreach (var prop in propList)
            {
                var propType = prop.Type;

                dedupTableAssetMap.TryGetValue(propType.Name, out var deduplicateAssetName);

                tableList.Add(new TableSpec {
                    typeFullName = propType.ToFullName(),
                    typeName = propType.Name,
                    propertyName = prop.Name,
                    nameCasing = nameCasing,
                    deduplicateAssetName = deduplicateAssetName,
                });
            }

            if (tableList.Count < 1)
            {
                return default;
            }

            TypeCreationHelpers.GenerateOpeningAndClosingSource(
                  typeSyntax
                , token
                , out var openingSource
                , out var closingSource
                , printAdditionalUsings: PrintAdditionalUsings
            );

            var syntaxTree = typeSyntax.SyntaxTree;
            var hintName = syntaxTree.GetHintName(typeSyntax, typeSymbol.ToFileName());

            var location = LocationInfo.From(
                typeSymbol.Locations.Length > 0
                    ? typeSymbol.Locations[0]
                    : Location.None
            );

            EquatableArray<TableSpec> tables = ImmutableArray.CreateRange(tableList);

            return new DatabaseSpec {
                location = location,
                typeName = typeName,
                isStruct = isStruct,
                nameCasing = nameCasing,
                assetName = assetName,
                withInstanceAPI = withInstanceAPI,
                openingSource = openingSource,
                closingSource = closingSource,
                hintName = hintName,
                tables = tables,
            };
        }

        private static void PrintAdditionalUsings(ref Printer p)
        {
            p.PrintEndLine();
            p.Print("#pragma warning disable CS0105 // Using directive appeared previously in this namespace").PrintEndLine();
            p.PrintEndLine();
            p.PrintLine("using S = global::System;");
            p.PrintLine("using SD = System.Diagnostics;");
            p.PrintLine("using SDCA = System.Diagnostics.CodeAnalysis;");
            p.PrintLine("using SCDC = global::System.CodeDom.Compiler;");
            p.PrintLine("using SRCS = global::System.Runtime.CompilerServices;");
            p.PrintLine("using SRIS = global::System.Runtime.InteropServices;");
            p.PrintLine("using ETAK = global::EncosyTower.AssetKeys;");
            p.PrintLine("using ET = global::EncosyTower.Common;");
            p.PrintLine("using ETDB = EncosyTower.Databases;");
            p.PrintLine("using ETDBSG = global::EncosyTower.Databases.SourceGen;");
            p.PrintLine("using ETI = global::EncosyTower.Initialization;");
            p.PrintLine("using ETSI = global::EncosyTower.StringIds;");
            p.PrintLine("using ETUE = global::EncosyTower.UnityExtensions;");
            p.PrintEndLine();
            p.Print("#pragma warning restore CS0105 // Using directive appeared previously in this namespace").PrintEndLine();
            p.PrintEndLine();
        }

        public readonly override bool Equals(object obj)
            => obj is DatabaseSpec other && Equals(other);

        public readonly bool Equals(DatabaseSpec other)
            => string.Equals(typeName, other.typeName, StringComparison.Ordinal)
            && isStruct == other.isStruct
            && nameCasing == other.nameCasing
            && string.Equals(assetName, other.assetName, StringComparison.Ordinal)
            && withInstanceAPI == other.withInstanceAPI
            && tables.Equals(other.tables)
            ;

        public readonly override int GetHashCode()
            => HashValue.Combine(
                  typeName
                , isStruct
                , nameCasing
                , assetName
                , withInstanceAPI
                , tables
            );
    }
}
