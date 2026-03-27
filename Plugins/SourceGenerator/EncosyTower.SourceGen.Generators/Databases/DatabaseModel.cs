using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Newtonsoft.Json.Utilities;

namespace EncosyTower.SourceGen.Generators.Databases
{
    using static EncosyTower.SourceGen.Generators.Databases.Helpers;

    public partial struct DatabaseModel : IEquatable<DatabaseModel>
    {
        // Excluded from equality — not part of cache key
        public LocationInfo location;

        // Code-generation data
        public string typeName;
        public bool isStruct;
        public NamingStrategy namingStrategy;
        public string assetName;
        public bool withInstanceAPI;
        public string openingSource;
        public string closingSource;
        public string hintName;
        public string sourceFilePath;
        public EquatableArray<TableModel> tables;

        public readonly bool IsValid
            => string.IsNullOrEmpty(typeName) == false;

        public static DatabaseModel Extract(
              GeneratorAttributeSyntaxContext context
            , CancellationToken token
        )
        {
            token.ThrowIfCancellationRequested();

            if (context.TargetNode is not TypeDeclarationSyntax typeSyntax
                || (typeSyntax.IsKind(SyntaxKind.ClassDeclaration) == false
                    && typeSyntax.IsKind(SyntaxKind.StructDeclaration) == false)
                || typeSyntax.Modifiers.Any(SyntaxKind.StaticKeyword)
                || context.TargetSymbol is not INamedTypeSymbol typeSymbol
            )
            {
                return default;
            }

            var compilation = context.SemanticModel.Compilation;

            if (compilation.IsValidCompilation(DATABASES_NAMESPACE, SKIP_ATTRIBUTE) == false)
            {
                return default;
            }

            var typeName = typeSyntax.Identifier.Text;
            var isStruct = typeSymbol.IsValueType;

            // ForAttributeWithMetadataName guarantees at least one matching attribute
            var attribute = context.Attributes[0];

            // constructor arguments: NamingStrategy
            var namingStrategy = default(NamingStrategy);

            foreach (var arg in attribute.ConstructorArguments)
            {
                if (arg.Kind != TypedConstantKind.Array && arg.Value != null)
                {
                    namingStrategy = arg.Value.ToNamingStrategy();
                    break;
                }
            }

            // named arguments: AssetName, WithInstanceAPI
            var assetName = $"DatabaseAsset_{typeName}";
            var withInstanceAPI = false;

            foreach (var arg in attribute.NamedArguments)
            {
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

            // Build tables — silently skip invalid ones; the analyzer handles diagnostics
            var tableList = new List<TableModel>();
            var members = typeSymbol.GetMembers();

            foreach (var member in members)
            {
                if (member is not IPropertySymbol property)
                {
                    continue;
                }

                if (property.Type is not INamedTypeSymbol propType)
                {
                    continue;
                }

                var tableAttrib = member.GetAttribute(TABLE_ATTRIBUTE);

                if (tableAttrib == null)
                {
                    continue;
                }

                if (propType.IsAbstract
                    || propType.IsGenericType
                    || propType.BaseType == null
                    || propType.TryGetGenericType(DATA_TABLE_ASSET, 3, 2, out _) == false
                )
                {
                    continue;
                }

                tableList.Add(new TableModel(
                      typeFullName: propType.ToFullName()
                    , typeName: propType.Name
                    , propertyName: property.Name
                    , namingStrategy: namingStrategy
                ));
            }

            if (tableList.Count < 1)
            {
                return default;
            }

            // Pre-compute opening/closing source so WriteCode never touches the syntax tree
            TypeCreationHelpers.GenerateOpeningAndClosingSource(
                  typeSyntax
                , token
                , out var openingSource
                , out var closingSource
                , printAdditionalUsings: PrintAdditionalUsings
            );

            // Pre-compute hint name and source file path
            var syntaxTree = typeSyntax.SyntaxTree;
            var fileTypeName = typeSymbol.ToFileName();

            var hintName = syntaxTree.GetGeneratedSourceFileName(
                  DatabaseGenerator.GENERATOR_NAME
                , typeSyntax
                , fileTypeName
            );

            var sourceFilePath = syntaxTree.GetGeneratedSourceFilePath(
                  compilation.Assembly.Name
                , DatabaseGenerator.GENERATOR_NAME
                , fileTypeName
            );

            var location = LocationInfo.From(
                typeSymbol.Locations.Length > 0
                    ? typeSymbol.Locations[0]
                    : Location.None
            );

            EquatableArray<TableModel> tables = ImmutableArray.CreateRange(tableList);

            return new DatabaseModel {
                location = location,
                typeName = typeName,
                isStruct = isStruct,
                namingStrategy = namingStrategy,
                assetName = assetName,
                withInstanceAPI = withInstanceAPI,
                openingSource = openingSource,
                closingSource = closingSource,
                hintName = hintName,
                sourceFilePath = sourceFilePath,
                tables = tables,
            };
        }

        private static void PrintAdditionalUsings(ref Printer p)
        {
            p.PrintEndLine();
            p.Print("#pragma warning disable CS0105 // Using directive appeared previously in this namespace").PrintEndLine();
            p.PrintEndLine();
            p.PrintLine("using System;");
            p.PrintLine("using System.Diagnostics;");
            p.PrintLine("using System.Diagnostics.CodeAnalysis;");
            p.PrintLine("using System.CodeDom.Compiler;");
            p.PrintLine("using System.Runtime.CompilerServices;");
            p.PrintLine("using System.Runtime.InteropServices;");
            p.PrintLine("using EncosyTower.AssetKeys;");
            p.PrintLine("using EncosyTower.Databases;");
            p.PrintLine("using EncosyTower.Databases.SourceGen;");
            p.PrintLine("using EncosyTower.Initialization;");
            p.PrintLine("using EncosyTower.StringIds;");
            p.PrintLine("using EncosyTower.UnityExtensions;");
            p.PrintEndLine();
            p.PrintLine("using SDCA = System.Diagnostics.CodeAnalysis;");
            p.PrintEndLine();
            p.Print("#pragma warning restore CS0105 // Using directive appeared previously in this namespace").PrintEndLine();
            p.PrintEndLine();
        }

        public readonly override bool Equals(object obj)
            => obj is DatabaseModel other && Equals(other);

        public readonly bool Equals(DatabaseModel other)
            => string.Equals(typeName, other.typeName, StringComparison.Ordinal)
            && isStruct == other.isStruct
            && namingStrategy == other.namingStrategy
            && string.Equals(assetName, other.assetName, StringComparison.Ordinal)
            && withInstanceAPI == other.withInstanceAPI
            && string.Equals(openingSource, other.openingSource, StringComparison.Ordinal)
            && string.Equals(closingSource, other.closingSource, StringComparison.Ordinal)
            && string.Equals(hintName, other.hintName, StringComparison.Ordinal)
            && string.Equals(sourceFilePath, other.sourceFilePath, StringComparison.Ordinal)
            && tables.Equals(other.tables)
            ;

        public readonly override int GetHashCode()
            => HashValue.Combine(
                  typeName
                , isStruct
                , namingStrategy
                , assetName
                , withInstanceAPI
                , hintName
                , tables
            );
    }
}
