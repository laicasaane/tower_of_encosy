#if UNITY_EDITOR

// MIT License
//
// Copyright (c) 2024 Mika Notarnicola
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.
//
// https://github.com/thebeardphantom/Runtime-TypeCache

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using EncosyTower.Modules.Collections;
using EncosyTower.Modules.Logging;
using EncosyTower.Modules.Types.Caches;
using EncosyTower.Modules.Types.Internals;
using UnityEditor;
using UnityEditor.Compilation;

namespace EncosyTower.Modules.Types.Editor
{
    using SystemAssembly = System.Reflection.Assembly;

    internal static class SerializedTypeCacheEditor
    {
        [MenuItem("Encosy Tower/Runtime Type Cache/Print Player Assemblies")]
        private static void PrintAssemblies()
        {
            var names = GetPlayerAssemblyNames();

            foreach (var name in names)
            {
                DevLoggerAPI.LogInfoSlim(name);
            }
        }

        public static void Regenerate(this SerializedTypeCache cache)
        {
            var typesDerivedFromTypeList = cache._typesDerivedFromTypeList = new();
            var typesWithAttributeList = cache._typesWithAttributeList = new();
            var fieldsWithAttributeList = cache._fieldsWithAttributeList = new();
            var methodsWithAttributeList = cache._methodsWithAttributeList = new();
            var typeStore = cache._typeStore = new();

            var playerAssemblyNames = GetPlayerAssemblyNames();
            var baseTypeToAssemblyMap = new Dictionary<Type, HashSet<string>>();
            var typeAttribToAssemblyMap = new Dictionary<Type, HashSet<string>>();
            var fieldAttribToAssemblyMap = new Dictionary<Type, HashSet<string>>();
            var methodAttribToAssemblyMap = new Dictionary<Type, HashSet<string>>();

            var tempTypesDerivedFromTypeList = new List<TempSerializedDerivedTypes>();
            var tempTypesWithAttributeList = new List<TempSerializedAnnonatedMembers<Type>>();
            var tempFieldsWithAttributeList = new List<TempSerializedAnnonatedMembers<FieldInfo>>();
            var tempMethodsWithAttributeList = new List<TempSerializedAnnonatedMembers<MethodInfo>>();
            var tempTypeStore = new TempTypeStore();

            GetTypesFromThisAttribute<CacheTypesDerivedFromThisTypeAttribute>(
                playerAssemblyNames, tempTypeStore, baseTypeToAssemblyMap
            );

            GetTypesFromNonThisAttribute<CacheTypesDerivedFromAttribute>(
                playerAssemblyNames, tempTypeStore, baseTypeToAssemblyMap
            );

            GetTypesFromThisAttribute<CacheTypesWithThisAttributeAttribute>(
                playerAssemblyNames, tempTypeStore, typeAttribToAssemblyMap
            );

            GetTypesFromNonThisAttribute<CacheTypesWithAttributeAttribute>(
                playerAssemblyNames, tempTypeStore, typeAttribToAssemblyMap
            );

            GetTypesFromThisAttribute<CacheFieldsWithThisAttributeAttribute>(
                playerAssemblyNames, tempTypeStore, fieldAttribToAssemblyMap
            );

            GetTypesFromNonThisAttribute<CacheFieldsWithAttributeAttribute>(
                playerAssemblyNames, tempTypeStore, fieldAttribToAssemblyMap
            );

            GetTypesFromThisAttribute<CacheMethodsWithThisAttributeAttribute>(
                playerAssemblyNames, tempTypeStore, methodAttribToAssemblyMap
            );

            GetTypesFromNonThisAttribute<CacheMethodsWithAttributeAttribute>(
                playerAssemblyNames, tempTypeStore, methodAttribToAssemblyMap
            );

            PrepareTypesDerivedFromTypeList(
                playerAssemblyNames, baseTypeToAssemblyMap, tempTypeStore, tempTypesDerivedFromTypeList
            );

            PrepareTypesWithAttributeList(
                playerAssemblyNames, typeAttribToAssemblyMap, tempTypeStore, tempTypesWithAttributeList
            );

            PrepareFieldsWithAttributeList(
                playerAssemblyNames, fieldAttribToAssemblyMap, tempTypeStore, tempFieldsWithAttributeList
            );

            PrepareMethodsWithAttributeList(
                playerAssemblyNames, methodAttribToAssemblyMap, tempTypeStore, tempMethodsWithAttributeList
            );

            BuildTypeStore(typeStore, tempTypeStore);
            BuildTypesDerivedFromTypeList(typesDerivedFromTypeList, tempTypesDerivedFromTypeList);
            BuildTypesWithAttributeList(typesWithAttributeList, tempTypesWithAttributeList);
            BuildFieldsWithAttributeList(fieldsWithAttributeList, tempFieldsWithAttributeList);
            BuildMethodsWithAttributeList(methodsWithAttributeList, tempMethodsWithAttributeList);
        }

        private static bool IsEditor(Type type, string assemblyName, HashSet<string> playerAssemblyNames)
        {
            return playerAssemblyNames.Contains(assemblyName) == false
                || (type.FullName ?? string.Empty).StartsWith("UnityEditorInternal");
        }

        private static HashSet<string> GetPlayerAssemblyNames()
        {
            const AssembliesType ASSEMBLIES_TYPE =
#if UNITY_INCLUDE_TESTS
                AssembliesType.Player
#else
                AssembliesType.PlayerWithoutTestAssemblies
#endif
                ;

            return CompilationPipeline.GetAssemblies(ASSEMBLIES_TYPE)
                .Select(static x => x.name)
                .Concat(CompilationPipeline
                    .GetPrecompiledAssemblyPaths(CompilationPipeline.PrecompiledAssemblySources.UnityEngine)
                    .Select(Path.GetFileNameWithoutExtension)
                    .Where(static x => x.StartsWith("UnityEditor") == false)
                )
                .Concat(CompilationPipeline
                    .GetPrecompiledAssemblyPaths(CompilationPipeline.PrecompiledAssemblySources.SystemAssembly)
                    .Select(Path.GetFileNameWithoutExtension)
                )
                .Concat(CompilationPipeline
                    .GetPrecompiledAssemblyPaths(CompilationPipeline.PrecompiledAssemblySources.UserAssembly)
                    .Select(Path.GetFileNameWithoutExtension)
                )
                .Distinct()
                .OrderBy(static x => x)
                .ToHashSet();
        }

        private static void GetTypesFromThisAttribute<TAttribute>(
              HashSet<string> playerAssemblyNames
            , TempTypeStore typeStore
            , Dictionary<Type, HashSet<string>> typeToAssemblyMap
        )
            where TAttribute : Attribute, ICacheAttributeWithAssemblyNames
        {
            var types = TypeCache.GetTypesWithAttribute<TAttribute>();

            foreach (var type in types)
            {
                var baseTypeAssemblyName = type.Assembly.GetName().Name;

                if (IsEditor(type, baseTypeAssemblyName, playerAssemblyNames))
                {
                    continue;
                }

                if (typeToAssemblyMap.TryGetValue(type, out var assemblies) == false)
                {
                    assemblies = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                    typeToAssemblyMap[type] = assemblies;
                }

                typeStore.Add(type);

                var attribute = type.GetCustomAttribute<TAttribute>();
                var assemblyNames = attribute.AssemblyNames.AsSpan();

                if (assemblyNames.Length < 1)
                {
                    assemblies.Add(string.Empty);
                    continue;
                }

                foreach (var assemblyName in assemblyNames)
                {
                    if (string.IsNullOrWhiteSpace(assemblyName))
                    {
                        assemblies.Add(string.Empty);
                    }
                    else if (playerAssemblyNames.Contains(assemblyName))
                    {
                        assemblies.Add(assemblyName);
                    }
                }
            }
        }

        private static void GetTypesFromNonThisAttribute<TAttribute>(
              HashSet<string> playerAssemblyNames
            , TempTypeStore typeStore
            , Dictionary<Type, HashSet<string>> typeToAssemblyMap
        )
            where TAttribute : Attribute, ICacheAttributeWithType, ICacheAttributeWithAssemblyNames
        {
            var runtimeTypeCacheTypes = TypeCache.GetTypesWithAttribute<TAttribute>();

            foreach (var runtimeTypeCacheType in runtimeTypeCacheTypes)
            {
                var attributes = runtimeTypeCacheType.GetCustomAttributes<TAttribute>();

                foreach (var attribute in attributes)
                {
                    if (attribute.Type == null)
                    {
                        continue;
                    }

                    var baseType = attribute.Type;
                    var baseTypeAssemblyName = baseType.Assembly.GetName().Name;

                    if (IsEditor(baseType, baseTypeAssemblyName, playerAssemblyNames))
                    {
                        continue;
                    }

                    if (typeToAssemblyMap.TryGetValue(baseType, out var assemblies) == false)
                    {
                        assemblies = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                        typeToAssemblyMap[baseType] = assemblies;
                    }

                    typeStore.Add(baseType);

                    var assemblyNames = attribute.AssemblyNames.AsSpan();

                    if (assemblyNames.Length < 1)
                    {
                        assemblies.Add(string.Empty);
                        continue;
                    }

                    foreach (var assemblyName in assemblyNames)
                    {
                        if (string.IsNullOrWhiteSpace(assemblyName))
                        {
                            assemblies.Add(string.Empty);
                        }
                        else if (playerAssemblyNames.Contains(assemblyName))
                        {
                            assemblies.Add(assemblyName);
                        }
                    }
                }
            }
        }

        private static void PrepareTypesDerivedFromTypeList(
              HashSet<string> playerAssemblyNames
            , Dictionary<Type, HashSet<string>> assemblyNameMap
            , TempTypeStore typeStore
            , List<TempSerializedDerivedTypes> result
        )
        {
            foreach (var (baseType, assemblyNames) in assemblyNameMap)
            {
                var filteredTypes = new List<Type>();

                foreach (var assemblyName in assemblyNames)
                {
                    filteredTypes.Clear();

                    var candidates = string.IsNullOrEmpty(assemblyName)
                        ? TypeCache.GetTypesDerivedFrom(baseType)
                        : TypeCache.GetTypesDerivedFrom(baseType, assemblyName);

                    foreach (var candidate in candidates)
                    {
                        if (IsEditor(candidate, candidate.Assembly.GetName().Name, playerAssemblyNames) == false)
                        {
                            filteredTypes.Add(candidate);
                        }
                    }

                    if (filteredTypes.Count < 1)
                    {
                        continue;
                    }

                    var item = new TempSerializedDerivedTypes {
                        assemblyName = assemblyName,
                        baseType = typeStore.Add(baseType),
                    };

                    var derivedTypes = item.derivedTypes;
                    derivedTypes.IncreaseCapacityTo(derivedTypes.Count);

                    foreach (var type in filteredTypes)
                    {
                        derivedTypes.Add(typeStore.Add(type));
                    }

                    result.Add(item);
                }
            }
        }

        private static void PrepareTypesWithAttributeList(
              HashSet<string> playerAssemblyNames
            , Dictionary<Type, HashSet<string>> assemblyNameMap
            , TempTypeStore typeStore
            , List<TempSerializedAnnonatedMembers<Type>> result
        )
        {
            foreach (var (attribType, assemblyNames) in assemblyNameMap)
            {
                var filteredTypes = new List<Type>();

                foreach (var assemblyName in assemblyNames)
                {
                    filteredTypes.Clear();

                    var candidates = string.IsNullOrEmpty(assemblyName)
                        ? TypeCache.GetTypesWithAttribute(attribType)
                        : TypeCache.GetTypesWithAttribute(attribType, assemblyName);

                    foreach (var candidate in candidates)
                    {
                        if (IsEditor(candidate, candidate.Assembly.GetName().Name, playerAssemblyNames) == false)
                        {
                            filteredTypes.Add(candidate);
                        }
                    }

                    if (filteredTypes.Count < 1)
                    {
                        continue;
                    }

                    var item = new TempSerializedAnnonatedMembers<Type> {
                        assemblyName = assemblyName,
                        attributeType = typeStore.Add(attribType),
                    };

                    var matches = item.matches;
                    matches.IncreaseCapacityTo(filteredTypes.Count);

                    foreach (var type in filteredTypes)
                    {
                        matches.Add(new MemberRef<Type>(type) {
                            declaringType = typeStore.Add(type),
                        });
                    }

                    result.Add(item);
                }
            }
        }

        private static void PrepareFieldsWithAttributeList(
              HashSet<string> playerAssemblyNames
            , Dictionary<Type, HashSet<string>> assemblyNameMap
            , TempTypeStore typeStore
            , List<TempSerializedAnnonatedMembers<FieldInfo>> result
        )
        {
            foreach (var (attribType, assemblyNames) in assemblyNameMap)
            {
                var filteredMembers = new List<FieldInfo>();

                foreach (var assemblyName in assemblyNames)
                {
                    filteredMembers.Clear();

                    var candidates = string.IsNullOrEmpty(assemblyName)
                        ? TypeCache.GetFieldsWithAttribute(attribType)
                        : TypeCache.GetFieldsWithAttribute(attribType, assemblyName);

                    foreach (var candidate in candidates)
                    {
                        var declType = candidate.DeclaringType;
                        var fieldType = candidate.FieldType;

                        if (IsEditor(declType, declType.Assembly.GetName().Name, playerAssemblyNames) == false
                            && IsEditor(fieldType, fieldType.Assembly.GetName().Name, playerAssemblyNames) == false
                        )
                        {
                            filteredMembers.Add(candidate);
                        }
                    }

                    if (filteredMembers.Count < 1)
                    {
                        continue;
                    }

                    var item = new TempSerializedAnnonatedMembers<FieldInfo> {
                        assemblyName = assemblyName,
                        attributeType = typeStore.Add(attribType),
                    };

                    var matches = item.matches;
                    matches.IncreaseCapacityTo(filteredMembers.Count);

                    foreach (var member in filteredMembers)
                    {
                        matches.Add(new MemberRef<FieldInfo>(member) {
                            declaringType = typeStore.Add(member.DeclaringType),
                        });
                    }

                    result.Add(item);
                }
            }
        }

        private static void PrepareMethodsWithAttributeList(
              HashSet<string> playerAssemblyNames
            , Dictionary<Type, HashSet<string>> assemblyNameMap
            , TempTypeStore typeStore
            , List<TempSerializedAnnonatedMembers<MethodInfo>> result
        )
        {
            foreach (var (attribType, assemblyNames) in assemblyNameMap)
            {
                var filteredMembers = new List<MethodInfo>();

                foreach (var assemblyName in assemblyNames)
                {
                    filteredMembers.Clear();

                    var candidates = string.IsNullOrEmpty(assemblyName)
                        ? TypeCache.GetMethodsWithAttribute(attribType)
                        : TypeCache.GetMethodsWithAttribute(attribType, assemblyName);

                    foreach (var candidate in candidates)
                    {
                        var declType = candidate.DeclaringType;
                        var returnType = candidate.ReturnType;

                        if (IsEditor(declType, declType.Assembly.GetName().Name, playerAssemblyNames) == false
                            && IsEditor(returnType, returnType.Assembly.GetName().Name, playerAssemblyNames) == false
                        )
                        {
                            filteredMembers.Add(candidate);
                        }
                    }

                    if (filteredMembers.Count < 1)
                    {
                        continue;
                    }

                    var item = new TempSerializedAnnonatedMembers<MethodInfo> {
                        assemblyName = assemblyName,
                        attributeType = typeStore.Add(attribType),
                    };

                    var matches = item.matches;
                    matches.IncreaseCapacityTo(filteredMembers.Count);

                    foreach (var member in filteredMembers)
                    {
                        matches.Add(new MemberRef<MethodInfo>(member) {
                            declaringType = typeStore.Add(member.DeclaringType),
                        });
                    }

                    result.Add(item);
                }
            }
        }

        private static void BuildTypeStore(SerializedTypeStore dest, TempTypeStore source)
        {
            var assemblyToTypesMap = source.assemblyToTypesMap;
            var assemblies = assemblyToTypesMap.Keys.ToList();
            assemblies.Sort(static (x, y) => string.CompareOrdinal(x.FullName, y.FullName));

            foreach (var assembly in assemblies)
            {
                var types = assemblyToTypesMap[assembly];
                var typeRefs = types.Values.ToList();
                typeRefs.Sort(static (x, y) => string.CompareOrdinal(x.Value.FullName, y.Value.FullName));

                foreach (var typeRef in typeRefs)
                {
                    typeRef.index = dest.Add(typeRef.Value);
                }
            }
        }

        private static void BuildTypesDerivedFromTypeList(
              List<SerializedDerivedTypes> dest
            , List<TempSerializedDerivedTypes> source
        )
        {
            foreach (var temp in source)
            {
                var tempDerivedTypes = temp.derivedTypes.AsSpan();
                var tempDerivedTypesLength = tempDerivedTypes.Length;

                var item = new SerializedDerivedTypes {
                    _assemblyName = temp.assemblyName,
                    _baseType = new(temp.baseType.index),
                    _derivedTypes = new(tempDerivedTypesLength),
                };

                var derivedTypes = item._derivedTypes;

                for (var i = 0; i < tempDerivedTypesLength; i++)
                {
                    derivedTypes.Add(new(tempDerivedTypes[i].index));
                }

                dest.Add(item);
            }
        }

        private static void BuildTypesWithAttributeList(
              List<SerializedAnnotatedMembers<SerializedType>> dest
            , List<TempSerializedAnnonatedMembers<Type>> source
        )
        {
            foreach (var temp in source)
            {
                var tempMatches = temp.matches.AsSpan();
                var tempMatchesLength = tempMatches.Length;

                var item = new SerializedAnnotatedMembers<SerializedType> {
                    _assemblyName = temp.assemblyName,
                    _attributeType = new(temp.attributeType.index),
                    _matches = new(tempMatchesLength),
                };

                var matches = item._matches;

                for (var i = 0; i < tempMatchesLength; i++)
                {
                    matches.Add(new(tempMatches[i].declaringType.index));
                }

                dest.Add(item);
            }
        }

        private static void BuildFieldsWithAttributeList(
              List<SerializedAnnotatedMembers<SerializedField>> dest
            , List<TempSerializedAnnonatedMembers<FieldInfo>> source
        )
        {
            foreach (var temp in source)
            {
                var tempMatches = temp.matches.AsSpan();
                var tempMatchesLength = tempMatches.Length;

                var item = new SerializedAnnotatedMembers<SerializedField> {
                    _assemblyName = temp.assemblyName,
                    _attributeType = new(temp.attributeType.index),
                    _matches = new(tempMatchesLength),
                };

                var matches = item._matches;

                for (var i = 0; i < tempMatchesLength; i++)
                {
                    var tempMatch = tempMatches[i];

                    matches.Add(new(tempMatch.Member, new(tempMatch.declaringType.index)));
                }

                dest.Add(item);
            }
        }

        private static void BuildMethodsWithAttributeList(
              List<SerializedAnnotatedMembers<SerializedMethod>> dest
            , List<TempSerializedAnnonatedMembers<MethodInfo>> source
        )
        {
            foreach (var temp in source)
            {
                var tempMatches = temp.matches.AsSpan();
                var tempMatchesLength = tempMatches.Length;

                var item = new SerializedAnnotatedMembers<SerializedMethod> {
                    _assemblyName = temp.assemblyName,
                    _attributeType = new(temp.attributeType.index),
                    _matches = new(tempMatchesLength),
                };

                var matches = item._matches;

                for (var i = 0; i < tempMatchesLength; i++)
                {
                    var tempMatch = tempMatches[i];

                    matches.Add(new(tempMatch.Member, new(tempMatch.declaringType.index)));
                }

                dest.Add(item);
            }
        }

        private record class IndexedRef<T>(T Value)
        {
            public int index;
        }

        private record class MemberRef<T>(T Member) where T : MemberInfo
        {
            public IndexedRef<Type> declaringType;
        }

        private class TempTypeStore
        {
            public Dictionary<SystemAssembly, Dictionary<Type, IndexedRef<Type>>> assemblyToTypesMap = new();

            public IndexedRef<Type> Add(Type type)
            {
                var assembly = type.Assembly;

                if (assemblyToTypesMap.TryGetValue(assembly, out var typeMap) == false)
                {
                    assemblyToTypesMap[assembly] = typeMap = new();
                }

                if (typeMap.TryGetValue(type, out var indexedType) == false)
                {
                    indexedType = new(type);
                    typeMap[type] = indexedType;
                }

                return indexedType;
            }
        }

        private class TempSerializedDerivedTypes
        {
            public string assemblyName;
            public IndexedRef<Type> baseType;
            public FasterList<IndexedRef<Type>> derivedTypes = new();
        }

        private class TempSerializedAnnonatedMembers<T>
            where T : MemberInfo
        {
            public string assemblyName;
            public IndexedRef<Type> attributeType;
            public FasterList<MemberRef<T>> matches = new();
        }
    }
}

#endif
