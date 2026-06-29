using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading;
using EncosyTower.SourceGen.Helpers.Data;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace EncosyTower.SourceGen.Generators.DatabaseAuthoring
{
    using static EncosyTower.SourceGen.Generators.DatabaseAuthoring.Helpers;

    partial struct DatabaseSpec
    {
        private const string GENERATOR_NAME = DatabaseAuthoringGenerator.GENERATOR_NAME;

        public static DatabaseSpec Extract(GeneratorAttributeSyntaxContext context, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            if (context.TargetNode is not TypeDeclarationSyntax authoringTypeSyntax
                || IsSupportedTypeSyntax(authoringTypeSyntax) == false
                || context.TargetSymbol is not INamedTypeSymbol authoringTypeSymbol
            )
            {
                return default;
            }

            var authorAttrib = context.Attributes[0];

            if (authorAttrib.ConstructorArguments.Length < 1
                || authorAttrib.ConstructorArguments[0].Kind != TypedConstantKind.Type
                || authorAttrib.ConstructorArguments[0].Value is not INamedTypeSymbol databaseSymbol
                || databaseSymbol.GetAttribute(DATABASE_ATTRIBUTE, token) is not AttributeData databaseAttrib
            )
            {
                return default;
            }

            var databaseTypeName = authoringTypeSyntax.Identifier.Text;
            var databaseTypeKeyword = authoringTypeSymbol.IsValueType ? "struct" : "class";
            var databaseIdentifier = authoringTypeSymbol.ToValidIdentifier();

            TypeCreationHelpers.GenerateOpeningAndClosingSource(
                  authoringTypeSyntax
                , token
                , out var openingSource
                , out var closingSource
                , printAdditionalUsings: PrintAdditionalUsings
            );

            token.ThrowIfCancellationRequested();

            var syntaxTree = authoringTypeSyntax.SyntaxTree;
            var fileName = $"{databaseIdentifier}_SheetContainer";
            var containerHintName = syntaxTree.GetHintName(authoringTypeSyntax, fileName);

            var authorDbMap = new Dictionary<string, ConverterSpec>(StringComparer.Ordinal);
            var databaseMap = new Dictionary<string, ConverterSpec>(StringComparer.Ordinal);
            var ignoredTypes = new IgnoredTypes();
            var resultTypes = new ResultTypes();
            var nameCasing = NameCasing.Pascal;
            var fullyQualifiedSheetNames = false;

            ProcessAttributes(
                  authorAttrib
                , databaseAttrib
                , authorDbMap
                , databaseMap
                , ignoredTypes
                , resultTypes
                , ref nameCasing
                , ref fullyQualifiedSheetNames
                , token
            );

            var cfgTableEntries = new List<ConverterForTableEntry>();
            var cfgPropEntries = new List<ConverterForDataPropertyEntry>();

            ExtractConverterAttributes(
                  authoringTypeSymbol
                , cfgTableEntries
                , cfgPropEntries
                , ignoredTypes
                , resultTypes
                , token
            );

            var tableInfoList = new List<TableInfo>();
            var hlBuilder = ImmutableArrayBuilder<HorizontalListSpec>.Rent();

            ProcessDatabaseSymbol(
                  databaseSymbol
                , ignoredTypes
                , resultTypes
                , nameCasing
                , tableInfoList
                , ref hlBuilder
                , token
            );

            if (tableInfoList.Count < 1)
            {
                hlBuilder.Dispose();
                return default;
            }

            token.ThrowIfCancellationRequested();

            var dataMap = BuildShapeDataMap(tableInfoList, ignoredTypes, resultTypes, token);

            var scopedConvertersBuilder = ImmutableArrayBuilder<ScopedConverterSpec>.Rent();

            ResolveConverters(
                  tableInfoList
                , dataMap
                , authorDbMap
                , databaseMap
                , cfgTableEntries
                , cfgPropEntries
                , ref scopedConvertersBuilder
                , token
            );

            var scopedConverters = scopedConvertersBuilder.ToImmutable().AsEquatableArray();
            scopedConvertersBuilder.Dispose();

            var sheetGroupMap = new Dictionary<string, SheetGroupInfo>(StringComparer.Ordinal);
            var sheetGroupListOrder = new List<string>();
            var typeNameList = new List<string>();
            var sheetList = new List<SheetSpec>();
            var tableSpecList = new List<TableSpec>();

            ProcessTableInfoList(
                  tableInfoList
                , dataMap
                , sheetGroupMap
                , sheetGroupListOrder
                , typeNameList
                , sheetList
                , tableSpecList
                , syntaxTree
                , authoringTypeSyntax
                , databaseIdentifier
                , fullyQualifiedSheetNames
                , token
            );

            if (sheetList.Count < 1)
            {
                return default;
            }

            token.ThrowIfCancellationRequested();

            using var dataSpecListBuilder = ImmutableArrayBuilder<DataSpec>.Rent();

            foreach (var dm in dataMap.Values)
            {
                token.ThrowIfCancellationRequested();

                dataSpecListBuilder.Add(dm);
            }

            token.ThrowIfCancellationRequested();

            using var tableSpecListBuilder = ImmutableArrayBuilder<TableSpec>.Rent();

            foreach (var tm in tableSpecList)
            {
                token.ThrowIfCancellationRequested();

                tableSpecListBuilder.Add(tm);
            }

            token.ThrowIfCancellationRequested();

            using var sheetGroupListBuilder = ImmutableArrayBuilder<SheetGroupSpec>.Rent();

            foreach (var key in sheetGroupListOrder)
            {
                token.ThrowIfCancellationRequested();

                var assetInfo = sheetGroupMap[key];
                sheetGroupListBuilder.Add(new SheetGroupSpec {
                    baseSheetName = assetInfo.baseSheetName,
                    sheets = assetInfo.sheets.ToImmutableArray().AsEquatableArray(),
                });
            }

            token.ThrowIfCancellationRequested();

            using var typeNamesBuilder = ImmutableArrayBuilder<string>.Rent();

            foreach (var tn in typeNameList)
            {
                token.ThrowIfCancellationRequested();

                typeNamesBuilder.Add(tn);
            }

            token.ThrowIfCancellationRequested();

            using var sheetListBuilder = ImmutableArrayBuilder<SheetSpec>.Rent();

            foreach (var s in sheetList)
            {
                token.ThrowIfCancellationRequested();

                sheetListBuilder.Add(s);
            }

            var horizontalListEntries = hlBuilder.ToImmutable().AsEquatableArray();
            hlBuilder.Dispose();

            return new DatabaseSpec {
                location = LocationInfo.From(authoringTypeSyntax.GetLocation()),
                databaseTypeName = databaseTypeName,
                databaseTypeKeyword = databaseTypeKeyword,
                databaseIdentifier = databaseIdentifier,
                openingSource = openingSource,
                closingSource = closingSource,
                containerHintName = containerHintName,
                allDataModels = dataSpecListBuilder.ToImmutable().AsEquatableArray(),
                scopedConverters = scopedConverters,
                horizontalListEntries = horizontalListEntries,
                tables = tableSpecListBuilder.ToImmutable().AsEquatableArray(),
                sheetGroups = sheetGroupListBuilder.ToImmutable().AsEquatableArray(),
                typeNames = typeNamesBuilder.ToImmutable().AsEquatableArray(),
                sheets = sheetListBuilder.ToImmutable().AsEquatableArray(),
            };
        }

        private static void ProcessAttributes(
              AttributeData authorAttrib
            , AttributeData databaseAttrib
            , Dictionary<string, ConverterSpec> authorDbMap
            , Dictionary<string, ConverterSpec> databaseMap
            , IgnoredTypes ignoredTypes
            , ResultTypes resultTypes
            , ref NameCasing nameCasing
            , ref bool fullyQualifiedSheetNames
            , CancellationToken token
        )
        {
            token.ThrowIfCancellationRequested();

            foreach (var arg in databaseAttrib.ConstructorArguments)
            {
                token.ThrowIfCancellationRequested();

                if (arg.Kind != TypedConstantKind.Array && arg.Value != null)
                {
                    nameCasing = arg.Value.ToNameCasing();
                    break;
                }
            }

            token.ThrowIfCancellationRequested();

            // Precedence #3: AuthorDatabaseAttribute converters.
            foreach (var arg in authorAttrib.ConstructorArguments)
            {
                token.ThrowIfCancellationRequested();

                if (arg.Kind == TypedConstantKind.Array)
                {
                    BuildConverterMap(arg.Values, authorDbMap, ignoredTypes, resultTypes, token);
                    break;
                }
            }

            token.ThrowIfCancellationRequested();

            // Precedence #6: DatabaseAttribute converters.
            foreach (var arg in databaseAttrib.ConstructorArguments)
            {
                token.ThrowIfCancellationRequested();

                if (arg.Kind == TypedConstantKind.Array)
                {
                    BuildConverterMap(arg.Values, databaseMap, ignoredTypes, resultTypes, token);
                    break;
                }
            }

            token.ThrowIfCancellationRequested();

            foreach (var arg in authorAttrib.NamedArguments)
            {
                token.ThrowIfCancellationRequested();

                if (arg.Key == "FullyQualifiedSheetNames"
                    && arg.Value.Kind == TypedConstantKind.Primitive
                    && arg.Value.Value is bool fullyQualified
                )
                {
                    fullyQualifiedSheetNames = fullyQualified;
                }
            }
        }

        private static void ProcessDatabaseSymbol(
              INamedTypeSymbol databaseSymbol
            , IgnoredTypes ignoredTypes
            , ResultTypes resultTypes
            , NameCasing nameCasing
            , List<TableInfo> tableInfoList
            , ref ImmutableArrayBuilder<HorizontalListSpec> hlBuilder
            , CancellationToken token
        )
        {
            token.ThrowIfCancellationRequested();

            // HorizontalListMap: targetTypeFullName -> containingTypeFullName -> HashSet<propertyName>
            var stringComparer = StringComparer.Ordinal;
            var horizontalListMap = new Dictionary<string, Dictionary<string, HashSet<string>>>(stringComparer);
            var dbMembers = databaseSymbol.GetMembers();

            foreach (var member in dbMembers)
            {
                token.ThrowIfCancellationRequested();

                if (member is not IPropertySymbol property)
                {
                    continue;
                }

                if (property.Type is not INamedTypeSymbol tableType)
                {
                    continue;
                }

                var tableAttrib = member.GetAttribute(TABLE_ATTRIBUTE, token);

                if (tableAttrib == null)
                {
                    continue;
                }

                if (tableType.BaseType == null
                    || tableType.TryGetGenericType(DATA_TABLE_ASSET, 3, 2, out var baseType, token) == false
                )
                {
                    continue;
                }

                token.ThrowIfCancellationRequested();

                var tableNameCasing = nameCasing;
                var tableConverterMap = new Dictionary<string, ConverterSpec>(stringComparer);

                foreach (var arg in tableAttrib.ConstructorArguments)
                {
                    token.ThrowIfCancellationRequested();

                    if (arg.Kind != TypedConstantKind.Array && arg.Value != null)
                    {
                        tableNameCasing = arg.Value.ToNameCasing();
                    }
                    else if (arg.Kind == TypedConstantKind.Array)
                    {
                        BuildConverterMap(arg.Values, tableConverterMap, ignoredTypes, resultTypes, token);
                    }
                }

                var dataType = baseType.TypeArguments[1];
                var idType = FindCorrectIdType(dataType, baseType.TypeArguments[0], token);
                var idTypeFullName = idType.ToFullName();
                var dataTypeFullName = dataType.ToFullName();
                var tableTypeName = tableType.Name;
                var tableTypeFullName = tableType.ToFullName();
                var dataTypeSimpleName = dataType.Name;

                tableInfoList.Add(new TableInfo {
                    tableType = tableType,
                    idType = idType,
                    dataType = dataType,
                    idTypeFullName = idTypeFullName,
                    dataTypeFullName = dataTypeFullName,
                    tableTypeFullName = tableTypeFullName,
                    tableTypeSimpleName = tableTypeName,
                    dataTypeSimpleName = dataTypeSimpleName,
                    propertyName = property.Name,
                    nameCasing = tableNameCasing,
                    tableConverterMap = tableConverterMap,
                });

                CollectHorizontalLists(member, tableType, horizontalListMap, token);
            }

            token.ThrowIfCancellationRequested();

            foreach (var targetKv in horizontalListMap)
            {
                token.ThrowIfCancellationRequested();

                foreach (var containingKv in targetKv.Value)
                {
                    token.ThrowIfCancellationRequested();

                    using var propBuilder = ImmutableArrayBuilder<string>.Rent();

                    foreach (var propName in containingKv.Value)
                    {
                        token.ThrowIfCancellationRequested();

                        propBuilder.Add(propName);
                    }

                    hlBuilder.Add(new HorizontalListSpec {
                        targetTypeFullName = targetKv.Key,
                        containingTypeFullName = containingKv.Key,
                        propertyNames = propBuilder.ToImmutable().AsEquatableArray(),
                    });
                }
            }
        }

        private static ITypeSymbol FindCorrectIdType(
              ITypeSymbol dataType
            , ITypeSymbol candidateIdType
            , CancellationToken token
        )
        {
            token.ThrowIfCancellationRequested();

            ISymbol foundMember = null;
            var members = dataType.GetMembers();

            foreach (var member in members)
            {
                token.ThrowIfCancellationRequested();

                if (member is IPropertySymbol && member.Name.Equals("Id", StringComparison.Ordinal))
                {
                    foundMember = member;
                    break;
                }

                if (member is IFieldSymbol && member.Name.Equals("_id", StringComparison.Ordinal))
                {
                    foundMember = member;
                    break;
                }
            }

            token.ThrowIfCancellationRequested();

            if (foundMember is { }
                && foundMember.GetAttribute(DATA_MANUAL_AUTHORING_ATTRIBUTE, token) is { } manualAuthoringAttrib
                && manualAuthoringAttrib.ConstructorArguments.Length > 0
                && manualAuthoringAttrib.ConstructorArguments[0].Value is ITypeSymbol authoringType
            )
            {
                return authoringType;
            }

            if (foundMember is IFieldSymbol field)
            {
                return field.Type;
            }

            if (foundMember is not IPropertySymbol property)
            {
                return candidateIdType;
            }

            token.ThrowIfCancellationRequested();

            var attributes = property.GetAttributes();
            ITypeSymbol correctIdType = default;

            foreach (var attribute in attributes)
            {
                token.ThrowIfCancellationRequested();

                if (attribute.ConstructorArguments.Length > 1
                    && attribute.AttributeClass.HasFullName(GENERATED_PROPERTY_FROM_FIELD, token)
                    && attribute.ConstructorArguments[1].Value is ITypeSymbol found1
                )
                {
                    correctIdType = found1;
                    break;
                }

                if (attribute.ConstructorArguments.Length > 0
                    && attribute.AttributeClass.HasFullName(DATA_PROPERTY_ATTRIBUTE, token)
                    && attribute.ConstructorArguments[0].Value is ITypeSymbol found2
                )
                {
                    correctIdType = found2;
                    break;
                }
            }

            return correctIdType ?? candidateIdType;
        }

        private static void ProcessTableInfoList(
              List<TableInfo> tableInfoList
            , Dictionary<string, DataSpec> dataMap
            , Dictionary<string, SheetGroupInfo> sheetGroupMap
            , List<string> sheetGroupListOrder
            , List<string> typeNameList
            , List<SheetSpec> sheetList
            , List<TableSpec> tableSpecList
            , SyntaxTree syntaxTree
            , TypeDeclarationSyntax typeSyntax
            , string databaseIdentifier
            , bool fullyQualifiedSheetNames
            , CancellationToken token
        )
        {
            token.ThrowIfCancellationRequested();

            var stringComparer = StringComparer.Ordinal;

            // dataTypeFullName -> baseSheetName ("simple" form), used to detect name collisions across data types
            var dataTypesPerBase = new Dictionary<string, HashSet<string>>(stringComparer);

            // dataTypeFullName -> (scopeKey -> stable index), used to disambiguate per-table converter divergence
            var scopeIndexByData = new Dictionary<string, Dictionary<HashValue64, int>>(stringComparer);

            var dedupTableAssetMap = new Dictionary<string, bool>(stringComparer);
            var processedTableTypes = new HashSet<string>(stringComparer);

            foreach (var tableInfo in tableInfoList)
            {
                token.ThrowIfCancellationRequested();

                var dataTypeFullName = tableInfo.dataTypeFullName;

                if (dataMap.ContainsKey(dataTypeFullName) == false)
                {
                    continue;
                }

                var simpleBaseSheetName = $"{tableInfo.dataTypeSimpleName}Sheet";

                if (dataTypesPerBase.TryGetValue(simpleBaseSheetName, out var dataTypeSet) == false)
                {
                    dataTypesPerBase[simpleBaseSheetName] = dataTypeSet = new HashSet<string>(stringComparer);
                }

                dataTypeSet.Add(dataTypeFullName);

                if (scopeIndexByData.TryGetValue(dataTypeFullName, out var scopeIndexMap) == false)
                {
                    scopeIndexByData[dataTypeFullName] = scopeIndexMap = new Dictionary<HashValue64, int>();
                }

                if (scopeIndexMap.ContainsKey(tableInfo.scopeKey) == false)
                {
                    scopeIndexMap[tableInfo.scopeKey] = scopeIndexMap.Count;
                }

                var tableTypeSimpleName = tableInfo.tableTypeSimpleName;
                var existingTableAsset = dedupTableAssetMap.ContainsKey(tableTypeSimpleName);
                dedupTableAssetMap[tableTypeSimpleName] = existingTableAsset;
            }

            token.ThrowIfCancellationRequested();

            foreach (var tableInfo in tableInfoList)
            {
                token.ThrowIfCancellationRequested();

                var dataTypeFullName = tableInfo.dataTypeFullName;

                if (dataMap.ContainsKey(dataTypeFullName) == false)
                {
                    continue;
                }

                var tableTypeSimpleName = tableInfo.tableTypeSimpleName;
                var simpleBaseSheetName = $"{tableInfo.dataTypeSimpleName}Sheet";

                var collision = fullyQualifiedSheetNames
                    || (dataTypesPerBase.TryGetValue(simpleBaseSheetName, out var dataTypeSet)
                        && dataTypeSet.Count > 1
                    );

                var baseSheetNamePerInfo = collision
                    ? $"{dataTypeFullName.ToValidIdentifier()}Sheet"
                    : simpleBaseSheetName;

                var scopeIndexMap = scopeIndexByData[dataTypeFullName];

                if (scopeIndexMap.Count > 1)
                {
                    baseSheetNamePerInfo = $"{baseSheetNamePerInfo}_{scopeIndexMap[tableInfo.scopeKey]}";
                }

                var uniqueSheetName = $"{tableTypeSimpleName}_{baseSheetNamePerInfo}_{tableInfo.propertyName}";
                typeNameList.Add(uniqueSheetName);

                if (sheetGroupMap.TryGetValue(baseSheetNamePerInfo, out var existing) == false)
                {
                    var newRef = new SheetGroupInfo {
                        baseSheetName = baseSheetNamePerInfo,
                        sheets = new List<SheetInfoSpec>(),
                    };

                    sheetGroupMap[baseSheetNamePerInfo] = newRef;
                    sheetGroupListOrder.Add(baseSheetNamePerInfo);
                    existing = newRef;
                }

                existing.sheets.Add(new SheetInfoSpec {
                    tableName = tableTypeSimpleName,
                    propertyName = tableInfo.propertyName,
                });

                dedupTableAssetMap.TryGetValue(tableTypeSimpleName, out var deduplicateAssetName);

                tableSpecList.Add(new TableSpec {
                    typeFullName = tableInfo.tableTypeFullName,
                    typeSimpleName = tableTypeSimpleName,
                    idTypeFullName = tableInfo.idTypeFullName,
                    dataTypeFullName = tableInfo.dataTypeFullName,
                    propertyName = tableInfo.propertyName,
                    nameCasing = tableInfo.nameCasing,
                    baseSheetName = baseSheetNamePerInfo,
                    uniqueSheetName = uniqueSheetName,
                    deduplicateAssetName = deduplicateAssetName,
                });

                if (processedTableTypes.Contains(baseSheetNamePerInfo))
                {
                    continue;
                }

                processedTableTypes.Add(baseSheetNamePerInfo);

                var fileName = $"{databaseIdentifier}_{baseSheetNamePerInfo}";
                var hintName = syntaxTree.GetHintName(typeSyntax, fileName);

                sheetList.Add(new SheetSpec {
                    hintName = hintName,
                    idTypeFullName = tableInfo.idTypeFullName,
                    idTypeSimpleName = tableInfo.idType.Name,
                    dataTypeFullName = tableInfo.dataTypeFullName,
                    dataTypeSimpleName = tableInfo.dataTypeSimpleName,
                    tableTypeFullName = tableInfo.tableTypeFullName,
                    sheetName = baseSheetNamePerInfo,
                    scopeKey = tableInfo.scopeKey,
                    nestedDataTypeFullNames = tableInfo.nestedDataTypeFullNames,
                });
            }
        }

        private static bool IsSupportedTypeSyntax(TypeDeclarationSyntax syntax)
            => syntax.IsKind(SyntaxKind.ClassDeclaration) || syntax.IsKind(SyntaxKind.StructDeclaration);

        private static void CollectHorizontalLists(
              ISymbol member
            , INamedTypeSymbol tableType
            , Dictionary<string, Dictionary<string, HashSet<string>>> horizontalListMap
            , CancellationToken token
        )
        {
            token.ThrowIfCancellationRequested();

            var attributes = member.GetAttributes(PR_HORIZONTAL_LIST_ATTRIBUTE, token);

            foreach (var attrib in attributes)
            {
                token.ThrowIfCancellationRequested();

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

        private static Dictionary<string, DataSpec> BuildShapeDataMap(
              List<TableInfo> tableInfoList
            , IgnoredTypes ignoredTypes
            , ResultTypes resultTypes
            , CancellationToken token
        )
        {
            token.ThrowIfCancellationRequested();

            var map = new Dictionary<string, DataSpec>(128, StringComparer.Ordinal);
            var set = new HashSet<ITypeSymbol>(SymbolEqualityComparer.Default);
            var queue = new Queue<ITypeSymbol>(128);

            EnqueueTypes(queue, resultTypes, token);
            resultTypes.Clear();

            foreach (var tableInfo in tableInfoList)
            {
                token.ThrowIfCancellationRequested();

                queue.Enqueue(tableInfo.idType);
                queue.Enqueue(tableInfo.dataType);

                while (queue.Count > 0)
                {
                    token.ThrowIfCancellationRequested();

                    var type = queue.Dequeue();

                    if (type.HasAttribute(DATA_ATTRIBUTE, token) == false)
                    {
                        ignoredTypes.Add(type);
                        continue;
                    }

                    if (set.Contains(type))
                    {
                        continue;
                    }

                    set.Add(type);

                    var dataModel = ExtractDataModel(
                          type
                        , ignoredTypes
                        , resultTypes
                        , token
                    );

                    EnqueueTypes(queue, resultTypes, token);
                    resultTypes.Clear();

                    if (dataModel.propRefs.Count < 1
                        && dataModel.fieldRefs.Count < 1
                        && dataModel.baseTypeRefs.Count < 1
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
            , IgnoredTypes ignoredTypes
            , ResultTypes resultTypes
            , CancellationToken token
        )
        {
            token.ThrowIfCancellationRequested();

            ExtractMemberModels(
                  type
                , ignoredTypes
                , resultTypes
                , out var propRefs
                , out var fieldRefs
                , token
            );

            using var baseTypeBuilder = ImmutableArrayBuilder<BaseDataSpec>.Rent();

            var baseType = (type as INamedTypeSymbol)?.BaseType;

            while (baseType != null)
            {
                token.ThrowIfCancellationRequested();

                if (baseType.TypeKind != TypeKind.Class || baseType.HasAttribute(DATA_ATTRIBUTE, token) == false)
                {
                    break;
                }

                ExtractMemberModels(
                      baseType
                    , ignoredTypes
                    , resultTypes
                    , out var basePropRefs
                    , out var baseFieldRefs
                    , token
                );

                baseTypeBuilder.Add(new BaseDataSpec {
                    fullName = baseType.ToFullName(),
                    simpleName = baseType.Name,
                    validIdentifier = baseType.ToValidIdentifier(),
                    isValueType = baseType.IsValueType,
                    propRefs = basePropRefs,
                    fieldRefs = baseFieldRefs,
                });

                baseType = baseType.BaseType;
            }

            token.ThrowIfCancellationRequested();

            var baseTypes = baseTypeBuilder.ToImmutable();
            using var reversedBaseTypeBuilder = ImmutableArrayBuilder<BaseDataSpec>.Rent();

            for (var i = baseTypes.Length - 1; i >= 0; i--)
            {
                token.ThrowIfCancellationRequested();

                reversedBaseTypeBuilder.Add(baseTypes[i]);
            }

            return new DataSpec {
                fullName = type.ToFullName(),
                simpleName = type.Name,
                validIdentifier = type.ToValidIdentifier(),
                isValueType = type.IsValueType,
                propRefs = propRefs,
                fieldRefs = fieldRefs,
                baseTypeRefs = reversedBaseTypeBuilder.ToImmutable().AsEquatableArray(),
            };
        }

        private static void ExtractMemberModels(
              ITypeSymbol type
            , IgnoredTypes ignoredTypes
            , ResultTypes resultTypes
            , out EquatableArray<MemberSpec> propRefs
            , out EquatableArray<MemberSpec> fieldRefs
            , CancellationToken token
        )
        {
            token.ThrowIfCancellationRequested();

            var members = type.GetMembers();
            var memberLength = members.Length;

            var properties = new List<MemberInfo>(memberLength);
            var fields = new List<MemberInfo>(memberLength);
            var fieldMap = new Dictionary<string, IFieldSymbol>(memberLength, StringComparer.Ordinal);
            var propertyMap = new Dictionary<string, IPropertySymbol>(memberLength, StringComparer.Ordinal);

            foreach (var member in members)
            {
                token.ThrowIfCancellationRequested();

                if (member is IPropertySymbol property)
                {
                    if (propertyMap.ContainsKey(member.Name) == false)
                    {
                        propertyMap.Add(member.Name, property);
                    }

                    var attributes = property.GetAttributes();

                    foreach (var attribute in attributes)
                    {
                        token.ThrowIfCancellationRequested();

                        if (attribute.AttributeClass.HasFullName(GENERATED_PROPERTY_FROM_FIELD, token)
                            && attribute.ConstructorArguments.Length > 1
                            && attribute.ConstructorArguments[0].Value is string fieldName
                            && attribute.ConstructorArguments[1].Value is ITypeSymbol genFieldType
                        )
                        {
                            properties.Add(new MemberInfo {
                                propertyName = property.Name,
                                fieldName = fieldName,
                                member = property,
                                fieldType = genFieldType,
                                isGenerated = true,
                            });
                            break;
                        }

                        if (attribute.AttributeClass.HasFullName(DATA_PROPERTY_ATTRIBUTE, token))
                        {
                            ITypeSymbol fieldType;

                            if (attribute.ConstructorArguments.Length > 0
                                && attribute.ConstructorArguments[0].Value is ITypeSymbol ft
                            )
                            {
                                fieldType = ft;
                            }
                            else
                            {
                                fieldType = property.Type;
                            }

                            fields.Add(new MemberInfo {
                                propertyName = property.Name,
                                fieldName = property.ToPrivateFieldName(),
                                member = property,
                                fieldType = fieldType,
                            });
                            break;
                        }
                    }

                    continue;
                }

                if (member is IFieldSymbol field)
                {
                    if (fieldMap.ContainsKey(member.Name) == false)
                    {
                        fieldMap.Add(member.Name, field);
                    }

                    var attributes = field.GetAttributes();

                    foreach (var attribute in attributes)
                    {
                        token.ThrowIfCancellationRequested();

                        if (attribute.AttributeClass.HasFullName(GENERATED_FIELD_FROM_PROPERTY, token)
                            && attribute.ConstructorArguments.Length > 0
                            && attribute.ConstructorArguments[0].Value is string propName
                        )
                        {
                            fields.Add(new MemberInfo {
                                propertyName = propName,
                                fieldName = field.Name,
                                member = field,
                                fieldType = field.Type,
                                isGenerated = true,
                            });
                            break;
                        }

                        if (attribute.AttributeClass.HasFullName(SERIALIZE_FIELD_ATTRIBUTE, token))
                        {
                            fields.Add(new MemberInfo {
                                propertyName = field.ToPropertyName(),
                                fieldName = field.Name,
                                member = field,
                                fieldType = field.Type,
                            });
                            break;
                        }
                    }

                    continue;
                }
            }

            token.ThrowIfCancellationRequested();

            using var propBuilder = ImmutableArrayBuilder<MemberSpec>.Rent();
            using var fieldBuilder = ImmutableArrayBuilder<MemberSpec>.Rent();

            var uniqueFieldNames = new HashSet<string>(StringComparer.Ordinal);

            foreach (var member in properties)
            {
                token.ThrowIfCancellationRequested();

                if (uniqueFieldNames.Contains(member.fieldName))
                {
                    continue;
                }

                ISymbol sourceMemberSymbol = null;

                if (member.isGenerated && fieldMap.TryGetValue(member.fieldName, out var field))
                {
                    sourceMemberSymbol = field;
                }

                uniqueFieldNames.Add(member.fieldName);
                propBuilder.Add(BuildMemberModel(
                      member.propertyName
                    , member.member
                    , sourceMemberSymbol
                    , member.fieldType
                    , ignoredTypes
                    , resultTypes
                    , token
                ));
            }

            token.ThrowIfCancellationRequested();

            foreach (var member in fields)
            {
                token.ThrowIfCancellationRequested();

                if (uniqueFieldNames.Contains(member.fieldName))
                {
                    continue;
                }

                ISymbol generatedFromMember = null;

                if (member.isGenerated && propertyMap.TryGetValue(member.propertyName, out var property))
                {
                    generatedFromMember = property;
                }

                uniqueFieldNames.Add(member.fieldName);
                fieldBuilder.Add(BuildMemberModel(
                      member.propertyName
                    , member.member
                    , generatedFromMember
                    , member.fieldType
                    , ignoredTypes
                    , resultTypes
                    , token
                ));
            }

            propRefs = propBuilder.ToImmutable().AsEquatableArray();
            fieldRefs = fieldBuilder.ToImmutable().AsEquatableArray();
        }

        private static MemberSpec BuildMemberModel(
              string propertyName
            , ISymbol memberSymbol
            , ISymbol sourceMemberSymbol
            , ITypeSymbol fieldType
            , IgnoredTypes ignoredTypes
            , ResultTypes resultTypes
            , CancellationToken token
        )
        {
            token.ThrowIfCancellationRequested();

            TryAddToResultTypes(fieldType, ignoredTypes, resultTypes);

            var manualAuthoring = ExtractManualAuthoring(
                  memberSymbol
                , ignoredTypes
                , resultTypes
                , token
            );

            if (manualAuthoring.defined == false && sourceMemberSymbol != null)
            {
                manualAuthoring = ExtractManualAuthoring(
                      sourceMemberSymbol
                    , ignoredTypes
                    , resultTypes
                    , token
                );
            }

            var memberConverter = TryMakeConverterModel(
                  memberSymbol
                , fieldType
                , ignoredTypes
                , resultTypes
                , token
            );

            if (memberConverter.kind == ConverterKind.None && sourceMemberSymbol != null)
            {
                memberConverter = TryMakeConverterModel(
                      sourceMemberSymbol
                    , fieldType
                    , ignoredTypes
                    , resultTypes
                    , token
                );
            }

            var localConverter = TryFallbackConverterModel(
                  fieldType
                , ignoredTypes
                , resultTypes
                , token
            );

            return new MemberSpec {
                propertyName = propertyName,
                manualAuthoring = manualAuthoring,
                type = MakeTypeModel(fieldType, token),
                collection = MakeCollectionModel(fieldType, ignoredTypes, resultTypes, token),
                memberConverter = memberConverter,
                localConverter = localConverter,
            };
        }

        private static ConverterSpec TryFallbackConverterModel(
              ITypeSymbol fieldType
            , IgnoredTypes ignoredTypes
            , ResultTypes resultTypes
            , CancellationToken token
        )
        {
            if (fieldType is not INamedTypeSymbol namedReturn)
            {
                return default;
            }

            if (TryGetConvertMethod(namedReturn, token, out var convertMethod, namedReturn) == false)
            {
                return default;
            }

            return MakeConverterModelFromMethod(namedReturn, convertMethod, ignoredTypes, resultTypes, token);
        }

        private static bool IsStringSourceConverter(ConverterSpec converter)
            => converter.sourceCollection.kind == CollectionKind.NotCollection
            && converter.sourceType.fullName == "string";

        private static TypeSpec MakeTypeModel(ITypeSymbol type, CancellationToken token)
            => new() {
                fullName = type.ToFullName(),
                simpleName = type.Name,
                isValueType = type.IsValueType,
                hasParameterlessConstructor = CheckParameterlessConstructor(type, token),
            };

        private static void EnqueueTypes(
              Queue<ITypeSymbol> queue
            , HashSet<ITypeSymbol> types
            , CancellationToken token
        )
        {
            token.ThrowIfCancellationRequested();

            foreach (var type in types)
            {
                token.ThrowIfCancellationRequested();

                if (type != null)
                {
                    queue.Enqueue(type);
                }
            }
        }

        private static bool CheckParameterlessConstructor(ITypeSymbol type, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

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
                token.ThrowIfCancellationRequested();

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
            , IgnoredTypes ignoredTypes
            , ResultTypes resultTypes
            , CancellationToken token
        )
        {
            if (type is IArrayTypeSymbol arrayType)
            {
                TryAddToResultTypes(arrayType.ElementType, ignoredTypes, resultTypes);

                return new() {
                    kind = CollectionKind.Array,
                    elementType = MakeTypeModel(arrayType.ElementType, token),
                };
            }

            if (type is INamedTypeSymbol namedType)
            {
                if (namedType.TryGetGenericType(LIST_FAST_TYPE_T, 1, out var listFastType, token))
                {
                    return MakeListModel(
                          listFastType.TypeArguments[0]
                        , ignoredTypes
                        , resultTypes
                        , token
                    );
                }

                if (namedType.TryGetGenericType(LIST_TYPE_T, 1, out var listType, token))
                {
                    return MakeListModel(
                          listType.TypeArguments[0]
                        , ignoredTypes
                        , resultTypes
                        , token
                    );
                }

                if (namedType.TryGetGenericType(DICTIONARY_TYPE_T, 2, out var dictType, token))
                {
                    return MakeDictModel(
                          dictType.TypeArguments[0]
                        , dictType.TypeArguments[1]
                        , ignoredTypes
                        , resultTypes
                        , token
                    );
                }

                if (namedType.TryGetGenericType(HASH_SET_TYPE_T, 1, out var hashSetType, token))
                {
                    return MakeKindModel(
                          CollectionKind.HashSet
                        , hashSetType.TypeArguments[0]
                        , ignoredTypes
                        , resultTypes
                        , token
                    );
                }

                if (namedType.TryGetGenericType(QUEUE_TYPE_T, 1, out var queueType, token))
                {
                    return MakeKindModel(
                          CollectionKind.Queue
                        , queueType.TypeArguments[0]
                        , ignoredTypes
                        , resultTypes
                        , token
                    );
                }

                if (namedType.TryGetGenericType(STACK_TYPE_T, 1, out var stackType, token))
                {
                    return MakeKindModel(
                          CollectionKind.Stack
                        , stackType.TypeArguments[0]
                        , ignoredTypes
                        , resultTypes
                        , token
                    );
                }

                if (namedType.TryGetGenericType(READONLY_MEMORY_TYPE_T, 1, out var readMemType, token))
                {
                    return MakeKindModel(
                          CollectionKind.Array
                        , readMemType.TypeArguments[0]
                        , ignoredTypes
                        , resultTypes
                        , token
                    );
                }

                if (namedType.TryGetGenericType(MEMORY_TYPE_T, 1, out var memType, token))
                {
                    return MakeKindModel(
                          CollectionKind.Array
                        , memType.TypeArguments[0]
                        , ignoredTypes
                        , resultTypes
                        , token
                    );
                }

                if (namedType.TryGetGenericType(READONLY_SPAN_TYPE_T, 1, out var readSpanType, token))
                {
                    return MakeKindModel(
                          CollectionKind.Array
                        , readSpanType.TypeArguments[0]
                        , ignoredTypes
                        , resultTypes
                        , token
                    );
                }

                if (namedType.TryGetGenericType(SPAN_TYPE_T, 1, out var spanType, token))
                {
                    return MakeKindModel(
                          CollectionKind.Array
                        , spanType.TypeArguments[0]
                        , ignoredTypes
                        , resultTypes
                        , token
                    );
                }

                if (namedType.TryGetGenericType(IREADONLY_LIST_TYPE_T, 1, out var roListType, token))
                {
                    return MakeListModel(
                          roListType.TypeArguments[0]
                        , ignoredTypes
                        , resultTypes
                        , token
                    );
                }

                if (namedType.TryGetGenericType(ILIST_TYPE_T, 1, out var ilistType, token))
                {
                    return MakeListModel(
                          ilistType.TypeArguments[0]
                        , ignoredTypes
                        , resultTypes
                        , token
                    );
                }

                if (namedType.TryGetGenericType(ISET_TYPE_T, 1, out var isetType, token))
                {
                    return MakeKindModel(
                          CollectionKind.HashSet
                        , isetType.TypeArguments[0]
                        , ignoredTypes
                        , resultTypes
                        , token
                    );
                }

                if (namedType.TryGetGenericType(IREADONLY_DICTIONARY_TYPE_T, 2, out var roDictType, token))
                {
                    return MakeDictModel(
                          roDictType.TypeArguments[0]
                        , roDictType.TypeArguments[1]
                        , ignoredTypes
                        , resultTypes
                        , token
                    );
                }

                if (namedType.TryGetGenericType(IDICTIONARY_TYPE_T, 2, out var idictType, token))
                {
                    return MakeDictModel(
                          idictType.TypeArguments[0]
                        , idictType.TypeArguments[1]
                        , ignoredTypes
                        , resultTypes
                        , token
                    );
                }
            }

            return default;

            static CollectionSpec MakeListModel(
                  ITypeSymbol elem
                , IgnoredTypes ignoredTypes
                , ResultTypes resultTypes
                , CancellationToken token
            )
            {
                TryAddToResultTypes(elem, ignoredTypes, resultTypes);

                return new() {
                    kind = CollectionKind.List,
                    elementType = MakeTypeModel(elem, token),
                };
            }

            static CollectionSpec MakeKindModel(
                  CollectionKind k
                , ITypeSymbol elem
                , IgnoredTypes ignoredTypes
                , ResultTypes resultTypes
                , CancellationToken token
            )
            {
                TryAddToResultTypes(elem, ignoredTypes, resultTypes);

                return new() {
                    kind = k,
                    elementType = MakeTypeModel(elem, token),
                };
            }

            static CollectionSpec MakeDictModel(
                  ITypeSymbol key
                , ITypeSymbol elem
                , IgnoredTypes ignoredTypes
                , ResultTypes resultTypes
                , CancellationToken token
            )
            {
                TryAddToResultTypes(key, ignoredTypes, resultTypes);
                TryAddToResultTypes(elem, ignoredTypes, resultTypes);

                return new() {
                    kind = CollectionKind.Dictionary,
                    keyType = MakeTypeModel(key, token),
                    elementType = MakeTypeModel(elem, token),
                };
            }
        }

        private static MemberManualAuthoring ExtractManualAuthoring(
              ISymbol memberSymbol
            , IgnoredTypes ignoredTypes
            , ResultTypes resultTypes
            , CancellationToken token
        )
        {
            token.ThrowIfCancellationRequested();

            var attrib = memberSymbol.GetAttribute(DATA_MANUAL_AUTHORING_ATTRIBUTE, token);

            if (attrib == null)
            {
                return default;
            }

            var result = new MemberManualAuthoring() { defined = true };

            foreach (var arg in attrib.ConstructorArguments)
            {
                token.ThrowIfCancellationRequested();

                if (arg.Kind == TypedConstantKind.Type && arg.Value is ITypeSymbol type)
                {
                    TryAddToResultTypes(type, ignoredTypes, resultTypes);

                    result.type = MakeTypeModel(type, token);
                    result.collection = MakeCollectionModel(type, ignoredTypes, resultTypes, token);
                }
            }

            return result;
        }

        private static ConverterSpec TryMakeConverterModel(
              ISymbol memberSymbol
            , ITypeSymbol targetType
            , IgnoredTypes ignoredTypes
            , ResultTypes resultTypes
            , CancellationToken token
        )
        {
            token.ThrowIfCancellationRequested();

            var attrib = memberSymbol.GetAttribute(DATA_AUTHORING_CONVERTER_ATTRIBUTE, token);

            if (attrib == null || attrib.ConstructorArguments.Length != 1)
            {
                return default;
            }

            if (attrib.ConstructorArguments[0].Value is not INamedTypeSymbol converterType)
            {
                return default;
            }

            if (TryGetConvertMethod(converterType, token, out var convertMethod, targetType) == false)
            {
                return default;
            }

            return MakeConverterModelFromMethod(converterType, convertMethod, ignoredTypes, resultTypes, token);
        }

        private static ConverterSpec MakeConverterModelFromMethod(
              INamedTypeSymbol converterType
            , IMethodSymbol convertMethod
            , IgnoredTypes ignoredTypes
            , ResultTypes resultTypes
            , CancellationToken token
        )
        {
            var sourceType = convertMethod.Parameters[0].Type;
            var sourceCollection = MakeCollectionModel(sourceType, ignoredTypes, resultTypes, token);
            var destType = convertMethod.ReturnType;
            MakeCollectionModel(destType, ignoredTypes, resultTypes, token);

            TryAddToResultTypes(sourceType, ignoredTypes, resultTypes);
            TryAddToResultTypes(destType, ignoredTypes, resultTypes);

            return new ConverterSpec {
                kind = convertMethod.IsStatic ? ConverterKind.Static : ConverterKind.Instance,
                converterTypeFullName = converterType.ToFullName(),
                sourceCollection = sourceCollection,
                sourceType = MakeTypeModel(sourceType, token),
                destType = MakeTypeModel(destType, token),
            };
        }

        private static bool TryGetConvertMethod(
              INamedTypeSymbol converterType
            , CancellationToken token
            , out IMethodSymbol convertMethod
            , ITypeSymbol returnType = null
        )
        {
            token.ThrowIfCancellationRequested();

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
                    token.ThrowIfCancellationRequested();

                    if (ctor is IMethodSymbol m
                        && m.DeclaredAccessibility == Accessibility.Public
                        && m.Parameters.Length == 0
                    )
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

            token.ThrowIfCancellationRequested();

            IMethodSymbol staticMethod = null;
            IMethodSymbol instanceMethod = null;
            var multipleStatic = false;
            var multipleInstance = false;

            foreach (var member in converterType.GetMembers("Convert"))
            {
                token.ThrowIfCancellationRequested();

                if (member is not IMethodSymbol method
                    || method.IsGenericMethod
                    || method.DeclaredAccessibility != Accessibility.Public
                )
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

            token.ThrowIfCancellationRequested();

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
            , IgnoredTypes ignoredTypes
            , ResultTypes resultTypes
            , CancellationToken token
        )
        {
            token.ThrowIfCancellationRequested();

            if (values.IsDefaultOrEmpty)
            {
                return;
            }

            for (var i = 0; i < values.Length; i++)
            {
                token.ThrowIfCancellationRequested();

                if (values[i].Value is not INamedTypeSymbol type)
                {
                    continue;
                }

                if (TryGetConvertMethod(type, token, out var convertMethod) == false)
                {
                    continue;
                }

                var targetTypeFullName = convertMethod.ReturnType.ToFullName();

                if (converterMap.ContainsKey(targetTypeFullName) == false)
                {
                    converterMap[targetTypeFullName] = MakeConverterModelFromMethod(
                          type
                        , convertMethod
                        , ignoredTypes
                        , resultTypes
                        , token
                    );
                }
            }
        }

        private static void TryAddToResultTypes(ITypeSymbol type, IgnoredTypes ignored, ResultTypes result)
        {
            if (ignored.Contains(type) == false)
            {
                result.Add(type);
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
            public Dictionary<string, ConverterSpec> tableConverterMap;
            public NameCasing nameCasing;
            public HashValue64 scopeKey;
            public EquatableArray<string> nestedDataTypeFullNames;
        }

        private struct SheetGroupInfo
        {
            public string baseSheetName;
            public List<SheetInfoSpec> sheets;
        }

        private struct MemberInfo
        {
            public string propertyName;
            public string fieldName;
            public ISymbol member;
            public ITypeSymbol fieldType;
            public bool isGenerated;
        }

        private sealed class IgnoredTypes : HashSet<ITypeSymbol>
        {
            public IgnoredTypes() : base(SymbolEqualityComparer.Default)
            {

            }
        }

        private sealed class ResultTypes : HashSet<ITypeSymbol>
        {
            public ResultTypes() : base(SymbolEqualityComparer.Default)
            {
            }
        }
    }
}
