using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Threading;
using EncosyTower.SourceGen.Common.Data.Common;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Newtonsoft.Json.Utilities;

namespace EncosyTower.SourceGen.Generators.DatabaseAuthoring
{
    using static EncosyTower.SourceGen.Generators.DatabaseAuthoring.Helpers;

    partial struct DatabaseModel
    {
        private const string GENERATOR_NAME = DatabaseAuthoringGenerator.GENERATOR_NAME;

        public static DatabaseModel Extract(GeneratorAttributeSyntaxContext context, CancellationToken token)
        {
            if (context.TargetNode is not TypeDeclarationSyntax typeSyntax
                || IsSupportedTypeSyntax(typeSyntax) == false
                || context.TargetSymbol is not INamedTypeSymbol authoringSymbol
            )
            {
                return default;
            }

            var compilation = context.SemanticModel.Compilation;

            if (compilation.IsValidCompilation(DATABASES_NAMESPACE, SKIP_ATTRIBUTE) == false)
            {
                return default;
            }

            // ForAttributeWithMetadataName guarantees at least one matching attribute
            var authorAttrib = context.Attributes[0];

            if (authorAttrib.ConstructorArguments.Length != 1
                || authorAttrib.ConstructorArguments[0].Kind != TypedConstantKind.Type
                || authorAttrib.ConstructorArguments[0].Value is not INamedTypeSymbol databaseSymbol
                || databaseSymbol.GetAttribute(DATABASE_ATTRIBUTE) is not AttributeData databaseAttrib
            )
            {
                return default;
            }

            var databaseTypeName = typeSyntax.Identifier.Text;
            var databaseTypeKeyword = authoringSymbol.IsValueType ? "struct" : "class";
            var databaseIdentifier = authoringSymbol.ToValidIdentifier();

            // Pre-compute opening/closing source (namespace/outer-type scope wrapper)
            TypeCreationHelpers.GenerateOpeningAndClosingSource(
                  typeSyntax
                , token
                , out var openingSource
                , out var closingSource
                , printAdditionalUsings: PrintAdditionalUsings
            );

            var syntaxTree = typeSyntax.SyntaxTree;

            // Hint name for SheetContainer
            var containerHintName = GetHintName(
                  syntaxTree
                , GENERATOR_NAME
                , typeSyntax
                , $"{databaseIdentifier}__SheetContainer"
            );

            // --- NamingStrategy ---
            var namingStrategy = NamingStrategy.PascalCase;

            foreach (var arg in databaseAttrib.ConstructorArguments)
            {
                if (arg.Kind != TypedConstantKind.Array && arg.Value != null)
                {
                    namingStrategy = arg.Value.ToNamingStrategy();
                    break;
                }
            }

            // --- Database-level converter map (keyed by target type full name) ---
            var dbConverterMap = new Dictionary<string, ConverterModel>(StringComparer.Ordinal);

            foreach (var arg in databaseAttrib.ConstructorArguments)
            {
                if (arg.Kind == TypedConstantKind.Array)
                {
                    BuildConverterMap(arg.Values, dbConverterMap);
                    break;
                }
            }

            // --- Tables ---
            var tableModelList = new List<TableModel>();
            // Also need intermediate structs to carry IdType/DataType for BFS
            var tableInfoList = new List<TableInfo>();
            // HorizontalListMap: targetTypeFullName -> containingTypeFullName -> HashSet<propertyName>
            var horizontalListMap = new Dictionary<string, Dictionary<string, HashSet<string>>>(StringComparer.Ordinal);

            var dbMembers = databaseSymbol.GetMembers();

            foreach (var member in dbMembers)
            {
                if (member is not IPropertySymbol property)
                {
                    continue;
                }

                if (property.Type is not INamedTypeSymbol tableType)
                {
                    continue;
                }

                var tableAttrib = member.GetAttribute(TABLE_ATTRIBUTE);

                if (tableAttrib == null)
                {
                    continue;
                }

                if (tableType.BaseType == null
                    || tableType.TryGetGenericType(DATA_TABLE_ASSET, 3, 2, out var baseType) == false
                )
                {
                    continue;
                }

                var tableNamingStrategy = namingStrategy;
                var tableConverterMap = new Dictionary<string, ConverterModel>(StringComparer.Ordinal);

                foreach (var arg in tableAttrib.ConstructorArguments)
                {
                    if (arg.Kind != TypedConstantKind.Array && arg.Value != null)
                    {
                        tableNamingStrategy = arg.Value.ToNamingStrategy();
                    }
                    else if (arg.Kind == TypedConstantKind.Array)
                    {
                        BuildConverterMap(arg.Values, tableConverterMap, offset: 2);
                    }
                }

                var idType = baseType.TypeArguments[0];
                var dataType = baseType.TypeArguments[1];
                var idTypeFullName = idType.ToFullName();
                var dataTypeFullName = dataType.ToFullName();
                var tableTypeName = tableType.Name;
                var dataTypeSimpleName = dataType is INamedTypeSymbol ndt ? ndt.Name : dataType.Name;
                var baseSheetName = $"{tableTypeName}_{dataTypeSimpleName}Sheet";
                var uniqueSheetName = $"{baseSheetName}__{property.Name}";
                var assetName = $"{tableTypeName}_{property.Name}".ToNamingCase(tableNamingStrategy);

                tableModelList.Add(new TableModel {
                    typeFullName      = tableType.ToFullName(),
                    typeSimpleName    = tableTypeName,
                    idTypeFullName    = idTypeFullName,
                    dataTypeFullName  = dataTypeFullName,
                    propertyName      = property.Name,
                    namingStrategy    = tableNamingStrategy,
                    assetName         = assetName,
                    uniqueSheetName   = uniqueSheetName,
                    baseSheetName     = baseSheetName,
                });

                tableInfoList.Add(new TableInfo {
                    tableType         = tableType,
                    idType            = idType,
                    dataType          = dataType,
                    idTypeFullName    = idTypeFullName,
                    dataTypeFullName  = dataTypeFullName,
                    tableTypeFullName = tableType.ToFullName(),
                    tableTypeSimpleName = tableTypeName,
                    dataTypeSimpleName = dataTypeSimpleName,
                    propertyName      = property.Name,
                    namingStrategy    = tableNamingStrategy,
                    tableConverterMap = tableConverterMap,
                });

                // Horizontal list entries
                CollectHorizontalLists(member, tableType, horizontalListMap);
            }

            if (tableModelList.Count < 1)
            {
                return default;
            }

            // --- DataMap (BFS over all IData types) ---
            var dataMap = BuildDataMap(tableInfoList, dbConverterMap);

            // --- Asset ref lists & type names (BuildDataContainers equivalent) ---
            var assetRefListDict = new Dictionary<string, AssetRefListModel>(StringComparer.Ordinal);
            var assetRefListOrder = new List<string>();
            var typeNameList = new List<string>();
            var maxFieldOfSameTable = 0;

            foreach (var tableInfo in tableInfoList)
            {
                var dataTypeFullName = tableInfo.dataTypeFullName;

                if (dataMap.ContainsKey(dataTypeFullName) == false)
                {
                    continue;
                }

                var uniqueSheetNameForType = $"{tableInfo.tableTypeSimpleName}_{tableInfo.dataTypeSimpleName}Sheet__{tableInfo.propertyName}";
                typeNameList.Add(uniqueSheetNameForType);

                var tableTypeKey = tableInfo.tableTypeFullName;

                if (assetRefListDict.TryGetValue(tableTypeKey, out var existing) == false)
                {
                    var newRef = new AssetRefListModel {
                        tableTypeFullName  = tableInfo.tableTypeFullName,
                        tableTypeSimpleName = tableInfo.tableTypeSimpleName,
                        dataTypeFullName   = tableInfo.dataTypeFullName,
                        dataTypeSimpleName = tableInfo.dataTypeSimpleName,
                        fieldNames         = default,
                    };

                    assetRefListDict[tableTypeKey] = newRef;
                    assetRefListOrder.Add(tableTypeKey);
                    existing = newRef;
                }

                // Build a temporary list then convert
                var currentFieldNames = new List<string>();

                if (existing.fieldNames.Count > 0)
                {
                    foreach (var fn in existing.fieldNames)
                    {
                        currentFieldNames.Add(fn);
                    }
                }

                currentFieldNames.Add(tableInfo.propertyName);

                existing.fieldNames = currentFieldNames.ToImmutableArray().AsEquatableArray();
                assetRefListDict[tableTypeKey] = existing;

                maxFieldOfSameTable = Math.Max(maxFieldOfSameTable, currentFieldNames.Count);
            }

            // Convert horizontal list map to EquatableArray<HorizontalListEntry>
            using var hlBuilder = ImmutableArrayBuilder<HorizontalListEntry>.Rent();

            foreach (var targetKv in horizontalListMap)
            {
                foreach (var containingKv in targetKv.Value)
                {
                    using var propBuilder = ImmutableArrayBuilder<string>.Rent();

                    foreach (var propName in containingKv.Value)
                    {
                        propBuilder.Add(propName);
                    }

                    hlBuilder.Add(new HorizontalListEntry {
                        targetTypeFullName    = targetKv.Key,
                        containingTypeFullName = containingKv.Key,
                        propertyNames         = propBuilder.ToImmutable().AsEquatableArray(),
                    });
                }
            }

            // --- Sheets ---
            // Build set of unique table types (per tableType, only first table is used for the sheet)
            var processedTableTypes = new HashSet<string>(StringComparer.Ordinal);
            var sheetList = new List<SheetModel>();

            foreach (var tableInfo in tableInfoList)
            {
                var tableTypeKey = tableInfo.tableTypeFullName;
                var dataTypeFullName = tableInfo.dataTypeFullName;

                if (dataMap.ContainsKey(dataTypeFullName) == false)
                {
                    continue;
                }

                if (processedTableTypes.Contains(tableTypeKey))
                {
                    continue;
                }

                processedTableTypes.Add(tableTypeKey);

                var sheetName = $"{tableInfo.tableTypeSimpleName}_{tableInfo.dataTypeSimpleName}Sheet";

                var sheetHintName = GetHintName(
                      syntaxTree
                    , GENERATOR_NAME
                    , typeSyntax
                    , $"{databaseIdentifier}__{tableInfo.tableTypeSimpleName}__{tableInfo.dataTypeSimpleName}Sheet"
                );

                // Nested data type full names
                var nestedFullNames = BuildNestedDataTypeFullNames(tableInfo, dataMap);

                sheetList.Add(new SheetModel {
                    hintName              = sheetHintName,
                    idTypeFullName        = tableInfo.idTypeFullName,
                    idTypeSimpleName      = tableInfo.idType is INamedTypeSymbol nid ? nid.Name : tableInfo.idType.Name,
                    dataTypeFullName      = dataTypeFullName,
                    dataTypeSimpleName    = tableInfo.dataTypeSimpleName,
                    tableTypeFullName     = tableInfo.tableTypeFullName,
                    sheetName             = sheetName,
                    nestedDataTypeFullNames = nestedFullNames,
                });
            }

            if (sheetList.Count < 1)
            {
                return default;
            }

            // Convert dataMap to EquatableArray<DataModel>
            using var dataModelsBuilder = ImmutableArrayBuilder<DataModel>.Rent();

            foreach (var dm in dataMap.Values)
            {
                dataModelsBuilder.Add(dm);
            }

            // Convert tableModels
            using var tableModelsBuilder = ImmutableArrayBuilder<TableModel>.Rent();

            foreach (var tm in tableModelList)
            {
                tableModelsBuilder.Add(tm);
            }

            // Convert assetRefLists (preserve insertion order)
            using var assetRefListBuilder = ImmutableArrayBuilder<AssetRefListModel>.Rent();

            foreach (var key in assetRefListOrder)
            {
                assetRefListBuilder.Add(assetRefListDict[key]);
            }

            // Convert typeNames
            using var typeNamesBuilder = ImmutableArrayBuilder<string>.Rent();

            foreach (var tn in typeNameList)
            {
                typeNamesBuilder.Add(tn);
            }

            // Convert sheets
            using var sheetsBuilder = ImmutableArrayBuilder<SheetModel>.Rent();

            foreach (var s in sheetList)
            {
                sheetsBuilder.Add(s);
            }

            return new DatabaseModel {
                location               = LocationInfo.From(typeSyntax.GetLocation()),
                databaseTypeName       = databaseTypeName,
                databaseTypeKeyword    = databaseTypeKeyword,
                databaseIdentifier     = databaseIdentifier,
                openingSource          = openingSource,
                closingSource          = closingSource,
                containerHintName      = containerHintName,
                allDataModels          = dataModelsBuilder.ToImmutable().AsEquatableArray(),
                horizontalListEntries  = hlBuilder.ToImmutable().AsEquatableArray(),
                tables                 = tableModelsBuilder.ToImmutable().AsEquatableArray(),
                assetRefLists          = assetRefListBuilder.ToImmutable().AsEquatableArray(),
                typeNames              = typeNamesBuilder.ToImmutable().AsEquatableArray(),
                maxFieldOfSameTable    = maxFieldOfSameTable,
                sheets                 = sheetsBuilder.ToImmutable().AsEquatableArray(),
            };
        }

        // ── Internal extraction helpers ────────────────────────────────────────────

        private static bool IsSupportedTypeSyntax(TypeDeclarationSyntax syntax)
            => syntax.IsKind(SyntaxKind.ClassDeclaration) || syntax.IsKind(SyntaxKind.StructDeclaration);

        private static string GetHintName(SyntaxTree syntaxTree, string generatorName, SyntaxNode node, string typeName)
        {
            var (isSuccess, fileName) = syntaxTree.TryGetFileNameWithoutExtension();
            var stableHashCode = SourceGenHelpers.GetStableHashCode(syntaxTree.FilePath) & 0x7fffffff;
            //var postfix = generatorName.Length > 0 ? $"__{generatorName}" : string.Empty;
            var postfix = string.Empty;

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

        private static void CollectHorizontalLists(
              ISymbol member
            , INamedTypeSymbol tableType
            , Dictionary<string, Dictionary<string, HashSet<string>>> horizontalListMap
        )
        {
            var attributes = member.GetAttributes(HORIZONTAL_LIST_ATTRIBUTE);

            foreach (var attrib in attributes)
            {
                var args = attrib.ConstructorArguments;

                if (args.Length < 2)
                {
                    continue;
                }

                if (args[0].Value is not INamedTypeSymbol targetType)
                {
                    continue;
                }

                if (args[1].Value is not string propertyName || string.IsNullOrWhiteSpace(propertyName))
                {
                    continue;
                }

                var targetKey = targetType.ToFullName();
                var containingKey = tableType.ToFullName();

                if (horizontalListMap.TryGetValue(targetKey, out var innerMap) == false)
                {
                    horizontalListMap[targetKey] = innerMap = new(StringComparer.Ordinal);
                }

                if (innerMap.TryGetValue(containingKey, out var propertyNames) == false)
                {
                    innerMap[containingKey] = propertyNames = new(StringComparer.Ordinal);
                }

                propertyNames.Add(propertyName);
            }
        }

        private static Dictionary<string, DataModel> BuildDataMap(
              List<TableInfo> tableInfoList
            , Dictionary<string, ConverterModel> dbConverterMap
        )
        {
            var map = new Dictionary<string, DataModel>(StringComparer.Ordinal);
            var queue = new Queue<ITypeSymbol>();

            foreach (var tableInfo in tableInfoList)
            {
                queue.Enqueue(tableInfo.idType);
                queue.Enqueue(tableInfo.dataType);

                while (queue.Count > 0)
                {
                    var type = queue.Dequeue();

                    if (IsIData(type) == false)
                    {
                        continue;
                    }

                    var typeFullName = type.ToFullName();

                    if (map.ContainsKey(typeFullName))
                    {
                        continue;
                    }

                    var dataModel = ExtractDataModel(
                          type
                        , dbConverterMap
                        , tableInfo.tableConverterMap
                        , buildBaseTypeLayers: true
                    );

                    if (dataModel.propRefs.Count < 1
                        && dataModel.fieldRefs.Count < 1
                        && dataModel.baseTypeLayers.Count < 1
                    )
                    {
                        continue;
                    }

                    map[typeFullName] = dataModel;
                }
            }

            return map;
        }

        private static DataModel ExtractDataModel(
              ITypeSymbol symbol
            , Dictionary<string, ConverterModel> dbConverterMap
            , Dictionary<string, ConverterModel> tableConverterMap
            , bool buildBaseTypeLayers
        )
        {
            var (propRefs, fieldRefs) = ExtractMemberModels(symbol, dbConverterMap, tableConverterMap);

            using var baseLayerBuilder = ImmutableArrayBuilder<DataModelLayer>.Rent();

            if (buildBaseTypeLayers)
            {
                var baseSymbol = (symbol as INamedTypeSymbol)?.BaseType;

                while (baseSymbol != null)
                {
                    if (baseSymbol.TypeKind != TypeKind.Class || IsIData(baseSymbol) == false)
                    {
                        break;
                    }

                    var baseMembers = ExtractMemberModels(baseSymbol, dbConverterMap, tableConverterMap);

                    baseLayerBuilder.Add(new DataModelLayer {
                        fullName       = baseSymbol.ToFullName(),
                        simpleName     = baseSymbol.Name,
                        validIdentifier = baseSymbol.ToValidIdentifier(),
                        propRefs       = baseMembers.propRefs,
                        fieldRefs      = baseMembers.fieldRefs,
                    });

                    baseSymbol = baseSymbol.BaseType;
                }
            }

            // Reverse so outermost base is first (matching original BaseTypeRefs order)
            var rawLayers = baseLayerBuilder.ToImmutable();
            using var reversedLayers = ImmutableArrayBuilder<DataModelLayer>.Rent();

            for (var i = rawLayers.Length - 1; i >= 0; i--)
            {
                reversedLayers.Add(rawLayers[i]);
            }

            return new DataModel {
                fullName        = symbol.ToFullName(),
                simpleName      = symbol.Name,
                validIdentifier = symbol.ToValidIdentifier(),
                propRefs        = propRefs,
                fieldRefs       = fieldRefs,
                baseTypeLayers  = reversedLayers.ToImmutable().AsEquatableArray(),
            };
        }

        private static (EquatableArray<MemberModel> propRefs, EquatableArray<MemberModel> fieldRefs) ExtractMemberModels(
              ITypeSymbol symbol
            , Dictionary<string, ConverterModel> dbConverterMap
            , Dictionary<string, ConverterModel> tableConverterMap
        )
        {
            var properties = new List<(string propertyName, string fieldName, ISymbol member, ITypeSymbol fieldType)>();
            var fields = new List<(string propertyName, string fieldName, ISymbol member, ITypeSymbol fieldType)>();

            var members = symbol.GetMembers();

            foreach (var member in members)
            {
                if (member is IPropertySymbol property)
                {
                    // For types in another assembly
                    var genPropAttr = property.GetAttribute(GENERATED_PROPERTY_FROM_FIELD);
                    if (genPropAttr != null
                        && genPropAttr.ConstructorArguments.Length > 1
                        && genPropAttr.ConstructorArguments[0].Value is string fieldName
                        && genPropAttr.ConstructorArguments[1].Value is ITypeSymbol genFieldType
                    )
                    {
                        properties.Add((property.Name, fieldName, property, genFieldType));
                        continue;
                    }

                    // For types in the same assembly
                    var dataPropAttr = property.GetAttribute(DATA_PROPERTY_ATTRIBUTE);
                    if (dataPropAttr != null)
                    {
                        ITypeSymbol fieldType;
                        if (dataPropAttr.ConstructorArguments.Length < 1
                            || dataPropAttr.ConstructorArguments[0].Value is not ITypeSymbol ft)
                        {
                            fieldType = property.Type;
                        }
                        else
                        {
                            fieldType = ft;
                        }

                        fields.Add((property.Name, property.ToPrivateFieldName(), property, fieldType));
                        continue;
                    }
                }
                else if (member is IFieldSymbol field)
                {
                    // For types in another assembly
                    var genFieldAttr = field.GetAttribute(GENERATED_FIELD_FROM_PROPERTY);
                    if (genFieldAttr != null
                        && genFieldAttr.ConstructorArguments.Length > 0
                        && genFieldAttr.ConstructorArguments[0].Value is string propName
                    )
                    {
                        fields.Add((propName, field.Name, field, field.Type));
                        continue;
                    }

                    // For types in the same assembly
                    if (field.HasAttribute(SERIALIZE_FIELD_ATTRIBUTE))
                    {
                        fields.Add((field.ToPropertyName(), field.Name, field, field.Type));
                        continue;
                    }
                }
            }

            using var propBuilder = ImmutableArrayBuilder<MemberModel>.Rent();
            using var fieldBuilder = ImmutableArrayBuilder<MemberModel>.Rent();

            var uniqueFieldNames = new HashSet<string>(StringComparer.Ordinal);

            foreach (var (propertyName, fieldName, member, fieldType) in properties)
            {
                if (uniqueFieldNames.Contains(fieldName))
                {
                    continue;
                }

                uniqueFieldNames.Add(fieldName);
                propBuilder.Add(BuildMemberModel(propertyName, member, fieldType, dbConverterMap, tableConverterMap));
            }

            foreach (var (propertyName, fieldName, member, fieldType) in fields)
            {
                if (uniqueFieldNames.Contains(fieldName))
                {
                    continue;
                }

                uniqueFieldNames.Add(fieldName);
                fieldBuilder.Add(BuildMemberModel(propertyName, member, fieldType, dbConverterMap, tableConverterMap));
            }

            return (
                propBuilder.ToImmutable().AsEquatableArray(),
                fieldBuilder.ToImmutable().AsEquatableArray()
            );
        }

        private static MemberModel BuildMemberModel(
              string propertyName
            , ISymbol memberSymbol
            , ITypeSymbol fieldType
            , Dictionary<string, ConverterModel> dbConverterMap
            , Dictionary<string, ConverterModel> tableConverterMap
        )
        {
            var collection = MakeCollectionModel(fieldType);
            var typeHasParameterlessCtor = CheckParameterlessConstructor(fieldType);

            // Try [DataConverter] attribute on the member
            var converter = TryMakeConverterModel(memberSymbol, fieldType);

            // Fall back to table-level then database-level converter map
            if (converter.kind == ConverterKind.None)
            {
                var fieldTypeFull = fieldType.ToFullName();

                if (tableConverterMap.TryGetValue(fieldTypeFull, out var tableConv))
                {
                    converter = tableConv;
                }
                else if (dbConverterMap.TryGetValue(fieldTypeFull, out var dbConv))
                {
                    converter = dbConv;
                }
            }

            // Fall back to the type's own Convert method
            if (converter.kind == ConverterKind.None)
            {
                converter = TryFallbackConverterModel(fieldType);
            }

            return new MemberModel {
                propertyName                   = propertyName,
                typeFullName                   = fieldType.ToFullName(),
                typeSimpleName                 = fieldType.Name,
                typeIsValueType                = fieldType.IsValueType,
                typeHasParameterlessConstructor = typeHasParameterlessCtor,
                collection                     = collection,
                converter                      = converter,
            };
        }

        private static bool CheckParameterlessConstructor(ITypeSymbol type)
        {
            if (type is not INamedTypeSymbol namedType)
            {
                return false;
            }

            if (namedType.IsValueType)
            {
                return true;
            }

            if (namedType.IsAbstract)
            {
                return false;
            }

            bool? found = null;
            var paramCtorCount = 0;

            foreach (var typeMember in namedType.GetMembers())
            {
                if (typeMember is IMethodSymbol method && method.MethodKind == MethodKind.Constructor)
                {
                    if (method.Parameters.Length == 0)
                    {
                        found = true;
                        break;
                    }
                    else
                    {
                        paramCtorCount++;
                    }
                }
            }

            if (found.HasValue)
            {
                return found.Value;
            }

            return paramCtorCount == 0;
        }

        private static CollectionModel MakeCollectionModel(ITypeSymbol type)
        {
            if (type is IArrayTypeSymbol arrayType)
            {
                return new CollectionModel {
                    kind = CollectionKind.Array,
                    elementTypeName = arrayType.ElementType.ToFullName(),
                    elementTypeSimpleName = arrayType.ElementType.Name,
                };
            }

            if (type is INamedTypeSymbol namedType)
            {
                if (namedType.TryGetGenericType(LIST_FAST_TYPE_T, 1, out var listFastType))
                {
                    return MakeListModel(listFastType.TypeArguments[0]);
                }

                if (namedType.TryGetGenericType(LIST_TYPE_T, 1, out var listType))
                {
                    return MakeListModel(listType.TypeArguments[0]);
                }

                if (namedType.TryGetGenericType(DICTIONARY_TYPE_T, 2, out var dictType))
                {
                    return MakeDictModel(dictType.TypeArguments[0], dictType.TypeArguments[1]);
                }

                if (namedType.TryGetGenericType(HASH_SET_TYPE_T, 1, out var hashSetType))
                {
                    return MakeKindModel(CollectionKind.HashSet, hashSetType.TypeArguments[0]);
                }

                if (namedType.TryGetGenericType(QUEUE_TYPE_T, 1, out var queueType))
                {
                    return MakeKindModel(CollectionKind.Queue, queueType.TypeArguments[0]);
                }

                if (namedType.TryGetGenericType(STACK_TYPE_T, 1, out var stackType))
                {
                    return MakeKindModel(CollectionKind.Stack, stackType.TypeArguments[0]);
                }

                if (namedType.TryGetGenericType(READONLY_MEMORY_TYPE_T, 1, out var readMemType))
                {
                    return MakeKindModel(CollectionKind.Array, readMemType.TypeArguments[0]);
                }

                if (namedType.TryGetGenericType(MEMORY_TYPE_T, 1, out var memType))
                {
                    return MakeKindModel(CollectionKind.Array, memType.TypeArguments[0]);
                }

                if (namedType.TryGetGenericType(READONLY_SPAN_TYPE_T, 1, out var readSpanType))
                {
                    return MakeKindModel(CollectionKind.Array, readSpanType.TypeArguments[0]);
                }

                if (namedType.TryGetGenericType(SPAN_TYPE_T, 1, out var spanType))
                {
                    return MakeKindModel(CollectionKind.Array, spanType.TypeArguments[0]);
                }

                if (namedType.TryGetGenericType(IREADONLY_LIST_TYPE_T, 1, out var roListType))
                {
                    return MakeListModel(roListType.TypeArguments[0]);
                }

                if (namedType.TryGetGenericType(ILIST_TYPE_T, 1, out var ilistType))
                {
                    return MakeListModel(ilistType.TypeArguments[0]);
                }

                if (namedType.TryGetGenericType(ISET_TYPE_T, 1, out var isetType))
                {
                    return MakeKindModel(CollectionKind.HashSet, isetType.TypeArguments[0]);
                }

                if (namedType.TryGetGenericType(IREADONLY_DICTIONARY_TYPE_T, 2, out var roDictType))
                {
                    return MakeDictModel(roDictType.TypeArguments[0], roDictType.TypeArguments[1]);
                }

                if (namedType.TryGetGenericType(IDICTIONARY_TYPE_T, 2, out var idictType))
                {
                    return MakeDictModel(idictType.TypeArguments[0], idictType.TypeArguments[1]);
                }
            }

            return default;

            static CollectionModel MakeListModel(ITypeSymbol elem)
                => new() { kind = CollectionKind.List, elementTypeName = elem.ToFullName(), elementTypeSimpleName = elem.Name };

            static CollectionModel MakeKindModel(CollectionKind k, ITypeSymbol elem)
                => new() { kind = k, elementTypeName = elem.ToFullName(), elementTypeSimpleName = elem.Name };

            static CollectionModel MakeDictModel(ITypeSymbol key, ITypeSymbol elem)
                => new() {
                    kind = CollectionKind.Dictionary,
                    keyTypeName = key.ToFullName(), keyTypeSimpleName = key.Name,
                    elementTypeName = elem.ToFullName(), elementTypeSimpleName = elem.Name,
                };
        }

        private static ConverterModel TryMakeConverterModel(ISymbol memberSymbol, ITypeSymbol targetType)
        {
            var attrib = memberSymbol.GetAttribute(DATA_CONVERTER_ATTRIBUTE);

            if (attrib == null || attrib.ConstructorArguments.Length != 1)
            {
                return default;
            }

            if (attrib.ConstructorArguments[0].Value is not INamedTypeSymbol converterType)
            {
                return default;
            }

            if (TryGetConvertMethod(converterType, out var convertMethod, targetType) == false)
            {
                return default;
            }

            return MakeConverterModelFromMethod(converterType, convertMethod);
        }

        private static ConverterModel TryFallbackConverterModel(ITypeSymbol fieldType)
        {
            if (fieldType is not INamedTypeSymbol namedReturn)
            {
                return default;
            }

            if (TryGetConvertMethod(namedReturn, out var convertMethod, namedReturn) == false)
            {
                return default;
            }

            return MakeConverterModelFromMethod(namedReturn, convertMethod);
        }

        private static ConverterModel MakeConverterModelFromMethod(
              INamedTypeSymbol converterType
            , IMethodSymbol convertMethod
        )
        {
            var sourceType = convertMethod.Parameters[0].Type;
            var sourceCollection = MakeCollectionModel(sourceType);
            var sourceHasParameterlessCtor = CheckParameterlessConstructor(sourceType);

            return new ConverterModel {
                kind                              = convertMethod.IsStatic ? ConverterKind.Static : ConverterKind.Instance,
                converterTypeFullName             = converterType.ToFullName(),
                sourceCollection                  = sourceCollection,
                sourceTypeFullName                = sourceType.ToFullName(),
                sourceTypeSimpleName              = sourceType.Name,
                sourceTypeIsValueType             = sourceType.IsValueType,
                sourceTypeHasParameterlessConstructor = sourceHasParameterlessCtor,
            };
        }

        private static bool TryGetConvertMethod(
              INamedTypeSymbol converterType
            , out IMethodSymbol convertMethod
            , ITypeSymbol returnType = null
        )
        {
            if (converterType.IsAbstract || converterType.IsUnboundGenericType)
            {
                convertMethod = null;
                return false;
            }

            if (converterType.IsValueType == false)
            {
                var hasPublicParameterlessCtor = false;
                foreach (var ctor in converterType.GetMembers(".ctor"))
                {
                    if (ctor is IMethodSymbol m
                        && m.DeclaredAccessibility == Accessibility.Public
                        && m.Parameters.Length == 0)
                    {
                        hasPublicParameterlessCtor = true;
                        break;
                    }
                }

                if (hasPublicParameterlessCtor == false)
                {
                    convertMethod = null;
                    return false;
                }
            }

            IMethodSymbol staticMethod = null;
            IMethodSymbol instanceMethod = null;
            var multipleStatic = false;
            var multipleInstance = false;

            foreach (var member in converterType.GetMembers("Convert"))
            {
                if (member is not IMethodSymbol method
                    || method.IsGenericMethod
                    || method.DeclaredAccessibility != Accessibility.Public)
                {
                    continue;
                }

                if (method.IsStatic)
                {
                    if (multipleStatic == false)
                    {
                        if (staticMethod != null)
                        {
                            staticMethod = null;
                            multipleStatic = true;
                        }
                        else
                        {
                            staticMethod = method;
                        }
                    }
                }
                else
                {
                    if (multipleInstance == false)
                    {
                        if (instanceMethod != null)
                        {
                            instanceMethod = null;
                            multipleInstance = true;
                        }
                        else
                        {
                            instanceMethod = method;
                        }
                    }
                }
            }

            if (multipleStatic || (multipleStatic == false && multipleInstance))
            {
                convertMethod = null;
                return false;
            }

            convertMethod = staticMethod ?? instanceMethod;

            if (convertMethod == null)
            {
                return false;
            }

            if (convertMethod.Parameters.Length != 1
                || convertMethod.ReturnsVoid
                || (returnType != null && SymbolEqualityComparer.Default.Equals(convertMethod.ReturnType, returnType) == false)
            )
            {
                convertMethod = null;
                return false;
            }

            return true;
        }

        private static void BuildConverterMap(
              ImmutableArray<TypedConstant> values
            , Dictionary<string, ConverterModel> converterMap
            , int offset = 0
        )
        {
            if (values.IsDefaultOrEmpty)
            {
                return;
            }

            for (var i = 0; i < values.Length; i++)
            {
                if (values[i].Value is not INamedTypeSymbol type)
                {
                    continue;
                }

                if (TryGetConvertMethod(type, out var convertMethod) == false)
                {
                    continue;
                }

                var targetTypeFullName = convertMethod.ReturnType.ToFullName();

                if (converterMap.ContainsKey(targetTypeFullName) == false)
                {
                    converterMap[targetTypeFullName] = MakeConverterModelFromMethod(type, convertMethod);
                }
            }
        }

        private static EquatableArray<string> BuildNestedDataTypeFullNames(
              TableInfo tableInfo
            , Dictionary<string, DataModel> dataMap
        )
        {
            var uniqueTypes = new HashSet<string>(StringComparer.Ordinal);
            var typeQueue = new Queue<string>();

            var idKey = tableInfo.idTypeFullName;
            var dataKey = tableInfo.dataTypeFullName;

            if (dataMap.ContainsKey(idKey))
            {
                typeQueue.Enqueue(idKey);
                uniqueTypes.Add(idKey);
            }

            if (dataMap.ContainsKey(dataKey))
            {
                typeQueue.Enqueue(dataKey);
                uniqueTypes.Add(dataKey);
            }

            while (typeQueue.Count > 0)
            {
                var key = typeQueue.Dequeue();

                if (dataMap.TryGetValue(key, out var dm) == false)
                {
                    continue;
                }

                TryEnqueueMembers(dm.propRefs, dataMap, uniqueTypes, typeQueue);
                TryEnqueueMembers(dm.fieldRefs, dataMap, uniqueTypes, typeQueue);

                foreach (var layer in dm.baseTypeLayers)
                {
                    TryEnqueueMembers(layer.propRefs, dataMap, uniqueTypes, typeQueue);
                    TryEnqueueMembers(layer.fieldRefs, dataMap, uniqueTypes, typeQueue);
                }
            }

            uniqueTypes.Remove(idKey);
            uniqueTypes.Remove(dataKey);

            if (uniqueTypes.Count < 1)
            {
                return default;
            }

            using var builder = ImmutableArrayBuilder<string>.Rent();

            foreach (var n in uniqueTypes)
            {
                builder.Add(n);
            }

            return builder.ToImmutable().AsEquatableArray();
        }

        private static void TryEnqueueMembers(
              EquatableArray<MemberModel> members
            , Dictionary<string, DataModel> dataMap
            , HashSet<string> uniqueTypes
            , Queue<string> queue
        )
        {
            foreach (var member in members)
            {
                var coll = member.SelectCollection();

                if (coll.kind == CollectionKind.Dictionary)
                {
                    TryEnqueue(coll.keyTypeName, dataMap, uniqueTypes, queue);
                    TryEnqueue(coll.elementTypeName, dataMap, uniqueTypes, queue);
                }
                else if (coll.kind != CollectionKind.NotCollection)
                {
                    TryEnqueue(coll.elementTypeName, dataMap, uniqueTypes, queue);
                }
                else
                {
                    TryEnqueue(member.SelectTypeFullName(), dataMap, uniqueTypes, queue);
                }
            }
        }

        private static void TryEnqueue(
              string typeName
            , Dictionary<string, DataModel> dataMap
            , HashSet<string> uniqueTypes
            , Queue<string> queue
        )
        {
            if (string.IsNullOrEmpty(typeName))
            {
                return;
            }

            if (uniqueTypes.Contains(typeName))
            {
                return;
            }

            if (dataMap.ContainsKey(typeName))
            {
                queue.Enqueue(typeName);
                uniqueTypes.Add(typeName);
            }
        }

        private static void PrintAdditionalUsings(ref Printer p)
        {
            p.PrintEndLine();
            p.Print("#pragma warning disable CS0105 // Using directive appeared previously in this namespace").PrintEndLine();
            p.PrintEndLine();
            p.PrintLine("using System;");
            p.PrintLine("using System.Collections.Generic;");
            p.PrintLine("using System.Diagnostics.CodeAnalysis;");
            p.PrintLine("using System.CodeDom.Compiler;");
            p.PrintLine("using System.Runtime.CompilerServices;");
            p.PrintLine("using System.Runtime.InteropServices;");
            p.PrintLine("using Cathei.BakingSheet;");
            p.PrintLine("using Cathei.BakingSheet.Unity;");
            p.PrintLine("using EncosyTower.Common;");
            p.PrintLine("using EncosyTower.Collections;");
            p.PrintLine("using EncosyTower.Data;");
            p.PrintLine("using EncosyTower.Databases.Authoring;");
            p.PrintEndLine();
            p.PrintLine("using ENaming = EncosyTower.Naming;");
            p.PrintLine("using EDAuthoring = EncosyTower.Databases.Authoring;");
            p.PrintLine("using EDASourceGen = EncosyTower.Databases.Authoring.SourceGen;");
            p.PrintLine("using MELogging = Microsoft.Extensions.Logging;");
            p.PrintEndLine();
            p.Print("#pragma warning restore CS0105 // Using directive appeared previously in this namespace").PrintEndLine();
            p.PrintEndLine();
        }

        // Internal helper struct used only during extraction (not cached)
        private struct TableInfo
        {
            public INamedTypeSymbol tableType;
            public ITypeSymbol idType;
            public ITypeSymbol dataType;
            public string idTypeFullName;
            public string dataTypeFullName;
            public string tableTypeFullName;
            public string tableTypeSimpleName;
            public string dataTypeSimpleName;
            public string propertyName;
            public NamingStrategy namingStrategy;
            public Dictionary<string, ConverterModel> tableConverterMap;
        }
    }
}
