using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using EncosyTower.SourceGen.Helpers.Data;
using Microsoft.CodeAnalysis;

namespace EncosyTower.SourceGen.Generators.DatabaseAuthoring
{
    using static EncosyTower.SourceGen.Generators.DatabaseAuthoring.Helpers;

    partial struct DatabaseSpec
    {
        private static void ExtractConverterAttributes(
              INamedTypeSymbol authoringTypeSymbol
            , List<ConverterForTableEntry> cfgTableEntries
            , List<ConverterForDataPropertyEntry> cfgPropEntries
            , IgnoredTypes ignoredTypes
            , ResultTypes resultTypes
            , CancellationToken token
        )
        {
            token.ThrowIfCancellationRequested();

            foreach (var attrib in authoringTypeSymbol.GetAttributes())
            {
                token.ThrowIfCancellationRequested();

                var attribClass = attrib.AttributeClass;

                if (attribClass == null)
                {
                    continue;
                }

                if (attribClass.HasFullName(CONVERTER_FOR_TABLE_ATTRIBUTE, token))
                {
                    var args = attrib.ConstructorArguments;

                    if (args.Length != 2
                        || args[1].Value is not INamedTypeSymbol converterType
                        || TryGetConvertMethod(converterType, token, out var convertMethod) == false
                    )
                    {
                        continue;
                    }

                    var spec = MakeConverterModelFromMethod(
                          converterType
                        , convertMethod
                        , ignoredTypes
                        , resultTypes
                        , token
                    );

                    string tableName = null;
                    INamedTypeSymbol tableType = null;

                    if (args[0].Value is string nameValue)
                    {
                        tableName = nameValue;
                    }
                    else if (args[0].Value is INamedTypeSymbol typeValue)
                    {
                        tableType = typeValue;
                    }
                    else
                    {
                        continue;
                    }

                    cfgTableEntries.Add(new ConverterForTableEntry {
                        tableName = tableName,
                        tableType = tableType,
                        converter = spec,
                        targetTypeFullName = spec.destType.fullName,
                    });

                    continue;
                }

                if (attribClass.HasFullName(CONVERTER_FOR_DATA_PROPERTY_ATTRIBUTE, token))
                {
                    var args = attrib.ConstructorArguments;

                    if (args.Length < 3
                        || args[0].Value is not INamedTypeSymbol dataType
                        || args[1].Value is not string propertyName
                        || string.IsNullOrWhiteSpace(propertyName)
                        || args[2].Value is not INamedTypeSymbol converterType
                        || TryGetConvertMethod(converterType, token, out var convertMethod) == false
                    )
                    {
                        continue;
                    }

                    var spec = MakeConverterModelFromMethod(
                          converterType
                        , convertMethod
                        , ignoredTypes
                        , resultTypes
                        , token
                    );

                    string tableName = null;
                    INamedTypeSymbol tableType = null;

                    if (args.Length >= 4)
                    {
                        if (args[3].Value is string nameValue)
                        {
                            tableName = nameValue;
                        }
                        else if (args[3].Value is INamedTypeSymbol typeValue)
                        {
                            tableType = typeValue;
                        }
                    }

                    cfgPropEntries.Add(new ConverterForDataPropertyEntry {
                        dataTypeFullName = dataType.ToFullName(),
                        propertyName = propertyName,
                        tableName = tableName,
                        tableType = tableType,
                        converter = spec,
                    });
                }
            }
        }

        private static void ResolveConverters(
              List<TableInfo> tableInfoList
            , Dictionary<string, DataSpec> dataMap
            , Dictionary<string, ConverterSpec> authorDbMap
            , Dictionary<string, ConverterSpec> databaseMap
            , List<ConverterForTableEntry> cfgTableEntries
            , List<ConverterForDataPropertyEntry> cfgPropEntries
            , ref ImmutableArrayBuilder<ScopedConverterSpec> scopedConvertersBuilder
            , CancellationToken token
        )
        {
            token.ThrowIfCancellationRequested();

            var added = new HashSet<ScopedDedupKey>();
            var sb = new StringBuilder(1024);

            for (var t = 0; t < tableInfoList.Count; t++)
            {
                token.ThrowIfCancellationRequested();

                var tableInfo = tableInfoList[t];

                var cfgTable = BuildScopedTableMap(cfgTableEntries, tableInfo);
                var cfgProp = BuildScopedPropMap(cfgPropEntries, tableInfo);

                var temp = new List<ScopedConverterSpec>();
                var nested = new List<string>();
                var visited = new HashSet<string>(StringComparer.Ordinal);
                var queue = new Queue<string>();

                EnqueueRoot(tableInfo.idTypeFullName, dataMap, visited, queue);
                EnqueueRoot(tableInfo.dataTypeFullName, dataMap, visited, queue);

                while (queue.Count > 0)
                {
                    token.ThrowIfCancellationRequested();

                    var full = queue.Dequeue();

                    if (dataMap.TryGetValue(full, out var shape) == false)
                    {
                        continue;
                    }

                    if (string.Equals(full, tableInfo.idTypeFullName, StringComparison.Ordinal) == false
                        && string.Equals(full, tableInfo.dataTypeFullName, StringComparison.Ordinal) == false
                    )
                    {
                        nested.Add(full);
                    }

                    ResolveTypeMembers(
                          shape.fullName
                        , shape.propRefs
                        , cfgProp
                        , cfgTable
                        , authorDbMap
                        , tableInfo.tableConverterMap
                        , databaseMap
                        , dataMap
                        , visited
                        , queue
                        , temp
                        , token
                     );

                    ResolveTypeMembers(
                          shape.fullName
                        , shape.fieldRefs
                        , cfgProp
                        , cfgTable
                        , authorDbMap
                        , tableInfo.tableConverterMap
                        , databaseMap
                        , dataMap
                        , visited
                        , queue
                        , temp
                        , token
                    );

                    foreach (var layer in shape.baseTypeRefs)
                    {
                        ResolveTypeMembers(
                              layer.fullName
                            , layer.propRefs
                            , cfgProp
                            , cfgTable
                            , authorDbMap
                            , tableInfo.tableConverterMap
                            , databaseMap
                            , dataMap
                            , visited
                            , queue
                            , temp
                            , token
                        );

                        ResolveTypeMembers(
                              layer.fullName
                            , layer.fieldRefs
                            , cfgProp
                            , cfgTable
                            , authorDbMap
                            , tableInfo.tableConverterMap
                            , databaseMap
                            , dataMap
                            , visited
                            , queue
                            , temp
                            , token
                        );
                    }
                }

                var scopeKey = ComputeScopeKey(temp, sb, token);

                tableInfo.scopeKey = scopeKey;
                tableInfo.nestedDataTypeFullNames = ToEquatableArray(nested, token);
                tableInfoList[t] = tableInfo;

                foreach (var entry in temp)
                {
                    token.ThrowIfCancellationRequested();

                    var dedupKey = new ScopedDedupKey {
                        scopeKey = scopeKey,
                        declaringDataTypeFullName = entry.declaringDataTypeFullName,
                        propertyName = entry.propertyName,
                    };

                    if (added.Add(dedupKey))
                    {
                        var scopedConverter = entry;
                        scopedConverter.scopeKey = scopeKey;
                        scopedConvertersBuilder.Add(scopedConverter);
                    }
                }
            }
        }

        private static void ResolveTypeMembers(
              string declaringTypeFullName
            , EquatableArray<MemberSpec> members
            , Dictionary<string, ConverterSpec> cfgProp
            , Dictionary<string, ConverterSpec> cfgTable
            , Dictionary<string, ConverterSpec> authorDbMap
            , Dictionary<string, ConverterSpec> tableMap
            , Dictionary<string, ConverterSpec> databaseMap
            , Dictionary<string, DataSpec> dataMap
            , HashSet<string> visited
            , Queue<string> queue
            , List<ScopedConverterSpec> temp
            , CancellationToken token
        )
        {
            foreach (var member in members)
            {
                token.ThrowIfCancellationRequested();

                ResolveMemberConverter(
                      declaringTypeFullName
                    , member
                    , cfgProp
                    , cfgTable
                    , authorDbMap
                    , tableMap
                    , databaseMap
                    , out var converter
                    , out var sheetConverter
                );

                temp.Add(new ScopedConverterSpec {
                    declaringDataTypeFullName = declaringTypeFullName,
                    propertyName = member.propertyName,
                    converter = converter,
                    sheetConverter = sheetConverter,
                });

                var resolvedMember = member;
                resolvedMember.converter = converter;

                var manualAuthoring = member.manualAuthoring;

                if (manualAuthoring.defined && manualAuthoring.type.IsValid)
                {
                    EnqueueSheetSideType(
                          manualAuthoring.collection
                        , manualAuthoring.type
                        , dataMap
                        , visited
                        , queue
                    );
                }

                EnqueueSheetSideType(
                      resolvedMember.SelectCollection()
                    , resolvedMember.SelectType()
                    , dataMap
                    , visited
                    , queue
                );
            }
        }

        private static void ResolveMemberConverter(
              string declaringTypeFullName
            , MemberSpec member
            , Dictionary<string, ConverterSpec> cfgProp
            , Dictionary<string, ConverterSpec> cfgTable
            , Dictionary<string, ConverterSpec> authorDbMap
            , Dictionary<string, ConverterSpec> tableMap
            , Dictionary<string, ConverterSpec> databaseMap
            , out ConverterSpec converter
            , out ConverterSpec sheetConverter
        )
        {
            converter = default;
            sheetConverter = default;

            var targetTypeFullName = member.type.fullName;
            var propKey = ScopedConverterSpec.MakeMemberKey(declaringTypeFullName, member.propertyName);

            if (cfgProp.TryGetValue(propKey, out var c1))
            {
                converter = c1;
            }
            else if (cfgTable.TryGetValue(targetTypeFullName, out var c2))
            {
                converter = c2;
            }
            else if (authorDbMap.TryGetValue(targetTypeFullName, out var c3))
            {
                converter = c3;
            }
            else if (member.memberConverter.kind != ConverterKind.None)
            {
                converter = member.memberConverter;
            }
            else if (tableMap.TryGetValue(targetTypeFullName, out var c5))
            {
                converter = c5;
            }
            else if (databaseMap.TryGetValue(targetTypeFullName, out var c6))
            {
                converter = c6;
            }
            else if (member.localConverter.kind != ConverterKind.None)
            {
                converter = member.localConverter;
            }

            if (member.collection.kind == CollectionKind.NotCollection
                && converter.kind != ConverterKind.None
            )
            {
                if (IsStringSourceConverter(converter))
                {
                    sheetConverter = converter;
                    converter = default;
                }
                else if (converter.sourceCollection.kind == CollectionKind.NotCollection
                    && TryGetStringSourceConverterScoped(
                          converter.sourceType.fullName
                        , cfgTable
                        , authorDbMap
                        , tableMap
                        , databaseMap
                        , out var sourceConverter
                    )
                )
                {
                    sheetConverter = sourceConverter;
                }
            }
        }

        private static bool TryGetStringSourceConverterScoped(
              string targetTypeFullName
            , Dictionary<string, ConverterSpec> cfgTable
            , Dictionary<string, ConverterSpec> authorDbMap
            , Dictionary<string, ConverterSpec> tableMap
            , Dictionary<string, ConverterSpec> databaseMap
            , out ConverterSpec converter
        )
        {
            if (cfgTable.TryGetValue(targetTypeFullName, out converter) && IsStringSourceConverter(converter))
            {
                return true;
            }

            if (authorDbMap.TryGetValue(targetTypeFullName, out converter) && IsStringSourceConverter(converter))
            {
                return true;
            }

            if (tableMap.TryGetValue(targetTypeFullName, out converter) && IsStringSourceConverter(converter))
            {
                return true;
            }

            if (databaseMap.TryGetValue(targetTypeFullName, out converter) && IsStringSourceConverter(converter))
            {
                return true;
            }

            converter = default;
            return false;
        }

        private static Dictionary<string, ConverterSpec> BuildScopedTableMap(
              List<ConverterForTableEntry> entries
            , in TableInfo tableInfo
        )
        {
            var map = new Dictionary<string, ConverterSpec>(StringComparer.Ordinal);

            foreach (var entry in entries)
            {
                if (MatchesTable(entry.tableName, entry.tableType, tableInfo) == false)
                {
                    continue;
                }

                if (map.ContainsKey(entry.targetTypeFullName) == false)
                {
                    map[entry.targetTypeFullName] = entry.converter;
                }
            }

            return map;
        }

        private static Dictionary<string, ConverterSpec> BuildScopedPropMap(
              List<ConverterForDataPropertyEntry> entries
            , in TableInfo tableInfo
        )
        {
            var map = new Dictionary<string, ConverterSpec>(StringComparer.Ordinal);

            foreach (var entry in entries)
            {
                if (entry.tableName != null || entry.tableType != null)
                {
                    continue;
                }

                var key = ScopedConverterSpec.MakeMemberKey(entry.dataTypeFullName, entry.propertyName);

                if (map.ContainsKey(key) == false)
                {
                    map[key] = entry.converter;
                }
            }

            foreach (var entry in entries)
            {
                if (entry.tableName == null && entry.tableType == null)
                {
                    continue;
                }

                if (MatchesTable(entry.tableName, entry.tableType, tableInfo) == false)
                {
                    continue;
                }

                var key = ScopedConverterSpec.MakeMemberKey(entry.dataTypeFullName, entry.propertyName);
                map[key] = entry.converter;
            }

            return map;
        }

        private static bool MatchesTable(string tableName, INamedTypeSymbol tableType, in TableInfo tableInfo)
        {
            if (tableName != null)
            {
                return string.Equals(tableName, tableInfo.propertyName, StringComparison.Ordinal);
            }

            if (tableType != null)
            {
                return SymbolEqualityComparer.Default.Equals(tableType, tableInfo.tableType);
            }

            return false;
        }

        private static void EnqueueRoot(
              string typeFullName
            , Dictionary<string, DataSpec> dataMap
            , HashSet<string> visited
            , Queue<string> queue
        )
        {
            if (string.IsNullOrEmpty(typeFullName))
            {
                return;
            }

            if (dataMap.ContainsKey(typeFullName) && visited.Add(typeFullName))
            {
                queue.Enqueue(typeFullName);
            }
        }

        private static void EnqueueSheetSideType(
              CollectionSpec collection
            , TypeSpec type
            , Dictionary<string, DataSpec> dataMap
            , HashSet<string> visited
            , Queue<string> queue
        )
        {
            if (collection.kind == CollectionKind.Dictionary)
            {
                TryEnqueue(collection.keyType.fullName, dataMap, visited, queue);
                TryEnqueue(collection.elementType.fullName, dataMap, visited, queue);
            }
            else if (collection.kind != CollectionKind.NotCollection)
            {
                TryEnqueue(collection.elementType.fullName, dataMap, visited, queue);
            }
            else
            {
                TryEnqueue(type.fullName, dataMap, visited, queue);
            }
        }

        private static HashValue64 ComputeScopeKey(
              List<ScopedConverterSpec> entries
            , StringBuilder builder
            , CancellationToken token
        )
        {
            token.ThrowIfCancellationRequested();

            if (entries.Count < 1)
            {
                return HashValue64.FNV1a(string.Empty);
            }

            entries.Sort(static (a, b) =>
            {
                var compare = string.CompareOrdinal(a.declaringDataTypeFullName, b.declaringDataTypeFullName);
                return compare != 0 ? compare : string.CompareOrdinal(a.propertyName, b.propertyName);
            });

            builder.Clear();

            foreach (var entry in entries)
            {
                token.ThrowIfCancellationRequested();

                builder.Append(entry.declaringDataTypeFullName).Append(".").Append(entry.propertyName)
                    .Append('=');

                AppendConverterSignature(builder, entry.converter);

                builder.Append('|');

                AppendConverterSignature(builder, entry.sheetConverter);

                builder.Append('\n');
            }

            return HashValue64.FNV1a(builder.ToString());

            static void AppendConverterSignature(StringBuilder builder, in ConverterSpec converter)
            {
                builder.Append((int)converter.kind).Append(':')
                    .Append(converter.converterTypeFullName).Append(':')
                    .Append(converter.sourceType.fullName).Append(':')
                    .Append((int)converter.sourceCollection.kind).Append(':')
                    .Append(converter.destType.fullName);
            }
        }

        private static EquatableArray<string> ToEquatableArray(List<string> values, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            if (values.Count < 1)
            {
                return default;
            }

            using var builder = ImmutableArrayBuilder<string>.Rent();

            for (var i = 0; i < values.Count; i++)
            {
                token.ThrowIfCancellationRequested();

                builder.Add(values[i]);
            }

            return builder.ToImmutable().AsEquatableArray();
        }

        private struct ConverterForTableEntry
        {
            public string tableName;
            public INamedTypeSymbol tableType;
            public ConverterSpec converter;
            public string targetTypeFullName;
        }

        private struct ConverterForDataPropertyEntry
        {
            public string dataTypeFullName;
            public string propertyName;
            public string tableName;
            public INamedTypeSymbol tableType;
            public ConverterSpec converter;
        }

        private struct ScopedDedupKey : IEquatable<ScopedDedupKey>
        {
            public HashValue64 scopeKey;
            public string declaringDataTypeFullName;
            public string propertyName;

            public readonly override int GetHashCode()
                => HashValue.Combine(scopeKey, declaringDataTypeFullName, propertyName);

            public readonly override bool Equals(object obj)
                => obj is ScopedDedupKey other && Equals(other);

            public readonly bool Equals(ScopedDedupKey other)
                => scopeKey == other.scopeKey
                && string.Equals(declaringDataTypeFullName, other.declaringDataTypeFullName, StringComparison.Ordinal)
                && string.Equals(propertyName, other.propertyName, StringComparison.Ordinal)
                ;
        }
    }
}
