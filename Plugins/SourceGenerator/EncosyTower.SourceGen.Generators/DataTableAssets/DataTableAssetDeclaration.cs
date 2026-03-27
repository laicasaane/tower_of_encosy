using System;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace EncosyTower.SourceGen.Generators.DataTableAssets
{
    using static EncosyTower.SourceGen.Generators.DataTableAssets.Helpers;

    public partial struct DataTableAssetDeclaration : IEquatable<DataTableAssetDeclaration>
    {
        /// <summary>Excluded from equality — location is not stable across incremental runs.</summary>
        public LocationInfo location;

        public string className;
        public string openingSource;
        public string closingSource;
        public string hintName;
        public string sourceFilePath;
        public string idTypeName;
        public string dataTypeName;
        public string convertedIdTypeName;
        public string convertExpression;
        public bool dataTypeImplementsIInitializable;
        public bool getIdMethodIsImplemented;
        public bool initializeMethodIsImplemented;
        public bool convertIdMethodIsImplemented;

        public readonly bool IsValid
            => string.IsNullOrEmpty(className) == false
            && string.IsNullOrEmpty(idTypeName) == false
            && string.IsNullOrEmpty(dataTypeName) == false
            ;

        public static DataTableAssetDeclaration Extract(
              GeneratorAttributeSyntaxContext context
            , CancellationToken token
        )
        {
            token.ThrowIfCancellationRequested();

            if (context.TargetNode is not ClassDeclarationSyntax classSyntax)
            {
                return default;
            }

            if (classSyntax.TypeParameterList is TypeParameterListSyntax typeParamList
                && typeParamList.Parameters.Count > 0
            )
            {
                return default;
            }

            if (context.TargetSymbol is not INamedTypeSymbol symbol || symbol.IsAbstract)
            {
                return default;
            }

            // Walk base type chain to find DataTableAsset<TId, TData[, TConv]>
            ITypeSymbol idType = null;
            ITypeSymbol dataType = null;
            ITypeSymbol convertedIdType = null;

            var baseType = symbol.BaseType;

            while (baseType != null)
            {
                var typeArguments = baseType.TypeArguments;

                if (typeArguments.Length >= 2)
                {
                    var fullName = baseType.ToFullName();

                    if (fullName.StartsWith(DATA_TABLE_ASSET))
                    {
                        idType = typeArguments[0];
                        dataType = typeArguments[1];

                        if (typeArguments.Length > 2)
                        {
                            convertedIdType = typeArguments[2];
                        }

                        break;
                    }
                }

                baseType = baseType.BaseType;
            }

            if (idType == null || dataType == null)
            {
                return default;
            }

            // Type kind validation — invalid types are reported by the analyzer, not the generator
            if (idType.TypeKind is not (TypeKind.Struct or TypeKind.Class or TypeKind.Enum))
            {
                return default;
            }

            if (dataType.TypeKind is not (TypeKind.Struct or TypeKind.Class or TypeKind.Enum))
            {
                return default;
            }

            // Scan members to detect manually-implemented methods
            var members = symbol.GetMembers();
            var comparer = SymbolEqualityComparer.Default;
            var getIdImpl = false;
            var initializeImpl = false;
            var convertIdImpl = false;

            foreach (var member in members)
            {
                if (member is not IMethodSymbol method)
                {
                    continue;
                }

                var name = method.Name;
                var parameters = method.Parameters;
                var parametersLength = parameters.Length;

                if (name == "GetId"
                    && parametersLength == 1
                    && parameters[0].RefKind == RefKind.In
                    && comparer.Equals(parameters[0].Type, dataType)
                )
                {
                    getIdImpl = true;
                    continue;
                }

                if (name == "Initialize"
                    && parametersLength == 1
                    && parameters[0].RefKind == RefKind.Ref
                    && comparer.Equals(parameters[0].Type, dataType)
                )
                {
                    initializeImpl = true;
                    continue;
                }

                if (name == "ConvertId"
                    && parametersLength == 1
                    && parameters[0].RefKind == RefKind.None
                    && comparer.Equals(parameters[0].Type, idType)
                )
                {
                    convertIdImpl = true;
                    continue;
                }
            }

            // Nothing to generate if GetId is already implemented
            if (getIdImpl)
            {
                return default;
            }

            // Pre-compute type names
            var idTypeName = idType.ToFullName();
            var dataTypeName = dataType.ToFullName();
            var convertedIdTypeName = convertedIdType?.ToFullName();

            // Pre-compute convert expression (symbol-only, no syntax access)
            string convertExpression = null;

            if (convertedIdType != null && convertIdImpl == false)
            {
                convertExpression = GetConvertExpression(idType, convertedIdType, convertedIdTypeName);

                if (string.IsNullOrEmpty(convertExpression))
                {
                    convertExpression = GetConvertExpression(convertedIdType, idType, convertedIdTypeName);
                }

                if (string.IsNullOrEmpty(convertExpression))
                {
                    convertExpression = null;
                }
            }

            // IInitializable check
            var dataTypeImplementsIInitializable = dataType.ImplementsInterface(IINITIALIZABLE);

            // Opening/closing source — only syntax tree access in this method
            TypeCreationHelpers.GenerateOpeningAndClosingSource(
                  classSyntax
                , token
                , out var openingSource
                , out var closingSource
                , printAdditionalUsings: PrintAdditionalUsings
            );

            // Hint name and source file path
            var syntaxTree = classSyntax.SyntaxTree;
            var fileTypeName = symbol.ToFileName();
            var compilation = context.SemanticModel.Compilation;

            var hintName = syntaxTree.GetGeneratedSourceFileName(
                  GENERATOR_NAME
                , classSyntax
                , fileTypeName
            );

            var sourceFilePath = syntaxTree.GetGeneratedSourceFilePath(
                  compilation.Assembly.Name
                , GENERATOR_NAME
                , fileTypeName
            );

            return new DataTableAssetDeclaration {
                location = LocationInfo.From(classSyntax.GetLocation()),
                className = classSyntax.Identifier.Text,
                openingSource = openingSource,
                closingSource = closingSource,
                hintName = hintName,
                sourceFilePath = sourceFilePath,
                idTypeName = idTypeName,
                dataTypeName = dataTypeName,
                convertedIdTypeName = convertedIdTypeName,
                convertExpression = convertExpression,
                dataTypeImplementsIInitializable = dataTypeImplementsIInitializable,
                getIdMethodIsImplemented = getIdImpl,
                initializeMethodIsImplemented = initializeImpl,
                convertIdMethodIsImplemented = convertIdImpl,
            };
        }

        private static void PrintAdditionalUsings(ref Printer p)
        {
            p.PrintEndLine();
            p.Print("#pragma warning disable CS0105 // Using directive appeared previously in this namespace").PrintEndLine();
            p.PrintEndLine();
            p.PrintLine("using System;");
            p.PrintLine("using System.Diagnostics.CodeAnalysis;");
            p.PrintLine("using System.CodeDom.Compiler;");
            p.PrintLine("using System.Runtime.CompilerServices;");
            p.PrintLine("using System.Runtime.InteropServices;");
            p.PrintLine("using EncosyTower.Databases;");
            p.PrintEndLine();
            p.Print("#pragma warning restore CS0105 // Using directive appeared previously in this namespace").PrintEndLine();
            p.PrintEndLine();
        }

        private static string GetConvertExpression(ITypeSymbol from, ITypeSymbol to, string typeName)
        {
            var members = from.GetMembers();
            var comparer = SymbolEqualityComparer.Default;

            foreach (var member in members)
            {
                if (member is not IMethodSymbol method)
                {
                    continue;
                }

                if (method.IsStatic == false
                    || method.DeclaredAccessibility != Accessibility.Public
                    || method.Parameters.Length != 1
                )
                {
                    continue;
                }

                var parameters = method.Parameters;

                if (comparer.Equals(method.ReturnType, to) == false
                    || comparer.Equals(parameters[0].Type, from) == false
                )
                {
                    continue;
                }

                if (method.Name == "op_Implicit")
                {
                    return "value";
                }

                if (method.Name == "op_Explicit")
                {
                    return $"({typeName})value";
                }
            }

            return null;
        }

        public readonly bool Equals(DataTableAssetDeclaration other)
            => string.Equals(className, other.className, StringComparison.Ordinal)
            && string.Equals(openingSource, other.openingSource, StringComparison.Ordinal)
            && string.Equals(closingSource, other.closingSource, StringComparison.Ordinal)
            && string.Equals(hintName, other.hintName, StringComparison.Ordinal)
            && string.Equals(sourceFilePath, other.sourceFilePath, StringComparison.Ordinal)
            && string.Equals(idTypeName, other.idTypeName, StringComparison.Ordinal)
            && string.Equals(dataTypeName, other.dataTypeName, StringComparison.Ordinal)
            && string.Equals(convertedIdTypeName, other.convertedIdTypeName, StringComparison.Ordinal)
            && string.Equals(convertExpression, other.convertExpression, StringComparison.Ordinal)
            && dataTypeImplementsIInitializable == other.dataTypeImplementsIInitializable
            && getIdMethodIsImplemented == other.getIdMethodIsImplemented
            && initializeMethodIsImplemented == other.initializeMethodIsImplemented
            && convertIdMethodIsImplemented == other.convertIdMethodIsImplemented
            ;

        public readonly override bool Equals(object obj)
            => obj is DataTableAssetDeclaration other && Equals(other);

        public readonly override int GetHashCode()
            => HashValue.Combine(
                  className
                , openingSource
                , closingSource
                , hintName
                , sourceFilePath
                , idTypeName
                , dataTypeName
                , convertedIdTypeName
            )
            .Add(convertExpression)
            .Add(dataTypeImplementsIInitializable)
            .Add(getIdMethodIsImplemented)
            .Add(initializeMethodIsImplemented)
            .Add(convertIdMethodIsImplemented)
            ;
    }
}
