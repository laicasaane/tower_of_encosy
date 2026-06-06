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
                || databaseSymbol.GetAttribute(DATABASE_ATTRIBUTE) is not AttributeData databaseAttrib
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

            var syntaxTree = authoringTypeSyntax.SyntaxTree;
            var containerHintName = GetHintName(
                  syntaxTree
                , GENERATOR_NAME
                , authoringTypeSyntax
                , $"{databaseIdentifier}_SheetContainer"
            );

            var dbConverterMap = new Dictionary<string, ConverterSpec>(StringComparer.Ordinal);
            var ignoredTypes = new IgnoredTypes();
            var resultTypes = new ResultTypes();
            var nameCasing = NameCasing.Pascal;
            var fullyQualifiedSheetNames = false;

            ProcessAttributes(
                  authorAttrib
                , databaseAttrib
                , dbConverterMap
                , ignoredTypes
                , resultTypes
                , ref nameCasing
                , ref fullyQualifiedSheetNames
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
            );

            if (tableInfoList.Count < 1)
            {
                return default;
            }

            var dataMap = BuildDataMap(tableInfoList, dbConverterMap, ignoredTypes, resultTypes);
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
            );

            if (sheetList.Count < 1)
            {
                return default;
            }

            using var dataSpecListBuilder = ImmutableArrayBuilder<DataSpec>.Rent();

            foreach (var dm in dataMap.Values)
            {
                dataSpecListBuilder.Add(dm);
            }

            using var tableSpecListBuilder = ImmutableArrayBuilder<TableSpec>.Rent();

            foreach (var tm in tableSpecList)
            {
                tableSpecListBuilder.Add(tm);
            }

            using var sheetGroupListBuilder = ImmutableArrayBuilder<SheetGroupSpec>.Rent();

            foreach (var key in sheetGroupListOrder)
            {
                var assetInfo = sheetGroupMap[key];
                sheetGroupListBuilder.Add(new SheetGroupSpec {
                    baseSheetName = assetInfo.baseSheetName,
                    sheets = assetInfo.sheets.ToImmutableArray().AsEquatableArray(),
                });
            }

            using var typeNamesBuilder = ImmutableArrayBuilder<string>.Rent();

            foreach (var tn in typeNameList)
            {
                typeNamesBuilder.Add(tn);
            }

            using var sheetListBuilder = ImmutableArrayBuilder<SheetSpec>.Rent();

            foreach (var s in sheetList)
            {
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
            , Dictionary<string, ConverterSpec> dbConverterMap
            , IgnoredTypes ignoredTypes
            , ResultTypes resultTypes
            , ref NameCasing nameCasing
            , ref bool fullyQualifiedSheetNames
        )
        {
            foreach (var arg in databaseAttrib.ConstructorArguments)
            {
                if (arg.Kind != TypedConstantKind.Array && arg.Value != null)
                {
                    nameCasing = arg.Value.ToNameCasing();
                    break;
                }
            }

            foreach (var arg in authorAttrib.ConstructorArguments)
            {
                if (arg.Kind == TypedConstantKind.Array)
                {
                    BuildConverterMap(arg.Values, dbConverterMap, ignoredTypes, resultTypes);
                    break;
                }
            }

            foreach (var arg in databaseAttrib.ConstructorArguments)
            {
                if (arg.Kind == TypedConstantKind.Array)
                {
                    BuildConverterMap(arg.Values, dbConverterMap, ignoredTypes, resultTypes);
                    break;
                }
            }

            foreach (var arg in authorAttrib.NamedArguments)
            {
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
        )
        {
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

                var tableNameCasing = nameCasing;
                var tableConverterMap = new Dictionary<string, ConverterSpec>(StringComparer.Ordinal);

                foreach (var arg in tableAttrib.ConstructorArguments)
                {
                    if (arg.Kind != TypedConstantKind.Array && arg.Value != null)
                    {
                        tableNameCasing = arg.Value.ToNameCasing();
                    }
                    else if (arg.Kind == TypedConstantKind.Array)
                    {
                        BuildConverterMap(arg.Values, tableConverterMap, ignoredTypes, resultTypes);
                    }
                }

                var dataType = baseType.TypeArguments[1];
                var idType = FindCorrectIdType(dataType, baseType.TypeArguments[0]);
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

                CollectHorizontalLists(member, tableType, horizontalListMap);
            }

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
        }

        private static ITypeSymbol FindCorrectIdType(ITypeSymbol dataType, ITypeSymbol candidateIdType)
        {
            ISymbol foundMember = null;
            var members = dataType.GetMembers();

            foreach (var member in members)
            {
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

            if (foundMember is IFieldSymbol field)
            {
                return field.Type;
            }

            if (foundMember is not IPropertySymbol property)
            {
                return candidateIdType;
            }

            var attributes = property.GetAttributes();
            ITypeSymbol correctIdType = default;

            foreach (var attribute in attributes)
            {
                if (attribute.ConstructorArguments.Length > 1
                    && attribute.AttributeClass.HasFullName(GENERATED_PROPERTY_FROM_FIELD)
                    && attribute.ConstructorArguments[1].Value is ITypeSymbol found1
                )
                {
                    correctIdType = found1;
                    break;
                }

                if (attribute.ConstructorArguments.Length > 0
                    && attribute.AttributeClass.HasFullName(DATA_PROPERTY_ATTRIBUTE)
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
        )
        {
            // baseSheetName -> dataTypeFullName -> TableInfo list
            var dedupTableInfoMap = new Dictionary<string, Dictionary<string, List<TableInfo>>>(StringComparer.Ordinal);
            var dedupTableAssetMap = new Dictionary<string, bool>(StringComparer.Ordinal);
            var processedTableTypes = new HashSet<string>(StringComparer.Ordinal);

            foreach (var tableInfo in tableInfoList)
            {
                var dataTypeFullName = tableInfo.dataTypeFullName;

                if (dataMap.ContainsKey(dataTypeFullName) == false)
                {
                    continue;
                }

                var baseSheetName = fullyQualifiedSheetNames
                    ? $"{dataTypeFullName.ToValidIdentifier()}Sheet"
                    : $"{tableInfo.dataTypeSimpleName}Sheet";

                if (dedupTableInfoMap.TryGetValue(baseSheetName, out var infoListMap) == false)
                {
                    infoListMap = new Dictionary<string, List<TableInfo>>(1);
                    dedupTableInfoMap[baseSheetName] = infoListMap;
                }

                if (infoListMap.TryGetValue(dataTypeFullName, out var infoList) == false)
                {
                    infoList = new List<TableInfo>(1);
                    infoListMap[dataTypeFullName] = infoList;
                }

                infoList.Add(tableInfo);

                var tableTypeSimpleName = tableInfo.tableTypeSimpleName;
                var existingTableAsset = dedupTableAssetMap.ContainsKey(tableTypeSimpleName);
                dedupTableAssetMap[tableTypeSimpleName] = existingTableAsset;
            }

            foreach (var pair in dedupTableInfoMap)
            {
                var infoListMap = pair.Value;
                var fullyQualified = infoListMap.Count > 1 || fullyQualifiedSheetNames;

                foreach (var infoList in infoListMap.Values)
                {
                    foreach (var tableInfo in infoList)
                    {
                        var tableTypeSimpleName = tableInfo.tableTypeSimpleName;
                        var baseSheetNamePerInfo = fullyQualified
                        ? $"{tableInfo.dataTypeFullName.ToValidIdentifier()}Sheet"
                        : pair.Key;

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

                        var sheetHintName = GetHintName(
                          syntaxTree
                        , GENERATOR_NAME
                        , typeSyntax
                        , $"{databaseIdentifier}_{baseSheetNamePerInfo}"
                    );

                        var nestedFullNames = BuildNestedDataTypeFullNames(tableInfo, dataMap);

                        sheetList.Add(new SheetSpec {
                            hintName = sheetHintName,
                            idTypeFullName = tableInfo.idTypeFullName,
                            idTypeSimpleName = tableInfo.idType.Name,
                            dataTypeFullName = tableInfo.dataTypeFullName,
                            dataTypeSimpleName = tableInfo.dataTypeSimpleName,
                            tableTypeFullName = tableInfo.tableTypeFullName,
                            sheetName = baseSheetNamePerInfo,
                            nestedDataTypeFullNames = nestedFullNames,
                        });
                    }
                }
            }
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
            , IgnoredTypes ignoredTypes
            , ResultTypes resultTypes
        )
        {
            var map = new Dictionary<string, DataSpec>(128, StringComparer.Ordinal);
            var set = new HashSet<ITypeSymbol>(SymbolEqualityComparer.Default);
            var queue = new Queue<ITypeSymbol>(128);

            EnqueueTypes(queue, resultTypes);
            resultTypes.Clear();

            foreach (var tableInfo in tableInfoList)
            {
                queue.Enqueue(tableInfo.idType);
                queue.Enqueue(tableInfo.dataType);

                while (queue.Count > 0)
                {
                    var type = queue.Dequeue();

                    if (type.HasAttribute(DATA_ATTRIBUTE) == false)
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
                        , dbConverterMap
                        , tableInfo.tableConverterMap
                        , ignoredTypes
                        , resultTypes
                    );

                    EnqueueTypes(queue, resultTypes);
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
            , Dictionary<string, ConverterSpec> dbConverterMap
            , Dictionary<string, ConverterSpec> tableConverterMap
            , IgnoredTypes ignoredTypes
            , ResultTypes resultTypes
        )
        {
            ExtractMemberModels(
                  type
                , dbConverterMap
                , tableConverterMap
                , ignoredTypes
                , resultTypes
                , out var propRefs
                , out var fieldRefs
            );

            using var baseTypeBuilder = ImmutableArrayBuilder<BaseDataSpec>.Rent();

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
                    , ignoredTypes
                    , resultTypes
                    , out var basePropRefs
                    , out var baseFieldRefs
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

            var baseTypes = baseTypeBuilder.ToImmutable();
            using var reversedBaseTypeBuilder = ImmutableArrayBuilder<BaseDataSpec>.Rent();

            for (var i = baseTypes.Length - 1; i >= 0; i--)
            {
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
            , Dictionary<string, ConverterSpec> dbConverterMap
            , Dictionary<string, ConverterSpec> tableConverterMap
            , IgnoredTypes ignoredTypes
            , ResultTypes resultTypes
            , out EquatableArray<MemberSpec> propRefs
            , out EquatableArray<MemberSpec> fieldRefs
        )
        {
            var members = type.GetMembers();
            var memberLength = members.Length;

            var properties = new List<MemberInfo>(memberLength);
            var fields = new List<MemberInfo>(memberLength);
            var fieldMap = new Dictionary<string, IFieldSymbol>(memberLength, StringComparer.Ordinal);
            var propertyMap = new Dictionary<string, IPropertySymbol>(memberLength, StringComparer.Ordinal);

            foreach (var member in members)
            {
                if (member is IPropertySymbol property)
                {
                    if (propertyMap.ContainsKey(member.Name) == false)
                    {
                        propertyMap.Add(member.Name, property);
                    }

                    var attributes = property.GetAttributes();

                    foreach (var attribute in attributes)
                    {
                        if (attribute.AttributeClass.HasFullName(GENERATED_PROPERTY_FROM_FIELD)
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

                        if (attribute.AttributeClass.HasFullName(DATA_PROPERTY_ATTRIBUTE))
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
                        if (attribute.AttributeClass.HasFullName(GENERATED_FIELD_FROM_PROPERTY)
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

                        if (attribute.AttributeClass.HasFullName(SERIALIZE_FIELD_ATTRIBUTE))
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

            using var propBuilder = ImmutableArrayBuilder<MemberSpec>.Rent();
            using var fieldBuilder = ImmutableArrayBuilder<MemberSpec>.Rent();

            var uniqueFieldNames = new HashSet<string>(StringComparer.Ordinal);

            foreach (var member in properties)
            {
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
                    , dbConverterMap
                    , tableConverterMap
                    , ignoredTypes
                    , resultTypes
                ));
            }

            foreach (var member in fields)
            {
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
                    , dbConverterMap
                    , tableConverterMap
                    , ignoredTypes
                    , resultTypes
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
            , Dictionary<string, ConverterSpec> dbConverterMap
            , Dictionary<string, ConverterSpec> tableConverterMap
            , IgnoredTypes ignoredTypes
            , ResultTypes resultTypes
        )
        {
            TryAddToResultTypes(fieldType, ignoredTypes, resultTypes);

            var manualAuthoring = ExtractManualAuthoring(memberSymbol, ignoredTypes, resultTypes);

            if (manualAuthoring.defined == false && sourceMemberSymbol != null)
            {
                manualAuthoring = ExtractManualAuthoring(sourceMemberSymbol, ignoredTypes, resultTypes);
            }

            var converter = TryMakeConverterModel(memberSymbol, fieldType, ignoredTypes, resultTypes);

            if (converter.kind == ConverterKind.None && sourceMemberSymbol != null)
            {
                converter = TryMakeConverterModel(sourceMemberSymbol, fieldType, ignoredTypes, resultTypes);
            }

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
                converter = TryFallbackConverterModel(fieldType, ignoredTypes, resultTypes);
            }

            var sheetConverter = default(ConverterSpec);
            var collection = MakeCollectionModel(fieldType, ignoredTypes, resultTypes);

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
                manualAuthoring = manualAuthoring,
                type = MakeTypeModel(fieldType),
                collection = collection,
                converter = converter,
                sheetConverter = sheetConverter,
            };

            static ConverterSpec TryFallbackConverterModel(
                  ITypeSymbol fieldType
                , IgnoredTypes ignoredTypes
                , ResultTypes resultTypes
            )
            {
                if (fieldType is not INamedTypeSymbol namedReturn)
                {
                    return default;
                }

                if (TryGetConvertMethod(namedReturn, out var convertMethod, namedReturn) == false)
                {
                    return default;
                }

                return MakeConverterModelFromMethod(namedReturn, convertMethod, ignoredTypes, resultTypes);
            }
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

        private static void EnqueueTypes(Queue<ITypeSymbol> queue, HashSet<ITypeSymbol> types)
        {
            foreach (var type in types)
            {
                if (type != null)
                {
                    queue.Enqueue(type);
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
            , IgnoredTypes ignoredTypes
            , ResultTypes resultTypes
        )
        {
            if (type is IArrayTypeSymbol arrayType)
            {
                TryAddToResultTypes(arrayType.ElementType, ignoredTypes, resultTypes);

                return new() {
                    kind = CollectionKind.Array,
                    elementType = MakeTypeModel(arrayType.ElementType),
                };
            }

            if (type is INamedTypeSymbol namedType)
            {
                if (namedType.TryGetGenericType(LIST_FAST_TYPE_T, 1, out var listFastType))
                {
                    return MakeListModel(listFastType.TypeArguments[0], ignoredTypes, resultTypes);
                }

                if (namedType.TryGetGenericType(LIST_TYPE_T, 1, out var listType))
                {
                    return MakeListModel(listType.TypeArguments[0], ignoredTypes, resultTypes);
                }

                if (namedType.TryGetGenericType(DICTIONARY_TYPE_T, 2, out var dictType))
                {
                    return MakeDictModel(dictType.TypeArguments[0], dictType.TypeArguments[1], ignoredTypes, resultTypes);
                }

                if (namedType.TryGetGenericType(HASH_SET_TYPE_T, 1, out var hashSetType))
                {
                    return MakeKindModel(CollectionKind.HashSet, hashSetType.TypeArguments[0], ignoredTypes, resultTypes);
                }

                if (namedType.TryGetGenericType(QUEUE_TYPE_T, 1, out var queueType))
                {
                    return MakeKindModel(CollectionKind.Queue, queueType.TypeArguments[0], ignoredTypes, resultTypes);
                }

                if (namedType.TryGetGenericType(STACK_TYPE_T, 1, out var stackType))
                {
                    return MakeKindModel(CollectionKind.Stack, stackType.TypeArguments[0], ignoredTypes, resultTypes);
                }

                if (namedType.TryGetGenericType(READONLY_MEMORY_TYPE_T, 1, out var readMemType))
                {
                    return MakeKindModel(CollectionKind.Array, readMemType.TypeArguments[0], ignoredTypes, resultTypes);
                }

                if (namedType.TryGetGenericType(MEMORY_TYPE_T, 1, out var memType))
                {
                    return MakeKindModel(CollectionKind.Array, memType.TypeArguments[0], ignoredTypes, resultTypes);
                }

                if (namedType.TryGetGenericType(READONLY_SPAN_TYPE_T, 1, out var readSpanType))
                {
                    return MakeKindModel(CollectionKind.Array, readSpanType.TypeArguments[0], ignoredTypes, resultTypes);
                }

                if (namedType.TryGetGenericType(SPAN_TYPE_T, 1, out var spanType))
                {
                    return MakeKindModel(CollectionKind.Array, spanType.TypeArguments[0], ignoredTypes, resultTypes);
                }

                if (namedType.TryGetGenericType(IREADONLY_LIST_TYPE_T, 1, out var roListType))
                {
                    return MakeListModel(roListType.TypeArguments[0], ignoredTypes, resultTypes);
                }

                if (namedType.TryGetGenericType(ILIST_TYPE_T, 1, out var ilistType))
                {
                    return MakeListModel(ilistType.TypeArguments[0], ignoredTypes, resultTypes);
                }

                if (namedType.TryGetGenericType(ISET_TYPE_T, 1, out var isetType))
                {
                    return MakeKindModel(CollectionKind.HashSet, isetType.TypeArguments[0], ignoredTypes, resultTypes);
                }

                if (namedType.TryGetGenericType(IREADONLY_DICTIONARY_TYPE_T, 2, out var roDictType))
                {
                    return MakeDictModel(roDictType.TypeArguments[0], roDictType.TypeArguments[1], ignoredTypes, resultTypes);
                }

                if (namedType.TryGetGenericType(IDICTIONARY_TYPE_T, 2, out var idictType))
                {
                    return MakeDictModel(idictType.TypeArguments[0], idictType.TypeArguments[1], ignoredTypes, resultTypes);
                }
            }

            return default;

            static CollectionSpec MakeListModel(
                  ITypeSymbol elem
                , IgnoredTypes ignoredTypes
                , ResultTypes resultTypes
            )
            {
                TryAddToResultTypes(elem, ignoredTypes, resultTypes);

                return new() {
                    kind = CollectionKind.List,
                    elementType = MakeTypeModel(elem),
                };
            }

            static CollectionSpec MakeKindModel(
                  CollectionKind k
                , ITypeSymbol elem
                , IgnoredTypes ignoredTypes
                , ResultTypes resultTypes
            )
            {
                TryAddToResultTypes(elem, ignoredTypes, resultTypes);

                return new() {
                    kind = k,
                    elementType = MakeTypeModel(elem),
                };
            }

            static CollectionSpec MakeDictModel(
                  ITypeSymbol key
                , ITypeSymbol elem
                , IgnoredTypes ignoredTypes
                , ResultTypes resultTypes
            )
            {
                TryAddToResultTypes(key, ignoredTypes, resultTypes);
                TryAddToResultTypes(elem, ignoredTypes, resultTypes);

                return new() {
                    kind = CollectionKind.Dictionary,
                    keyType = MakeTypeModel(key),
                    elementType = MakeTypeModel(elem),
                };
            }
        }

        private static MemberManualAuthoring ExtractManualAuthoring(
              ISymbol memberSymbol
            , IgnoredTypes ignoredTypes
            , ResultTypes resultTypes
        )
        {
            var attrib = memberSymbol.GetAttribute(DATA_MANUAL_AUTHORING_ATTRIBUTE);

            if (attrib == null)
            {
                return default;
            }

            var result = new MemberManualAuthoring() { defined = true };

            foreach (var arg in attrib.ConstructorArguments)
            {
                if (arg.Kind == TypedConstantKind.Type && arg.Value is ITypeSymbol type)
                {
                    TryAddToResultTypes(type, ignoredTypes, resultTypes);

                    result.type = MakeTypeModel(type);
                    result.collection = MakeCollectionModel(type, ignoredTypes, resultTypes);
                }
            }

            return result;
        }

        private static ConverterSpec TryMakeConverterModel(
              ISymbol memberSymbol
            , ITypeSymbol targetType
            , IgnoredTypes ignoredTypes
            , ResultTypes resultTypes
        )
        {
            var attrib = memberSymbol.GetAttribute(DATA_AUTHORING_CONVERTER_ATTRIBUTE);

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

            return MakeConverterModelFromMethod(converterType, convertMethod, ignoredTypes, resultTypes);
        }

        private static ConverterSpec MakeConverterModelFromMethod(
              INamedTypeSymbol converterType
            , IMethodSymbol convertMethod
            , IgnoredTypes ignoredTypes
            , ResultTypes resultTypes
        )
        {
            var sourceType = convertMethod.Parameters[0].Type;
            var sourceCollection = MakeCollectionModel(sourceType, ignoredTypes, resultTypes);
            var destType = convertMethod.ReturnType;
            MakeCollectionModel(destType, ignoredTypes, resultTypes);

            TryAddToResultTypes(sourceType, ignoredTypes, resultTypes);
            TryAddToResultTypes(destType, ignoredTypes, resultTypes);

            return new ConverterSpec {
                kind = convertMethod.IsStatic ? ConverterKind.Static : ConverterKind.Instance,
                converterTypeFullName = converterType.ToFullName(),
                sourceCollection = sourceCollection,
                sourceType = MakeTypeModel(sourceType),
                destType = MakeTypeModel(destType),
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

            IMethodSymbol staticMethod = null;
            IMethodSymbol instanceMethod = null;
            var multipleStatic = false;
            var multipleInstance = false;

            foreach (var member in converterType.GetMembers("Convert"))
            {
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
                        , ignoredTypes
                        , resultTypes
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

                foreach (var layer in dm.baseTypeRefs)
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
                var manualAuthoring = member.manualAuthoring;

                if (manualAuthoring.defined && manualAuthoring.type.IsValid)
                {
                    Enqueue(manualAuthoring.collection, manualAuthoring.type, dataMap, uniqueTypes, queue);
                }

                Enqueue(member.SelectCollection(), member.SelectType(), dataMap, uniqueTypes, queue);
            }

            static void Enqueue(
                  CollectionSpec collection
                , TypeSpec type
                , Dictionary<string, DataSpec> dataMap
                , HashSet<string> uniqueTypes
                , Queue<string> queue
            )
            {
                if (collection.kind == CollectionKind.Dictionary)
                {
                    TryEnqueue(collection.keyType.fullName, dataMap, uniqueTypes, queue);
                    TryEnqueue(collection.elementType.fullName, dataMap, uniqueTypes, queue);
                }
                else if (collection.kind != CollectionKind.NotCollection)
                {
                    TryEnqueue(collection.elementType.fullName, dataMap, uniqueTypes, queue);
                }
                else
                {
                    TryEnqueue(type.fullName, dataMap, uniqueTypes, queue);
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
