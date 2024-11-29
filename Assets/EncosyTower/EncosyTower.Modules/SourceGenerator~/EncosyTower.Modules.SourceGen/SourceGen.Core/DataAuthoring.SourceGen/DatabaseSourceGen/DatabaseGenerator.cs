using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Threading;
using EncosyTower.Modules.Data.SourceGen;
using EncosyTower.Modules.SourceGen;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace EncosyTower.Modules.DataAuthoring.SourceGen
{
    using static EncosyTower.Modules.DataAuthoring.SourceGen.Helpers;

    [Generator]
    public class DatabaseGenerator : IIncrementalGenerator
    {
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            var projectPathProvider = SourceGenHelpers.GetSourceGenConfigProvider(context);

            var databaseRefProvider = context.SyntaxProvider.CreateSyntaxProvider(
                predicate: IsValidDatabaseSyntax,
                transform: GetDatabaseRefSemanticMatch
            ).Where(static t => t is { });

            var combined = databaseRefProvider.Collect()
                .Combine(context.CompilationProvider)
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

            return node is ClassDeclarationSyntax classSyntax
                && classSyntax.AttributeLists.Count > 0
                && classSyntax.HasAttributeCandidate("EncosyTower.Modules.Data.Authoring", "Database");
        }

        public static DatabaseRef GetDatabaseRefSemanticMatch(
              GeneratorSyntaxContext context
            , CancellationToken token
        )
        {
            token.ThrowIfCancellationRequested();

            if (context.SemanticModel.Compilation.IsValidCompilation(SKIP_ATTRIBUTE) == false
                || context.Node is not ClassDeclarationSyntax classSyntax
            )
            {
                return null;
            }

            var semanticModel = context.SemanticModel;
            var symbol = semanticModel.GetDeclaredSymbol(classSyntax, token);
            var attribute = symbol.GetAttribute(DATABASE_ATTRIBUTE);

            if (attribute == null)
            {
                return null;
            }

            return new DatabaseRef(
                  classSyntax
                , symbol
                , attribute
            );
        }

        private static void GenerateOutput(
              SourceProductionContext context
            , Compilation compilation
            , ImmutableArray<DatabaseRef> candidates
            , string projectPath
            , bool outputSourceGenFiles
        )
        {
            if (candidates.Length < 1)
            {
                return;
            }

            var objectType = compilation.GetSpecialType(SpecialType.System_Object);

            context.CancellationToken.ThrowIfCancellationRequested();

            SourceGenHelpers.ProjectPath = projectPath;
            var assemblyName = compilation.Assembly.Name;

            foreach (var candidate in candidates)
            {
                try
                {
                    var declaration = new DatabaseDeclaration(context, candidate);
                    var syntaxTree = candidate.Syntax.SyntaxTree;
                    var databaseIdentifier = candidate.Symbol.ToValidIdentifier();

                    var databaseHintName = syntaxTree.GetGeneratedSourceFileName(
                          GENERATOR_NAME
                        , candidate.Syntax
                        , databaseIdentifier
                    );

                    var databaseSourceFilePath = syntaxTree.GetGeneratedSourceFilePath(
                          assemblyName
                        , GENERATOR_NAME
                    );

                    if (declaration.DatabaseRef.Tables.Length < 1)
                    {
                        OutputSource(
                              context
                            , outputSourceGenFiles
                            , declaration.DatabaseRef.Syntax
                            , declaration.WriteContainer(null, null)
                            , databaseHintName
                            , databaseSourceFilePath
                        );

                        continue;
                    }

                    var dataMap = BuildDataMap(context, declaration);
                    var dataTableAssetRefMap = BuildDataTableAssetRefMap(declaration, dataMap);

                    OutputSource(
                          context
                        , outputSourceGenFiles
                        , declaration.DatabaseRef.Syntax
                        , declaration.WriteContainer(dataTableAssetRefMap, dataMap)
                        , databaseHintName
                        , databaseSourceFilePath
                    );

                    var tables = declaration.DatabaseRef.Tables;

                    foreach (var table in tables)
                    {
                        if (dataTableAssetRefMap.TryGetValue(table.Type, out var dataTableAssetRef) == false)
                        {
                            continue;
                        }

                        if (dataMap.TryGetValue(dataTableAssetRef.DataType, out var dataTypeDeclaration) == false)
                        {
                            continue;
                        }

                        var sheetHintName = GetHintName(
                              syntaxTree
                            , GENERATOR_NAME
                            , candidate.Syntax
                            , $"{databaseIdentifier}_{dataTableAssetRef.Symbol.Name}_{dataTableAssetRef.DataType.Name}Sheet"
                        );

                        var sheetSourceFilePath = GetSourceFilePath(
                              sheetHintName
                            , assemblyName
                        );

                        OutputSource(
                              context
                            , outputSourceGenFiles
                            , declaration.DatabaseRef.Syntax
                            , declaration.WriteSheet(table, dataTableAssetRef, dataTypeDeclaration, dataMap, objectType)
                            , sheetHintName
                            , sheetSourceFilePath
                        );
                    }
                }
                catch (Exception e)
                {
                    context.ReportDiagnostic(
                          s_errorDescriptor
                        , candidate.Attribute.ApplicationSyntaxReference.GetSyntax(context.CancellationToken)
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

            fileName = $"{fileName}_";

            if (isSuccess)
            {
                var salting = node.GetLocation().GetLineSpan().StartLinePosition.Line;
                fileName = $"{fileName}{postfix}_{stableHashCode}{salting}.g.cs";
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

            return $"Temp/GeneratedCode/{assemblyName}";
        }

        private static void OutputSource(
              SourceProductionContext context
            , bool outputSourceGenFiles
            , SyntaxNode syntax
            , string source
            , string hintName
            , string sourceFilePath
        )
        {
            var outputSource = TypeCreationHelpers.GenerateSourceTextForRootNodes(
                  sourceFilePath
                , syntax
                , source
                , context.CancellationToken
            );

            context.AddSource(hintName, outputSource);

            if (outputSourceGenFiles)
            {
                SourceGenHelpers.OutputSourceToFile(
                      context
                    , syntax.GetLocation()
                    , sourceFilePath
                    , outputSource
                );
            }
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
                var args = table.BaseType.TypeArguments;
                queue.Enqueue(args[0]);
                queue.Enqueue(args[1]);

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
                        Symbol = table.Type,
                        IdType = table.BaseType.TypeArguments[0],
                        DataType = table.BaseType.TypeArguments[1],
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
              DataTableAssetRef dataTableAssetRef
            , Dictionary<ITypeSymbol, DataDeclaration> dataMap
            , HashSet<ITypeSymbol> uniqueTypeNames
            , Queue<DataDeclaration> typeQueue
        )
        {
            var idType = dataTableAssetRef.IdType;
            var dataType = dataTableAssetRef.DataType;

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
                dataTableAssetRef.NestedDataTypes = arrayBuilder.ToImmutable();
            }
            else
            {
                dataTableAssetRef.NestedDataTypes = ImmutableArray<ITypeSymbol>.Empty;
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
