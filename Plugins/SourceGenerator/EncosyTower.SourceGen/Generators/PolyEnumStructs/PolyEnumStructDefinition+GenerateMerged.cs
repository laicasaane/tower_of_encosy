using System;
using System.Collections.Generic;

namespace EncosyTower.SourceGen.Generators.PolyEnumStructs
{
    partial struct PolyEnumStructDefinition
    {
        public class StructRef
        {
            public StructDefinition Value { get; set; }

            public Dictionary<string, string> FieldToMergedFieldMap { get; } = new(StringComparer.Ordinal);
        }

        public class MergedFieldRef
        {
            public FieldDefinition Value { get; set; }

            public string Name { get; set; }

            public sealed class SizeComparer : IComparer<MergedFieldRef>
            {
                public static readonly SizeComparer Default = new();

                public int Compare(MergedFieldRef x, MergedFieldRef y)
                {
                    return y.Value.size.CompareTo(x.Value.size);
                }
            }
        }

        public class MergedStructRef
        {
            public List<MergedFieldRef> FieldRefs { get; } = new();

            public Dictionary<PropertyDefinition, bool> PropertyDimMap { get; } = new();

            public Dictionary<IndexerDefinition, bool> IndexerDimMap { get; } = new();

            public Dictionary<MethodDefinition, bool> MethodDimMap { get; } = new();

            public Dictionary<SlimTypeDefinition, Dictionary<ConstructionValue, StructId>> TypeValueToStructMap { get; } = new();

            public Dictionary<SlimTypeDefinition, HashSet<StructId>> TypeToStructsMap { get; } = new();

            public Dictionary<StructId, Dictionary<SlimTypeDefinition, List<ConstructionValue>>> StructToValuesMap { get; } = new();

            public int Size { get; set; }

            public int GetMaxCount()
                => Math.Max(Math.Max(PropertyDimMap.Count, IndexerDimMap.Count), MethodDimMap.Count);
        }

        public class PartialInterfaceRef
        {
            public List<PropertyDefinition> Properties { get; } = new();

            public List<IndexerDefinition> Indexers { get; } = new();

            public List<MethodDefinition> Methods { get; } = new();
        }

        public class TempCollections
        {
            public List<PropertyDefinition> Properties { get; } = new();

            public List<IndexerDefinition> Indexers { get; } = new();

            public List<MethodDefinition> Methods { get; } = new();
        }

        private readonly void GenerateMerged(
              out List<StructRef> structRefs
            , out MergedStructRef mergedStructRef
            , out PartialInterfaceRef partialInterfaceRef
            , out string undefinedType
            , out string enumCaseType
        )
        {
            var structs = this.structs;
            var structCount = structs.Count;

            enumCaseType = (ulong)(structCount + 1) switch {
                > uint.MaxValue => "ulong",
                > ushort.MaxValue => "uint",
                > byte.MaxValue => "ushort",
                _ => "byte",
            };

            var enumCaseSize = (ulong)(structCount + 1) switch {
                > uint.MaxValue => 8,
                > ushort.MaxValue => 4,
                > byte.MaxValue => 2,
                _ => 1,
            };

            structRefs = new List<StructRef>(structCount);
            mergedStructRef = new MergedStructRef { Size = enumCaseSize };
            partialInterfaceRef = new PartialInterfaceRef();

            mergedStructRef.FieldRefs.Add(new MergedFieldRef {
                Name = "enumCase",
                Value = new FieldDefinition {
                    name = "enumCase",
                    returnType = new SlimTypeDefinition { name = "EnumCase", isEnum = true },
                    size = enumCaseSize,
                }
            });

            var propertySignatures = new HashSet<PropertySignature>();
            var indexerSignatures = new HashSet<IndexerSignature>();
            var methodSignatures = new HashSet<MethodSignature>();

            Aggregator.AggregateDimMap(interfaceDef.properties, mergedStructRef.PropertyDimMap, propertySignatures);
            Aggregator.AggregateDimMap(interfaceDef.indexers, mergedStructRef.IndexerDimMap, indexerSignatures);
            Aggregator.AggregateDimMap(interfaceDef.methods, mergedStructRef.MethodDimMap, methodSignatures);

            var usedIndexesInList = new HashSet<int>();
            var fieldRefPool = new Queue<MergedFieldRef>();

            var propertyCountMap = new Dictionary<PropertyDefinition, int>(structCount);
            var indexerCountMap = new Dictionary<IndexerDefinition, int>(structCount);
            var methodCountMap = new Dictionary<MethodDefinition, int>(structCount);
            var mergedStructSize = 0;

            for (var i = 0; i < structCount; i++)
            {
                var structRef = new StructRef() { Value = structs[i] };
                var @struct = structRef.Value;

                structRefs.Add(structRef);

                Aggregator.AggregateMergedFieldRefs(
                      structRef
                    , mergedStructRef.FieldRefs
                    , usedIndexesInList
                    , fieldRefPool
                    , ref mergedStructSize
                );

                Aggregator.AggregateCountMap(@struct.properties, propertyCountMap, propertySignatures);
                Aggregator.AggregateCountMap(@struct.indexers, indexerCountMap, indexerSignatures);
                Aggregator.AggregateCountMap(@struct.methods, methodCountMap, methodSignatures);
                Aggregator.AggregateConstructionMaps(@struct, mergedStructRef);
            }

            var maxCount = definedUndefinedStruct == DefinedUndefinedStruct.None
                ? structCount - 1
                : structCount;

            Aggregator.CopyMaxCount(propertyCountMap, mergedStructRef.PropertyDimMap, partialInterfaceRef.Properties, maxCount);
            Aggregator.CopyMaxCount(indexerCountMap, mergedStructRef.IndexerDimMap, partialInterfaceRef.Indexers, maxCount);
            Aggregator.CopyMaxCount(methodCountMap, mergedStructRef.MethodDimMap, partialInterfaceRef.Methods, maxCount);

            undefinedType = definedUndefinedStruct switch {
                DefinedUndefinedStruct.Default => "Undefined",
                _ => $"{typeName}_Undefined",
            };

            mergedStructRef.Size += mergedStructSize;

            if (sortFieldsBySize)
            {
                mergedStructRef.FieldRefs.Sort(MergedFieldRef.SizeComparer.Default);
            }
        }

        internal readonly struct Aggregator
        {
            public static void AggregateMergedFieldRefs(
                  StructRef structRef
                , List<MergedFieldRef> mergedFieldRefs
                , HashSet<int> usedIndexesInList
                , Queue<MergedFieldRef> pool
                , ref int structSize
            )
            {
                usedIndexesInList.Clear();

                var fields = structRef.Value.fields;
                var fieldCount = fields.Count;

                for (int i = 0; i < fieldCount; i++)
                {
                    ref readonly var field = ref fields[i];
                    var matchingListIndex = -1;
                    var mergedFieldRefCount = mergedFieldRefs.Count;

                    for (int mergedIndex = 0; mergedIndex < mergedFieldRefCount; mergedIndex++)
                    {
                        if (usedIndexesInList.Contains(mergedIndex) == false)
                        {
                            if (field.returnType.Equals(mergedFieldRefs[mergedIndex].Value.returnType))
                            {
                                matchingListIndex = mergedIndex;
                                break;
                            }
                        }
                    }

                    MergedFieldRef mergedField;

                    if (matchingListIndex < 0)
                    {
                        int newListIndex = mergedFieldRefs.Count;

                        if (pool.Count > 0)
                        {
                            mergedField = pool.Dequeue();
                        }
                        else
                        {
                            mergedField = new MergedFieldRef();
                        }

                        mergedField.Value = field;
                        mergedField.Name = $"field_{field.returnType.name.ToValidIdentifier()}_{newListIndex}";

                        structSize += field.size;
                        mergedFieldRefs.Add(mergedField);
                        usedIndexesInList.Add(newListIndex);
                    }
                    else
                    {
                        mergedField = mergedFieldRefs[matchingListIndex];
                        usedIndexesInList.Add(matchingListIndex);
                    }

                    structRef.FieldToMergedFieldMap[field.name] = mergedField.Name;
                }
            }

            public static void AggregateCountMap<TDef, TSig>(
                  EquatableArray<TDef> items
                , Dictionary<TDef, int> countMap
                , HashSet<TSig> ignoredSignatures
            )
                where TDef : struct, IEquatable<TDef>, ICast<TSig>
                where TSig : struct, IEquatable<TSig>
            {
                var itemCount = items.Count;

                for (var i = 0; i < itemCount; i++)
                {
                    ref readonly var item = ref items[i];

                    if (ignoredSignatures.Contains(item.Cast()))
                    {
                        continue;
                    }

                    if (countMap.TryGetValue(item, out var count) == false)
                    {
                        count = 0;
                    }

                    countMap[item] = count + 1;
                }
            }

            public static void AggregateDimMap<TDef, TSig>(
                  EquatableArray<TDef> items
                , Dictionary<TDef, bool> dimMap
                , HashSet<TSig> signatures
            )
                where TDef : struct, IEquatable<TDef>, ICloneWithDim<TDef>, ICast<TSig>
                where TSig : struct, IEquatable<TSig>, ICloneWithDim<TSig>
            {
                var itemCount = items.Count;

                for (var i = 0; i < itemCount; i++)
                {
                    ref readonly var item = ref items[i];
                    var isDim = item.IsDim;
                    var cloned = item.Clone(false);

                    if (dimMap.ContainsKey(cloned) == false)
                    {
                        dimMap[cloned] = isDim;
                    }

                    if (isDim)
                    {
                        signatures.Add(item.Cast());
                    }
                }
            }

            public static void CopyMaxCount<T>(
                  Dictionary<T, int> countMap
                , Dictionary<T, bool> dimMap
                , List<T> list
                , int maxCount
            )
                where T : struct, IEquatable<T>, ICloneWithDim<T>
            {
                foreach (var kv in countMap)
                {
                    if (kv.Value == maxCount)
                    {
                        var item = kv.Key;
                        var cloned = item.Clone(false);

                        if (dimMap.ContainsKey(cloned) == false)
                        {
                            dimMap[cloned] = item.IsDim;
                            list.Add(cloned);
                        }
                    }
                }
            }

            public static void AggregateConstructionMaps(StructDefinition def, MergedStructRef mergedStructRef)
            {
                var typeValueToStructMap = mergedStructRef.TypeValueToStructMap;
                var typeToStructsMap = mergedStructRef.TypeToStructsMap;
                var structToValuesMap = mergedStructRef.StructToValuesMap;
                var constructions = def.constructions.AsReadOnlySpan();

                foreach (var construction in constructions)
                {
                    var type = construction.type;
                    var value = construction.value;
                    var structId = new StructId {
                        name = def.name,
                        identifier = def.identifier,
                    };

                    if (typeValueToStructMap.TryGetValue(type, out var valueToStruct) == false)
                    {
                        typeValueToStructMap[type] = valueToStruct = new();
                    }

                    if (typeToStructsMap.TryGetValue(type, out var structs) == false)
                    {
                        typeToStructsMap[type] = structs = new();
                    }

                    if (structToValuesMap.TryGetValue(structId, out var typeToValues) == false)
                    {
                        structToValuesMap[structId] = typeToValues = new();
                    }

                    if (valueToStruct.ContainsKey(value) == false)
                    {
                        valueToStruct[value] = structId;
                    }

                    structs.Add(structId);

                    if (typeToValues.TryGetValue(type, out var values) == false)
                    {
                        typeToValues[type] = values = new();
                    }

                    values.Add(value);
                }
            }
        }
    }
}
