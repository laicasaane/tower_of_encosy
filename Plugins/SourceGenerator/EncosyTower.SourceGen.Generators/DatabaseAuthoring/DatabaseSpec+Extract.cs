using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading;
using EncosyTower.SourceGen.Helpers.Data;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Newtonsoft.Json.Utilities;

namespace EncosyTower.SourceGen.Generators.DatabaseAuthoring
{
    using static EncosyTower.SourceGen.Generators.DatabaseAuthoring.Helpers;

    partial struct DatabaseSpec
    {
        private const string GENERATOR_NAME = DatabaseAuthoringGenerator.GENERATOR_NAME;

        public static DatabaseSpec Extract(GeneratorAttributeSyntaxContext context, CancellationToken token)
        {
            if (context.TargetNode is not TypeDeclarationSyntax typeSyntax
                || IsSupportedTypeSyntax(typeSyntax) == false
                || context.TargetSymbol is not INamedTypeSymbol authoringSymbol
            )
            {
                return default;
            }

            var authorAttrib = context.Attributes[0];

            if (authorAttrib.ConstructorArguments.Length < 1
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

            TypeCreationHelpers.GenerateOpeningAndClosingSource(
                  typeSyntax
                , token
                , out var openingSource
                , out var closingSource
                , printAdditionalUsings: PrintAdditionalUsings
            );

            var syntaxTree = typeSyntax.SyntaxTree;
            var containerHintName = GetHintName(
                  syntaxTree
                , GENERATOR_NAME
                , typeSyntax
                , $"{databaseIdentifier}__SheetContainer"
            );

            var namingStrategy = NamingStrategy.PascalCase;

            foreach (var arg in databaseAttrib.ConstructorArguments)
            {
                if (arg.Kind != TypedConstantKind.Array && arg.Value != null)
                {
                    namingStrategy = arg.Value.ToNamingStrategy();
                    break;
                }
            }

            var dbConverterMap = new Dictionary<string, ConverterSpec>(StringComparer.Ordinal);

            foreach (var arg in authorAttrib.ConstructorArguments)
            {
                if (arg.Kind == TypedConstantKind.Array)
                {
                    BuildConverterMap(arg.Values, dbConverterMap);
                    break;
                }
            }

            foreach (var arg in databaseAttrib.ConstructorArguments)
            {
                if (arg.Kind == TypedConstantKind.Array)
                {
                    BuildConverterMap(arg.Values, dbConverterMap);
                    break;
                }
            }

            var tableModelList = new List<TableSpec>();
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
                var tableConverterMap = new Dictionary<string, ConverterSpec>(StringComparer.Ordinal);

                foreach (var arg in tableAttrib.ConstructorArguments)
                {
                    if (arg.Kind != TypedConstantKind.Array && arg.Value != null)
                    {
                        tableNamingStrategy = arg.Value.ToNamingStrategy();
                    }
                    else if (arg.Kind == TypedConstantKind.Array)
                    {
                        BuildConverterMap(arg.Values, tableConverterMap);
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

                tableModelList.Add(new TableSpec {
                    typeFullName = tableType.ToFullName(),
                    typeSimpleName = tableTypeName,
                    idTypeFullName = idTypeFullName,
                    dataTypeFullName = dataTypeFullName,
                    propertyName = property.Name,
                    namingStrategy = tableNamingStrategy,
                    assetName = assetName,
                    uniqueSheetName = uniqueSheetName,
                    baseSheetName = baseSheetName,
                });

                tableInfoList.Add(new TableInfo {
                    tableType = tableType,
                    idType = idType,
                    dataType = dataType,
                    idTypeFullName = idTypeFullName,
                    dataTypeFullName = dataTypeFullName,
                    tableTypeFullName = tableType.ToFullName(),
                    tableTypeSimpleName = tableTypeName,
                    dataTypeSimpleName = dataTypeSimpleName,
                    propertyName = property.Name,
                    namingStrategy = tableNamingStrategy,
                    tableConverterMap = tableConverterMap,
                });

                CollectHorizontalLists(member, tableType, horizontalListMap);
            }

            if (tableModelList.Count < 1)
            {
                return default;
            }

            var dataMap = BuildDataMap(tableInfoList, dbConverterMap);
            var assetRefListDict = new Dictionary<string, AssetRefListSpec>(StringComparer.Ordinal);
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
                    var newRef = new AssetRefListSpec {
                        tableTypeFullName = tableInfo.tableTypeFullName,
                        tableTypeSimpleName = tableInfo.tableTypeSimpleName,
                        dataTypeFullName = tableInfo.dataTypeFullName,
                        dataTypeSimpleName = tableInfo.dataTypeSimpleName,
                        fieldNames = default,
                    };

                    assetRefListDict[tableTypeKey] = newRef;
                    assetRefListOrder.Add(tableTypeKey);
                    existing = newRef;
                }

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

            using var hlBuilder = ImmutableArrayBuilder<HorizontalListSpec>.Rent();

            foreach (var targetKv in horizontalListMap)
            {
                foreach (var containingKv in targetKv.Value)
                {
                    using var propBuilder = ImmutableArrayBuilder<string>.Rent();

                    foreach (var propName in containingKv.Value)
                    {
                        propBuilder.Add(propName);
                    }

                    hlBuilder.Add(new HorizontalListSpec {
                        targetTypeFullName = targetKv.Key,
                        containingTypeFullName = containingKv.Key,
                        propertyNames = propBuilder.ToImmutable().AsEquatableArray(),
                    });
                }
            }

            var processedTableTypes = new HashSet<string>(StringComparer.Ordinal);
            var sheetList = new List<SheetSpec>();

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

                var nestedFullNames = BuildNestedDataTypeFullNames(tableInfo, dataMap);

                sheetList.Add(new SheetSpec {
                    hintName = sheetHintName,
                    idTypeFullName = tableInfo.idTypeFullName,
                    idTypeSimpleName = tableInfo.idType is INamedTypeSymbol nid ? nid.Name : tableInfo.idType.Name,
                    dataTypeFullName = dataTypeFullName,
                    dataTypeSimpleName = tableInfo.dataTypeSimpleName,
                    tableTypeFullName = tableInfo.tableTypeFullName,
                    sheetName = sheetName,
                    nestedDataTypeFullNames = nestedFullNames,
                });
            }

            if (sheetList.Count < 1)
            {
                return default;
            }

            using var dataModelsBuilder = ImmutableArrayBuilder<DataSpec>.Rent();

            foreach (var dm in dataMap.Values)
            {
                dataModelsBuilder.Add(dm);
            }

            using var tableModelsBuilder = ImmutableArrayBuilder<TableSpec>.Rent();

            foreach (var tm in tableModelList)
            {
                tableModelsBuilder.Add(tm);
            }

            using var assetRefListBuilder = ImmutableArrayBuilder<AssetRefListSpec>.Rent();

            foreach (var key in assetRefListOrder)
            {
                assetRefListBuilder.Add(assetRefListDict[key]);
            }

            using var typeNamesBuilder = ImmutableArrayBuilder<string>.Rent();

            foreach (var tn in typeNameList)
            {
                typeNamesBuilder.Add(tn);
            }

            using var sheetsBuilder = ImmutableArrayBuilder<SheetSpec>.Rent();

            foreach (var s in sheetList)
            {
                sheetsBuilder.Add(s);
            }

            return new DatabaseSpec {
                location = LocationInfo.From(typeSyntax.GetLocation()),
                databaseTypeName = databaseTypeName,
                databaseTypeKeyword = databaseTypeKeyword,
                databaseIdentifier = databaseIdentifier,
                openingSource = openingSource,
                closingSource = closingSource,
                containerHintName = containerHintName,
                allDataModels = dataModelsBuilder.ToImmutable().AsEquatableArray(),
                horizontalListEntries = hlBuilder.ToImmutable().AsEquatableArray(),
                tables = tableModelsBuilder.ToImmutable().AsEquatableArray(),
                assetRefLists = assetRefListBuilder.ToImmutable().AsEquatableArray(),
                typeNames = typeNamesBuilder.ToImmutable().AsEquatableArray(),
                maxFieldOfSameTable = maxFieldOfSameTable,
                sheets = sheetsBuilder.ToImmutable().AsEquatableArray(),
            };
        }


        private static bool IsSupportedTypeSyntax(TypeDeclarationSyntax syntax)
            => syntax.IsKind(SyntaxKind.ClassDeclaration) || syntax.IsKind(SyntaxKind.StructDeclaration);

        private static string GetHintName(SyntaxTree syntaxTree, string _, SyntaxNode node, string typeName)
        {
            var stableHashCode = SourceGenHelpers.GetStableHashCode(syntaxTree.FilePath) & 0x7fffffff;
            var postfix = string.IsNullOrWhiteSpace(typeName) ? string.Empty : typeName;
            var salting = node.GetLineNumber();
            return $"{postfix}_{stableHashCode}_{salting}.g.cs";
        }

        private static void CollectHorizontalLists(
              ISymbol member
            , INamedTypeSymbol tableType
            , Dictionary<string, Dictionary<string, HashSet<string>>> horizontalListMap
        )
        {
            var attributes = member.GetAttributes(PR_HORIZONTAL_LIST_ATTRIBUTE);

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

        private static Dictionary<string, DataSpec> BuildDataMap(
              List<TableInfo> tableInfoList
            , Dictionary<string, ConverterSpec> dbConverterMap
        )
        {
            var map = new Dictionary<string, DataSpec>(StringComparer.Ordinal);
            var set = new HashSet<ITypeSymbol>(SymbolEqualityComparer.Default);
            var queue = new Queue<ITypeSymbol>();

            foreach (var tableInfo in tableInfoList)
            {
                queue.Enqueue(tableInfo.idType);
                queue.Enqueue(tableInfo.dataType);

                while (queue.Count > 0)
                {
                    var type = queue.Dequeue();

                    if (type.HasAttribute(DATA_ATTRIBUTE) == false)
                    {
                        continue;
                    }

                    if (set.Contains(type))
                    {
                        continue;
                    }

                    set.Add(type);

                    var dataModel = ExtractDataModel(
                          type
                        , dbConverterMap
                        , tableInfo.tableConverterMap
                        , queue
                    );

                    if (dataModel.propRefs.Count < 1
                        && dataModel.fieldRefs.Count < 1
                        && dataModel.baseTypeLayers.Count < 1
                    )
                    {
                        continue;
                    }

                    map[dataModel.fullName] = dataModel;
                }
            }

            return map;
        }

        private static DataSpec ExtractDataModel(
              ITypeSymbol type
            , Dictionary<string, ConverterSpec> dbConverterMap
            , Dictionary<string, ConverterSpec> tableConverterMap
            , Queue<ITypeSymbol> queue
        )
        {
            ExtractMemberModels(
                  type
                , dbConverterMap
                , tableConverterMap
                , queue
                , out var propRefs
                , out var fieldRefs
            );

            using var baseLayerBuilder = ImmutableArrayBuilder<DataLayerSpec>.Rent();

            var baseType = (type as INamedTypeSymbol)?.BaseType;

            while (baseType != null)
            {
                if (baseType.TypeKind != TypeKind.Class || baseType.HasAttribute(DATA_ATTRIBUTE) == false)
                {
                    break;
                }

                ExtractMemberModels(
                      baseType
                    , dbConverterMap
                    , tableConverterMap
                    , queue
                    , out var basePropRefs
                    , out var baseFieldRefs
                );

                baseLayerBuilder.Add(new DataLayerSpec {
                    fullName = baseType.ToFullName(),
                    simpleName = baseType.Name,
                    validIdentifier = baseType.ToValidIdentifier(),
                    propRefs = basePropRefs,
                    fieldRefs = baseFieldRefs,
                });

                baseType = baseType.BaseType;
            }

            var rawLayers = baseLayerBuilder.ToImmutable();
            using var reversedLayers = ImmutableArrayBuilder<DataLayerSpec>.Rent();

            for (var i = rawLayers.Length - 1; i >= 0; i--)
            {
                reversedLayers.Add(rawLayers[i]);
            }

            return new DataSpec {
                fullName = type.ToFullName(),
                simpleName = type.Name,
                validIdentifier = type.ToValidIdentifier(),
                propRefs = propRefs,
                fieldRefs = fieldRefs,
                baseTypeLayers = reversedLayers.ToImmutable().AsEquatableArray(),
            };
        }

        private static void ExtractMemberModels(
              ITypeSymbol type
            , Dictionary<string, ConverterSpec> dbConverterMap
            , Dictionary<string, ConverterSpec> tableConverterMap
            , Queue<ITypeSymbol> queue
            , out EquatableArray<MemberSpec> propRefs
            , out EquatableArray<MemberSpec> fieldRefs
        )
        {
            var properties = new List<(string propertyName, string fieldName, ISymbol member, ITypeSymbol fieldType)>();
            var fields = new List<(string propertyName, string fieldName, ISymbol member, ITypeSymbol fieldType)>();

            var members = type.GetMembers();

            foreach (var member in members)
            {
                if (member is IPropertySymbol property)
                {
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

                    var dataPropAttr = property.GetAttribute(DATA_PROPERTY_ATTRIBUTE);

                    if (dataPropAttr != null)
                    {
                        ITypeSymbol fieldType;

                        if (dataPropAttr.ConstructorArguments.Length < 1
                            || dataPropAttr.ConstructorArguments[0].Value is not ITypeSymbol ft
                        )
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
                    var genFieldAttr = field.GetAttribute(GENERATED_FIELD_FROM_PROPERTY);

                    if (genFieldAttr != null
                        && genFieldAttr.ConstructorArguments.Length > 0
                        && genFieldAttr.ConstructorArguments[0].Value is string propName
                    )
                    {
                        fields.Add((propName, field.Name, field, field.Type));
                        continue;
                    }

                    if (field.HasAttribute(SERIALIZE_FIELD_ATTRIBUTE))
                    {
                        fields.Add((field.ToPropertyName(), field.Name, field, field.Type));
                        continue;
                    }
                }
            }

            using var propBuilder = ImmutableArrayBuilder<MemberSpec>.Rent();
            using var fieldBuilder = ImmutableArrayBuilder<MemberSpec>.Rent();

            var uniqueFieldNames = new HashSet<string>(StringComparer.Ordinal);

            foreach (var (propertyName, fieldName, member, fieldType) in properties)
            {
                if (uniqueFieldNames.Contains(fieldName))
                {
                    continue;
                }

                uniqueFieldNames.Add(fieldName);
                propBuilder.Add(BuildMemberModel(
                      propertyName
                    , member
                    , fieldType
                    , dbConverterMap
                    , tableConverterMap
                    , queue
                ));
            }

            foreach (var (propertyName, fieldName, member, fieldType) in fields)
            {
                if (uniqueFieldNames.Contains(fieldName))
                {
                    continue;
                }

                uniqueFieldNames.Add(fieldName);
                fieldBuilder.Add(BuildMemberModel(
                      propertyName
                    , member
                    , fieldType
                    , dbConverterMap
                    , tableConverterMap
                    , queue
                ));
            }

            propRefs = propBuilder.ToImmutable().AsEquatableArray();
            fieldRefs = fieldBuilder.ToImmutable().AsEquatableArray();
        }

        private static MemberSpec BuildMemberModel(
              string propertyName
            , ISymbol memberSymbol
            , ITypeSymbol fieldType
            , Dictionary<string, ConverterSpec> dbConverterMap
            , Dictionary<string, ConverterSpec> tableConverterMap
            , Queue<ITypeSymbol> queue
        )
        {
            var collection = MakeCollectionModel(fieldType, out var keyType, out var elemType);
            EnqueueTypes(queue, fieldType, keyType, elemType);

            var converter = TryMakeConverterModel(
                  memberSymbol
                , fieldType
                , out var convType
                , out var convKeyType
                , out var convElemType
            );

            EnqueueTypes(queue, convType, convKeyType, convElemType);

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

            if (converter.kind == ConverterKind.None)
            {
                converter = TryFallbackConverterModel(
                      fieldType
                    , out var convFbType
                    , out var convFbKeyType
                    , out var convFbElemType
                );

                EnqueueTypes(queue, convFbType, convFbKeyType, convFbElemType);
            }

            var sheetConverter = default(ConverterSpec);

            if (collection.kind == CollectionKind.NotCollection
                && converter.kind != ConverterKind.None
            )
            {
                if (IsStringSourceConverter(converter))
                {
                    sheetConverter = converter;
                    converter = default;
                }
                else if (converter.sourceCollection.kind == CollectionKind.NotCollection && TryGetStringSourceConverter(
                        converter.sourceType.fullName
                        , tableConverterMap
                        , dbConverterMap
                        , out var sourceConverter
                ))
                {
                    sheetConverter = sourceConverter;
                }
            }

            return new MemberSpec {
                propertyName = propertyName,
                type = MakeTypeModel(fieldType),
                collection = collection,
                converter = converter,
                sheetConverter = sheetConverter,
            };
        }

        private static bool IsStringSourceConverter(ConverterSpec converter)
            => converter.sourceCollection.kind == CollectionKind.NotCollection
            && converter.sourceType.fullName == "string";

        private static bool TryGetStringSourceConverter(
              string targetTypeFullName
            , Dictionary<string, ConverterSpec> tableConverterMap
            , Dictionary<string, ConverterSpec> dbConverterMap
            , out ConverterSpec converter
        )
        {
            if (tableConverterMap.TryGetValue(targetTypeFullName, out converter)
                && IsStringSourceConverter(converter)
            )
            {
                return true;
            }

            if (dbConverterMap.TryGetValue(targetTypeFullName, out converter)
                && IsStringSourceConverter(converter)
            )
            {
                return true;
            }

            converter = default;
            return false;
        }

        private static TypeSpec MakeTypeModel(ITypeSymbol type)
            => new() {
                fullName = type.ToFullName(),
                simpleName = type.Name,
                isValueType = type.IsValueType,
                hasParameterlessConstructor = CheckParameterlessConstructor(type),
            };

        private static void EnqueueTypes(Queue<ITypeSymbol> queue, ITypeSymbol type, ITypeSymbol keyType, ITypeSymbol elemType)
        {
            if (keyType == null && elemType == null)
            {
                queue.Enqueue(type);
            }
            else
            {
                if (keyType != null)
                {
                    queue.Enqueue(keyType);
                }

                if (elemType != null)
                {
                    queue.Enqueue(elemType);
                }
            }
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
                if (typeMember is not IMethodSymbol method || method.MethodKind != MethodKind.Constructor)
                {
                    continue;
                }

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

            return found ?? paramCtorCount == 0;
        }

        private static CollectionSpec MakeCollectionModel(
              ITypeSymbol type
            , out ITypeSymbol keyType
            , out ITypeSymbol elemType
        )
        {
            keyType = null;
            elemType = null;

            if (type is IArrayTypeSymbol arrayType)
            {
                elemType = arrayType.ElementType;

                return new() {
                    kind = CollectionKind.Array,
                    elementType = MakeTypeModel(elemType),
                };
            }

            if (type is INamedTypeSymbol namedType)
            {
                if (namedType.TryGetGenericType(LIST_FAST_TYPE_T, 1, out var listFastType))
                {
                    return MakeListModel(elemType = listFastType.TypeArguments[0]);
                }

                if (namedType.TryGetGenericType(LIST_TYPE_T, 1, out var listType))
                {
                    return MakeListModel(elemType = listType.TypeArguments[0]);
                }

                if (namedType.TryGetGenericType(DICTIONARY_TYPE_T, 2, out var dictType))
                {
                    return MakeDictModel(keyType = dictType.TypeArguments[0], elemType = dictType.TypeArguments[1]);
                }

                if (namedType.TryGetGenericType(HASH_SET_TYPE_T, 1, out var hashSetType))
                {
                    return MakeKindModel(CollectionKind.HashSet, elemType = hashSetType.TypeArguments[0]);
                }

                if (namedType.TryGetGenericType(QUEUE_TYPE_T, 1, out var queueType))
                {
                    return MakeKindModel(CollectionKind.Queue, elemType = queueType.TypeArguments[0]);
                }

                if (namedType.TryGetGenericType(STACK_TYPE_T, 1, out var stackType))
                {
                    return MakeKindModel(CollectionKind.Stack, elemType = stackType.TypeArguments[0]);
                }

                if (namedType.TryGetGenericType(READONLY_MEMORY_TYPE_T, 1, out var readMemType))
                {
                    return MakeKindModel(CollectionKind.Array, elemType = readMemType.TypeArguments[0]);
                }

                if (namedType.TryGetGenericType(MEMORY_TYPE_T, 1, out var memType))
                {
                    return MakeKindModel(CollectionKind.Array, elemType = memType.TypeArguments[0]);
                }

                if (namedType.TryGetGenericType(READONLY_SPAN_TYPE_T, 1, out var readSpanType))
                {
                    return MakeKindModel(CollectionKind.Array, elemType = readSpanType.TypeArguments[0]);
                }

                if (namedType.TryGetGenericType(SPAN_TYPE_T, 1, out var spanType))
                {
                    return MakeKindModel(CollectionKind.Array, elemType = spanType.TypeArguments[0]);
                }

                if (namedType.TryGetGenericType(IREADONLY_LIST_TYPE_T, 1, out var roListType))
                {
                    return MakeListModel(elemType = roListType.TypeArguments[0]);
                }

                if (namedType.TryGetGenericType(ILIST_TYPE_T, 1, out var ilistType))
                {
                    return MakeListModel(elemType = ilistType.TypeArguments[0]);
                }

                if (namedType.TryGetGenericType(ISET_TYPE_T, 1, out var isetType))
                {
                    return MakeKindModel(CollectionKind.HashSet, elemType = isetType.TypeArguments[0]);
                }

                if (namedType.TryGetGenericType(IREADONLY_DICTIONARY_TYPE_T, 2, out var roDictType))
                {
                    return MakeDictModel(keyType = roDictType.TypeArguments[0], elemType = roDictType.TypeArguments[1]);
                }

                if (namedType.TryGetGenericType(IDICTIONARY_TYPE_T, 2, out var idictType))
                {
                    return MakeDictModel(keyType = idictType.TypeArguments[0], elemType = idictType.TypeArguments[1]);
                }
            }

            return default;

            static CollectionSpec MakeListModel(ITypeSymbol elem)
                => new() {
                    kind = CollectionKind.List,
                    elementType = MakeTypeModel(elem),
                };

            static CollectionSpec MakeKindModel(CollectionKind k, ITypeSymbol elem)
                => new() {
                    kind = k,
                    elementType = MakeTypeModel(elem),
                };

            static CollectionSpec MakeDictModel(ITypeSymbol key, ITypeSymbol elem)
                => new() {
                    kind = CollectionKind.Dictionary,
                    keyType = MakeTypeModel(key),
                    elementType = MakeTypeModel(elem),
                };
        }

        private static ConverterSpec TryMakeConverterModel(
              ISymbol memberSymbol
            , ITypeSymbol targetType
            , out ITypeSymbol type
            , out ITypeSymbol keyType
            , out ITypeSymbol elemType
        )
        {
            var attrib = memberSymbol.GetAttribute(DATA_CONVERTER_ATTRIBUTE);

            if (attrib == null || attrib.ConstructorArguments.Length != 1)
            {
                type = targetType;
                keyType = elemType = null;
                return default;
            }

            if (attrib.ConstructorArguments[0].Value is not INamedTypeSymbol converterType)
            {
                type = targetType;
                keyType = elemType = null;
                return default;
            }

            if (TryGetConvertMethod(converterType, out var convertMethod, targetType) == false)
            {
                type = targetType;
                keyType = elemType = null;
                return default;
            }

            return MakeConverterModelFromMethod(converterType, convertMethod, out type, out keyType, out elemType);
        }

        private static ConverterSpec TryFallbackConverterModel(
              ITypeSymbol fieldType
            , out ITypeSymbol type
            , out ITypeSymbol keyType
            , out ITypeSymbol elemType
        )
        {
            if (fieldType is not INamedTypeSymbol namedReturn)
            {
                type = fieldType;
                keyType = elemType = null;
                return default;
            }

            if (TryGetConvertMethod(namedReturn, out var convertMethod, namedReturn) == false)
            {
                type = fieldType;
                keyType = elemType = null;
                return default;
            }

            return MakeConverterModelFromMethod(namedReturn, convertMethod, out type, out keyType, out elemType);
        }

        private static ConverterSpec MakeConverterModelFromMethod(
              INamedTypeSymbol converterType
            , IMethodSymbol convertMethod
            , out ITypeSymbol type
            , out ITypeSymbol keyType
            , out ITypeSymbol elemType
        )
        {
            var sourceType = convertMethod.Parameters[0].Type;
            var sourceCollection = MakeCollectionModel(sourceType, out keyType, out elemType);
            type = keyType == null && elemType == null ? sourceType : null;

            return new ConverterSpec {
                kind = convertMethod.IsStatic ? ConverterKind.Static : ConverterKind.Instance,
                converterTypeFullName = converterType.ToFullName(),
                sourceCollection = sourceCollection,
                sourceType = MakeTypeModel(sourceType),
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
                || (returnType != null
                    && SymbolEqualityComparer.Default.Equals(convertMethod.ReturnType, returnType) == false
                )
            )
            {
                convertMethod = null;
                return false;
            }

            return true;
        }

        private static void BuildConverterMap(
              ImmutableArray<TypedConstant> values
            , Dictionary<string, ConverterSpec> converterMap
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
                    converterMap[targetTypeFullName] = MakeConverterModelFromMethod(
                          type
                        , convertMethod
                        , out _
                        , out _
                        , out _
                    );
                }
            }
        }

        private static EquatableArray<string> BuildNestedDataTypeFullNames(
              TableInfo tableInfo
            , Dictionary<string, DataSpec> dataMap
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
              EquatableArray<MemberSpec> members
            , Dictionary<string, DataSpec> dataMap
            , HashSet<string> uniqueTypes
            , Queue<string> queue
        )
        {
            foreach (var member in members)
            {
                var coll = member.SelectCollection();

                if (coll.kind == CollectionKind.Dictionary)
                {
                    TryEnqueue(coll.keyType.fullName, dataMap, uniqueTypes, queue);
                    TryEnqueue(coll.elementType.fullName, dataMap, uniqueTypes, queue);
                }
                else if (coll.kind != CollectionKind.NotCollection)
                {
                    TryEnqueue(coll.elementType.fullName, dataMap, uniqueTypes, queue);
                }
                else
                {
                    TryEnqueue(member.SelectType().fullName, dataMap, uniqueTypes, queue);
                }
            }
        }

        private static void TryEnqueue(
              string typeName
            , Dictionary<string, DataSpec> dataMap
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
            p.PrintLine("using S = global::System;");
            p.PrintLine("using SCG = global::System.Collections.Generic;");
            p.PrintLine("using SDCA = global::System.Diagnostics.CodeAnalysis;");
            p.PrintLine("using SCDC = global::System.CodeDom.Compiler;");
            p.PrintLine("using SRCS = global::System.Runtime.CompilerServices;");
            p.PrintLine("using SRIS = global::System.Runtime.InteropServices;");
            p.PrintLine("using CBS = Cathei.BakingSheet;");
            p.PrintLine("using CBSU = Cathei.BakingSheet.Unity;");
            p.PrintLine("using ET = global::EncosyTower.Common;");
            p.PrintLine("using ETC = EncosyTower.Collections;");
            p.PrintLine("using ETCE = global::EncosyTower.Collections.Extensions;");
            p.PrintLine("using ETD = global::EncosyTower.Data;");
            p.PrintLine("using ETDBA = EncosyTower.Databases.Authoring;");
            p.PrintLine("using ETDBASG = EncosyTower.Databases.Authoring.SourceGen;");
            p.PrintLine("using ETDSG = global::EncosyTower.Data.SourceGen;");
            p.PrintLine("using ETN = EncosyTower.Naming;");
            p.PrintLine("using MEL = Microsoft.Extensions.Logging;");
            p.PrintLine("using UE = global::UnityEngine;");
            p.PrintLine("using UP = global::Unity.Properties;");
            p.PrintEndLine();
            p.Print("#pragma warning restore CS0105 // Using directive appeared previously in this namespace").PrintEndLine();
            p.PrintEndLine();
        }

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
            public Dictionary<string, ConverterSpec> tableConverterMap;
        }
    }
}
