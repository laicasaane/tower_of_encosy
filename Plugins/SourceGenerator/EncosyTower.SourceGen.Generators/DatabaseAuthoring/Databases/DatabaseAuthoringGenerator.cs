using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Threading;
using EncosyTower.SourceGen.Generators.Data;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace EncosyTower.SourceGen.Generators.DatabaseAuthoring
{
    using static EncosyTower.SourceGen.Generators.DatabaseAuthoring.Helpers;

    [Generator]
    public class DatabaseAuthoringGenerator : IIncrementalGenerator
    {
        public const string GENERATOR_NAME = nameof(DatabaseAuthoringGenerator);

        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            var projectPathProvider = SourceGenHelpers.GetSourceGenConfigProvider(context);

            var databaseRefProvider = context.SyntaxProvider.CreateSyntaxProvider(
                predicate: IsValidDatabaseSyntax,
                transform: GetDatabaseRefSemanticMatch
            ).Where(static t => t is { });

            var compilationProvider = context.CompilationProvider
                .Select(CompilationRef.GetCompilation);

            var combined = databaseRefProvider.Collect()
                .Combine(compilationProvider)
                .Combine(projectPathProvider);

            context.RegisterSourceOutput(combined, static (sourceProductionContext, source) => {
                GenerateOutput(
                    sourceProductionContext
                    , source.Left.Right
                    , source.Left.Left
                    , source.Right.projectPath
                    , source.Right.outputSourceGenFiles
                );
            });
        }

        private static bool IsValidDatabaseSyntax(SyntaxNode node, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            return node is TypeDeclarationSyntax typeSyntax
                && IsSupportedTypeSyntax(typeSyntax)
                && typeSyntax.AttributeLists.Count > 0
                && typeSyntax.HasAttributeCandidate("EncosyTower.Databases.Authoring", "AuthorDatabase");
        }

        public static DatabaseRef GetDatabaseRefSemanticMatch(
              GeneratorSyntaxContext context
            , CancellationToken token
        )
        {
            token.ThrowIfCancellationRequested();

            if (context.SemanticModel.Compilation.IsValidCompilation(DATABASES_NAMESPACE, SKIP_ATTRIBUTE) == false
                || context.Node is not TypeDeclarationSyntax typeSyntax
                || IsSupportedTypeSyntax(typeSyntax) == false
            )
            {
                return null;
            }

            var semanticModel = context.SemanticModel;
            var symbol = semanticModel.GetDeclaredSymbol(typeSyntax, token);

            if (symbol.GetAttribute(AUTHOR_DATABASE_ATTRIBUTE) is not AttributeData authorAttrib
                || authorAttrib.ConstructorArguments.Length != 1
                || authorAttrib.ConstructorArguments[0].Kind != TypedConstantKind.Type
                || authorAttrib.ConstructorArguments[0].Value is not INamedTypeSymbol databaseSymbol
                || databaseSymbol.GetAttribute(DATABASE_ATTRIBUTE) is not AttributeData databaseAttrib
            )
            {
                return null;
            }

            return new DatabaseRef(
                  typeSyntax
                , symbol
                , databaseSymbol
                , authorAttrib
                , databaseAttrib
            );
        }

        private static bool IsSupportedTypeSyntax(TypeDeclarationSyntax syntax)
            => syntax.IsKind(SyntaxKind.ClassDeclaration) || syntax.IsKind(SyntaxKind.StructDeclaration);

        private static void GenerateOutput(
              SourceProductionContext context
            , CompilationRef compilation
            , ImmutableArray<DatabaseRef> candidates
            , string projectPath
            , bool outputSourceGenFiles
        )
        {
            if (candidates.Length < 1)
            {
                return;
            }

            var objectType = compilation.compilation.GetSpecialType(SpecialType.System_Object);
            var databaseAuthoring = compilation.databaseAuthoring;
            var bakingSheet = compilation.bakingSheet;

            context.CancellationToken.ThrowIfCancellationRequested();

            SourceGenHelpers.ProjectPath = projectPath;
            var assemblyName = compilation.compilation.Assembly.Name;

            foreach (var candidate in candidates)
            {
                try
                {
                    var declaration = new DatabaseDeclaration(context, candidate);
                    var databaseRef = declaration.DatabaseRef;
                    var syntaxTree = candidate.Syntax.SyntaxTree;
                    var databaseIdentifier = candidate.Symbol.ToValidIdentifier();
                    var tables = databaseRef.Tables;

                    if (tables.Length < 1)
                    {
                        continue;
                    }

                    var dataMap = BuildDataMap(context, declaration);
                    var tableAssetRefMap = BuildDataTableAssetRefMap(declaration, dataMap);

                    BuildDataContainers(
                          databaseRef
                        , dataMap
                        , out var assetRefListMap
                        , out var maxFieldOfSameTable
                        , out var typeNames
                    );

                    // SheetContainer
                    if (typeNames.Count > 0)
                    {
                        var containerHintName = GetHintName(
                              syntaxTree
                            , GENERATOR_NAME
                            , candidate.Syntax
                            , $"{databaseIdentifier}__SheetContainer"
                        );

                        var containerSourceFilePath = GetSourceFilePath(
                              containerHintName
                            , assemblyName
                        );

                        var printer = Printer.DefaultLarge;

                        printer.PrintLineIf(databaseAuthoring, DEFINE_DATABASE_AUTHORING, DEFINE_NO_DATABASE_AUTHORING);
                        printer.PrintLineIf(bakingSheet, DEFINE_BAKING_SHEET, DEFINE_NO_BAKING_SHEET);

                        context.OutputSource(
                              outputSourceGenFiles
                            , databaseRef.Syntax
                            , declaration.WriteContainer(assetRefListMap, maxFieldOfSameTable, typeNames)
                            , containerHintName
                            , containerSourceFilePath
                            , printer
                        );
                    }

                    foreach (var tableAssetRef in tableAssetRefMap.Values)
                    {
                        var table = tableAssetRef.Table;
                        var tableType = tableAssetRef.TableType;
                        var dataType = tableAssetRef.DataType;

                        if (dataMap.TryGetValue(dataType, out var dataTypeDeclaration) == false)
                        {
                            continue;
                        }

                        var sheetHintName = GetHintName(
                              syntaxTree
                            , GENERATOR_NAME
                            , candidate.Syntax
                            , $"{databaseIdentifier}__{tableType.Name}__{dataType.Name}Sheet"
                        );

                        var sheetSourceFilePath = GetSourceFilePath(
                              sheetHintName
                            , assemblyName
                        );

                        var printer = Printer.DefaultLarge;

                        printer.PrintLineIf(databaseAuthoring, DEFINE_DATABASE_AUTHORING, DEFINE_NO_DATABASE_AUTHORING);
                        printer.PrintLineIf(bakingSheet, DEFINE_BAKING_SHEET, DEFINE_NO_BAKING_SHEET);

                        context.OutputSource(
                              outputSourceGenFiles
                            , databaseRef.Syntax
                            , table.SyntaxNode
                            , declaration.WriteSheet(tableAssetRef, dataTypeDeclaration, dataMap, objectType)
                            , sheetHintName
                            , sheetSourceFilePath
                            , printer
                        );
                    }
                }
                catch (Exception e)
                {
                    context.ReportDiagnostic(
                          s_errorDescriptor
                        , candidate.AuthorAttribute.ApplicationSyntaxReference.GetSyntax(context.CancellationToken)
                        , e.ToUnityPrintableString()
                    );
                }
            }
        }

        private static string GetHintName(
              SyntaxTree syntaxTree
            , string generatorName
            , SyntaxNode node
            , string typeName
        )
        {
            var (isSuccess, fileName) = syntaxTree.TryGetFileNameWithoutExtension();
            var stableHashCode = SourceGenHelpers.GetStableHashCode(syntaxTree.FilePath) & 0x7fffffff;

            var postfix = generatorName.Length > 0 ? $"__{generatorName}" : string.Empty;

            if (string.IsNullOrWhiteSpace(typeName) == false)
            {
                postfix = $"__{typeName}{postfix}";
            }

            if (isSuccess)
            {
                var salting = node.GetLineNumber();
                fileName = $"{fileName}{postfix}_{stableHashCode}_{salting}.g.cs";
            }
            else
            {
                fileName = Path.Combine($"{Path.GetRandomFileName()}{postfix}", ".g.cs");
            }

            return fileName;
        }

        private static string GetSourceFilePath(string fileName, string assemblyName)
        {
            if (SourceGenHelpers.CanWriteToProjectPath)
            {
                var saveToDirectory = $"{SourceGenHelpers.ProjectPath}/Temp/GeneratedCode/{assemblyName}/";
                Directory.CreateDirectory(saveToDirectory);
                return saveToDirectory + fileName;
            }

            return $"Temp/GeneratedCode/{assemblyName}/{fileName}";
        }

        private static Dictionary<ITypeSymbol, DataDeclaration> BuildDataMap(
              SourceProductionContext context
            , DatabaseDeclaration database
        )
        {
            var tables = database.DatabaseRef.Tables;
            var map = new Dictionary<ITypeSymbol, DataDeclaration>(SymbolEqualityComparer.Default);
            var queue = new Queue<ITypeSymbol>();

            foreach (var table in tables)
            {
                queue.Enqueue(table.IdType);
                queue.Enqueue(table.DataType);

                while (queue.Count > 0)
                {
                    var type = queue.Dequeue();

                    if (type.InheritsFromInterface(IDATA) == false)
                    {
                        continue;
                    }

                    if (map.ContainsKey(type))
                    {
                        continue;
                    }

                    var dataDeclaration = new DataDeclaration(context, database, table, type, true);

                    if (dataDeclaration.PropRefs.Length < 1 && dataDeclaration.FieldRefs.Length < 1)
                    {
                        continue;
                    }

                    map[type] = dataDeclaration;

                    Build(queue, dataDeclaration.PropRefs);
                    Build(queue, dataDeclaration.FieldRefs);

                    var baseTypeRefs = dataDeclaration.BaseTypeRefs;

                    for (var i = 0; i < baseTypeRefs.Length; i++)
                    {
                        var typeRef = baseTypeRefs[i];
                        Build(queue, typeRef.PropRefs);
                        Build(queue, typeRef.FieldRefs);
                    }
                }
            }

            return map;

            static void Build(Queue<ITypeSymbol> queue, ImmutableArray<MemberRef> memberRefs)
            {
                foreach (var memberRef in memberRefs)
                {
                    BuildTypeRef(queue, memberRef.TypeRef);
                    BuildTypeRef(queue, memberRef.ConverterRef.SourceTypeRef);
                }
            }

            static void BuildTypeRef(Queue<ITypeSymbol> queue, TypeRef typeRef)
            {
                var collectionTypeRef = typeRef.CollectionTypeRef;

                if (collectionTypeRef.KeyType != null)
                {
                    queue.Enqueue(collectionTypeRef.KeyType);
                }

                if (collectionTypeRef.ElementType != null)
                {
                    queue.Enqueue(collectionTypeRef.ElementType);
                }

                if (typeRef.Type != null)
                {
                    queue.Enqueue(typeRef.Type);
                }
            }
        }

        private static Dictionary<ITypeSymbol, DataTableAssetRef> BuildDataTableAssetRefMap(
              DatabaseDeclaration database
            , Dictionary<ITypeSymbol, DataDeclaration> dataMap
        )
        {
            var tables = database.DatabaseRef.Tables;
            var map = new Dictionary<ITypeSymbol, DataTableAssetRef>(SymbolEqualityComparer.Default);
            var uniqueTypeNames = new HashSet<ITypeSymbol>(SymbolEqualityComparer.Default);
            var typeQueue = new Queue<DataDeclaration>();

            foreach (var table in tables)
            {
                var type = table.Type;

                if (map.ContainsKey(type) == false)
                {
                    var dataTableAssetRef = new DataTableAssetRef {
                        Table = table,
                    };

                    InitializeDataTableAssetRef(dataTableAssetRef, dataMap, uniqueTypeNames, typeQueue);
                    map[type] = dataTableAssetRef;
                }

                uniqueTypeNames.Clear();
                typeQueue.Clear();
            }

            return map;
        }

        private static void InitializeDataTableAssetRef(
              DataTableAssetRef tableAssetRef
            , Dictionary<ITypeSymbol, DataDeclaration> dataMap
            , HashSet<ITypeSymbol> uniqueTypeNames
            , Queue<DataDeclaration> typeQueue
        )
        {
            var idType = tableAssetRef.IdType;
            var dataType = tableAssetRef.DataType;

            if (dataMap.TryGetValue(idType, out var idDeclaration))
            {
                typeQueue.Enqueue(idDeclaration);
                uniqueTypeNames.Add(idType);
            }

            if (dataMap.TryGetValue(dataType, out var dataDeclaration))
            {
                typeQueue.Enqueue(dataDeclaration);
                uniqueTypeNames.Add(dataType);
            }

            while (typeQueue.Count > 0)
            {
                var declaration = typeQueue.Dequeue();

                TryAddAll(dataMap, uniqueTypeNames, typeQueue, declaration.PropRefs);
                TryAddAll(dataMap, uniqueTypeNames, typeQueue, declaration.FieldRefs);

                var baseTypeRefs = declaration.BaseTypeRefs;

                for (var i = 0; i < baseTypeRefs.Length; i++)
                {
                    var typeRef = baseTypeRefs[i];

                    TryAddAll(dataMap, uniqueTypeNames, typeQueue, typeRef.PropRefs);
                    TryAddAll(dataMap, uniqueTypeNames, typeQueue, typeRef.FieldRefs);
                }
            }

            uniqueTypeNames.Remove(idType);
            uniqueTypeNames.Remove(dataType);

            if (uniqueTypeNames.Count > 0)
            {
                using var arrayBuilder = ImmutableArrayBuilder<ITypeSymbol>.Rent();
                arrayBuilder.AddRange(uniqueTypeNames);
                tableAssetRef.NestedDataTypes = arrayBuilder.ToImmutable();
            }
            else
            {
                tableAssetRef.NestedDataTypes = ImmutableArray<ITypeSymbol>.Empty;
            }

            static void TryAddAll(
                  Dictionary<ITypeSymbol, DataDeclaration> dataMap
                , HashSet<ITypeSymbol> uniqueTypeNames
                , Queue<DataDeclaration> typeQueue
                , ImmutableArray<MemberRef> memberRefs
            )
            {
                foreach (var memberRef in memberRefs)
                {
                    TryAddTypeRef(dataMap, uniqueTypeNames, typeQueue, memberRef.TypeRef);
                    TryAddTypeRef(dataMap, uniqueTypeNames, typeQueue, memberRef.ConverterRef.SourceTypeRef);
                }
            }

            static void TryAddTypeRef(
                  Dictionary<ITypeSymbol, DataDeclaration> dataMap
                , HashSet<ITypeSymbol> uniqueTypeNames
                , Queue<DataDeclaration> typeQueue
                , TypeRef typeRef
            )
            {
                var collectionTypeRef = typeRef.CollectionTypeRef;

                switch (collectionTypeRef.Kind)
                {
                    case CollectionKind.Array:
                    case CollectionKind.List:
                    case CollectionKind.HashSet:
                    case CollectionKind.Queue:
                    case CollectionKind.Stack:
                    {
                        TryAdd(collectionTypeRef.ElementType, dataMap, uniqueTypeNames, typeQueue);
                        break;
                    }

                    case CollectionKind.Dictionary:
                    {
                        TryAdd(collectionTypeRef.KeyType, dataMap, uniqueTypeNames, typeQueue);
                        TryAdd(collectionTypeRef.ElementType, dataMap, uniqueTypeNames, typeQueue);
                        break;
                    }

                    default:
                    {
                        TryAdd(typeRef.Type, dataMap, uniqueTypeNames, typeQueue);
                        break;
                    }
                }
            }

            static void TryAdd(
                  ITypeSymbol typeSymbol
                , Dictionary<ITypeSymbol, DataDeclaration> dataMap
                , HashSet<ITypeSymbol> uniqueTypeNames
                , Queue<DataDeclaration> typeQueue
            )
            {
                if (typeSymbol == null)
                {
                    return;
                }

                if (uniqueTypeNames.Contains(typeSymbol))
                {
                    return;
                }

                if (dataMap.TryGetValue(typeSymbol, out var typeDeclaration))
                {
                    typeQueue.Enqueue(typeDeclaration);
                    uniqueTypeNames.Add(typeSymbol);
                }
            }
        }
        public static void BuildDataContainers(
              DatabaseRef databaseRef
            , Dictionary<ITypeSymbol, DataDeclaration> dataMap
            , out Dictionary<INamedTypeSymbol, DataTableAssetRefList> assetRefListMap
            , out int maxFieldOfSameTable
            , out List<string> typeNames
        )
        {
            var tables = databaseRef.Tables;

            assetRefListMap = new(SymbolEqualityComparer.Default);
            maxFieldOfSameTable = 0;
            typeNames = new(tables.Length);

            foreach (var table in tables)
            {
                var tableType = table.Type;
                var dataType = table.DataType;

                if (dataMap.ContainsKey(dataType) == false)
                {
                    continue;
                }

                var typeName = DatabaseDeclaration.GetUniqueSheetName(table);
                typeNames.Add(typeName);

                if (assetRefListMap.TryGetValue(tableType, out var assetRefList) == false)
                {
                    assetRefListMap[tableType] = assetRefList = new DataTableAssetRefList(dataType);
                }

                var fieldNames = assetRefList.FieldNames;
                fieldNames.Add(table.PropertyName);
                maxFieldOfSameTable = Math.Max(maxFieldOfSameTable, fieldNames.Count);
            }
        }

        private static readonly DiagnosticDescriptor s_errorDescriptor
            = new("DATABASE_UNKNOWN_0001"
                , "Database Generator Error"
                , "This error indicates a bug in the Database source generators. Error message: '{0}'."
                , "DatabaseGenerator"
                , DiagnosticSeverity.Error
                , isEnabledByDefault: true
                , description: ""
            );
    }
}
