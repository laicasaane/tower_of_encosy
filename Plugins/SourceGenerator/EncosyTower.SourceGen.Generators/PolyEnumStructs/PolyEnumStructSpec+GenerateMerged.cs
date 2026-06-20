using System;
using System.Collections.Generic;
using System.Text;
using EncosyTower.SourceGen.Generators.EnumExtensions;

namespace EncosyTower.SourceGen.Generators.PolyEnumStructs
{
    partial struct PolyEnumStructSpec
    {
        public struct StructRef
        {
            public StructSpec Value { get; set; }

            public Dictionary<string, string> FieldToMergedFieldMap { get; set; }
        }

        public struct MergedFieldRef
        {
            public FieldSpec Value { get; set; }

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

            public Dictionary<PropertyDeclaration, bool> PropertyDimMap { get; } = new();

            public Dictionary<IndexerDeclaration, bool> IndexerDimMap { get; } = new();

            public Dictionary<MethodDeclaration, bool> MethodDimMap { get; } = new();

            public Dictionary<TypeSpec, Dictionary<ConstructionValue, StructId>> TypeValueToStructMap { get; } = new();

            public Dictionary<TypeSpec, HashSet<StructId>> TypeToStructsMap { get; } = new();

            public Dictionary<StructId, Dictionary<TypeSpec, List<ConstructionValue>>> StructToValuesMap { get; } = new();

            public int Size { get; set; }

            public int GetMaxCount()
                => Math.Max(Math.Max(PropertyDimMap.Count, IndexerDimMap.Count), MethodDimMap.Count);
        }

        public class PartialInterfaceRef
        {
            public List<PropertyDeclaration> Properties { get; } = new();

            public List<IndexerDeclaration> Indexers { get; } = new();

            public List<MethodDeclaration> Methods { get; } = new();
        }

        public class DimCollections
        {
            public List<PropertyDeclaration> Properties { get; } = new();

            public List<IndexerDeclaration> Indexers { get; } = new();

            public List<MethodDeclaration> Methods { get; } = new();
        }

        public class EnumCaseType
        {
            public string UnderlyingType { get; set; }

            public int ByteOffset { get; set; }

            public int MaxByteCount { get; set; }

            public List<EnumMemberSpec> Members { get; } = new();
        }

        private readonly void GenerateMerged(
              out List<StructRef> structRefs
            , out MergedStructRef mergedStructRef
            , out PartialInterfaceRef partialInterfaceRef
            , out string undefinedType
            , out EnumCaseType enumCaseType
        )
        {
            var structs = this.structs;
            var structCount = structs.Count;

            var enumCaseSize = (ulong)(structCount + 1) switch {
                > uint.MaxValue => 8,
                > ushort.MaxValue => 4,
                > byte.MaxValue => 2,
                _ => 1,
            };

            structRefs = new List<StructRef>(structCount);
            mergedStructRef = new MergedStructRef { Size = enumCaseSize };
            partialInterfaceRef = new PartialInterfaceRef();
            enumCaseType = new EnumCaseType {
                UnderlyingType = (ulong)(structCount + 1) switch {
                    > uint.MaxValue => "ulong",
                    > ushort.MaxValue => "uint",
                    > byte.MaxValue => "ushort",
                    _ => "byte",
                },
                ByteOffset = (ulong)(structCount + 1) switch {
                    > uint.MaxValue => 8,
                    > ushort.MaxValue => 4,
                    > byte.MaxValue => 2,
                    _ => 1,
                },
            };

            mergedStructRef.FieldRefs.Add(new MergedFieldRef {
                Name = "enumCase",
                Value = new FieldSpec {
                    name = "enumCase",
                    returnType = new TypeSpec { name = "EnumCase", isEnum = true },
                    size = enumCaseSize,
                }
            });

            var propertySignatures = new HashSet<PropertySignature>();
            var indexerSignatures = new HashSet<IndexerSignature>();
            var methodSignatures = new HashSet<MethodSignature>();

            Aggregator.AggregateDimMap(interfaceDef.properties, mergedStructRef.PropertyDimMap, propertySignatures, true);
            Aggregator.AggregateDimMap(interfaceDef.indexers, mergedStructRef.IndexerDimMap, indexerSignatures, true);
            Aggregator.AggregateDimMap(interfaceDef.methods, mergedStructRef.MethodDimMap, methodSignatures, true);

            var usedIndexesInList = new HashSet<int>();
            var fieldRefPool = new Queue<MergedFieldRef>();

            var propertyCountMap = new Dictionary<PropertyDeclaration, int>(structCount);
            var indexerCountMap = new Dictionary<IndexerDeclaration, int>(structCount);
            var methodCountMap = new Dictionary<MethodDeclaration, int>(structCount);
            var enumCaseMembers = enumCaseType.Members;
            var mergedStructSize = 0;
            var structNameByteCountMax = 0;

            enumCaseMembers.Add(new EnumMemberSpec {
                name = UNDEFINED_NAME,
                displayName = UNDEFINED_NAME,
                order = 0,
            });

            var lastStructIndex = structCount - 1;

            for (var i = 0; i < structCount; i++)
            {
                var structRef = new StructRef() {
                    Value = structs[i],
                    FieldToMergedFieldMap = new(),
                };

                var @struct = structRef.Value;

                Aggregator.AggregateMergedFieldRefs(
                      ref structRef
                    , mergedStructRef.FieldRefs
                    , usedIndexesInList
                    , fieldRefPool
                    , ref mergedStructSize
                );

                Aggregator.AggregateCountMap(@struct.properties, propertyCountMap, propertySignatures);
                Aggregator.AggregateCountMap(@struct.indexers, indexerCountMap, indexerSignatures);
                Aggregator.AggregateCountMap(@struct.methods, methodCountMap, methodSignatures);
                Aggregator.AggregateConstructionMaps(@struct, mergedStructRef);

                if (i < lastStructIndex)
                {
                    structNameByteCountMax = Math.Max(structNameByteCountMax, Encoding.UTF8.GetByteCount(@struct.name));
                    enumCaseMembers.Add(new EnumMemberSpec {
                        name = @struct.name,
                        displayName = @struct.displayName,
                        order = (ulong)(i + 1),
                    });
                }

                structRefs.Add(structRef);
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

            enumCaseType.MaxByteCount = structNameByteCountMax;
            mergedStructRef.Size += mergedStructSize;

            if (sortFieldsBySize)
            {
                mergedStructRef.FieldRefs.Sort(MergedFieldRef.SizeComparer.Default);
            }
        }

        internal readonly struct Aggregator
        {
            public static void AggregateMergedFieldRefs(
                  ref StructRef structRef
                , List<MergedFieldRef> mergedFieldRefs
                , HashSet<int> usedIndexesInList
                , Queue<MergedFieldRef> pool
                , ref int structSize
            )
            {
                usedIndexesInList.Clear();

                var fieldMergedFieldMap = structRef.FieldToMergedFieldMap;
                var parameters = structRef.Value.parameters;
                var parameterCount = parameters.Count;

                for (int i = 0; i < parameterCount; i++)
                {
                    ref readonly var field = ref parameters[i].field;
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
                        mergedField.Name = $"field_{field.returnType.identifier.ToValidIdentifier()}_{newListIndex}";

                        structSize += field.size;
                        mergedFieldRefs.Add(mergedField);
                        usedIndexesInList.Add(newListIndex);
                    }
                    else
                    {
                        mergedField = mergedFieldRefs[matchingListIndex];
                        usedIndexesInList.Add(matchingListIndex);
                    }

                    fieldMergedFieldMap[field.name] = mergedField.Name;
                }

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
                        mergedField.Name = $"field_{field.returnType.identifier.ToValidIdentifier()}_{newListIndex}";

                        structSize += field.size;
                        mergedFieldRefs.Add(mergedField);
                        usedIndexesInList.Add(newListIndex);
                    }
                    else
                    {
                        mergedField = mergedFieldRefs[matchingListIndex];
                        usedIndexesInList.Add(matchingListIndex);
                    }

                    fieldMergedFieldMap[field.name] = mergedField.Name;
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
                , bool alwaysAddToSignature
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

                    if (isDim || alwaysAddToSignature)
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

            public static void AggregateConstructionMaps(StructSpec def, MergedStructRef mergedStructRef)
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
