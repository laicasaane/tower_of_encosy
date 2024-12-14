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

            GetTypesFromThis<CacheTypesDerivedFromThisTypeAttribute>(
                playerAssemblyNames, typeStore, baseTypeToAssemblyMap
            );

            GetTypesFrom<CacheTypesDerivedFromAttribute>(
                playerAssemblyNames, typeStore, baseTypeToAssemblyMap
            );

            GetTypesFromThis<CacheTypesWithThisAttributeAttribute>(
                playerAssemblyNames, typeStore, typeAttribToAssemblyMap
            );

            GetTypesFrom<CacheTypesWithAttributeAttribute>(
                playerAssemblyNames, typeStore, typeAttribToAssemblyMap
            );

            GetTypesFromThis<CacheFieldsWithThisAttributeAttribute>(
                playerAssemblyNames, typeStore, fieldAttribToAssemblyMap
            );

            GetTypesFrom<CacheFieldsWithAttributeAttribute>(
                playerAssemblyNames, typeStore, fieldAttribToAssemblyMap
            );

            GetTypesFromThis<CacheMethodsWithThisAttributeAttribute>(
                playerAssemblyNames, typeStore, methodAttribToAssemblyMap
            );

            GetTypesFrom<CacheMethodsWithAttributeAttribute>(
                playerAssemblyNames, typeStore, methodAttribToAssemblyMap
            );

            BuildTypesDerivedFromTypeList(
                playerAssemblyNames, baseTypeToAssemblyMap, typeStore, typesDerivedFromTypeList
            );

            BuildTypesWithAttributeList(
                playerAssemblyNames, typeAttribToAssemblyMap, typeStore, typesWithAttributeList
            );

            BuildFieldsWithAttributeList(
                playerAssemblyNames, fieldAttribToAssemblyMap, typeStore, fieldsWithAttributeList
            );

            BuildMethodsWithAttributeList(
                playerAssemblyNames, methodAttribToAssemblyMap, typeStore, methodsWithAttributeList
            );
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

        private static void GetTypesFromThis<TAttribute>(
              HashSet<string> playerAssemblyNames
            , SerializedTypeStore typeStore
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

        private static void GetTypesFrom<TAttribute>(
              HashSet<string> playerAssemblyNames
            , SerializedTypeStore typeStore
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

        private static void BuildTypesDerivedFromTypeList(
              HashSet<string> playerAssemblyNames
            , Dictionary<Type, HashSet<string>> assemblyNameMap
            , SerializedTypeStore typeStore
            , List<SerializedDerivedTypes> result
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

                    var item = new SerializedDerivedTypes {
                        _assemblyName = assemblyName,
                        _baseType = new SerializedType(baseType, typeStore),
                    };

                    var derivedTypes = item._derivedTypes;

                    foreach (var type in filteredTypes)
                    {
                        derivedTypes.Add(new SerializedType(type, typeStore));
                    }

                    result.Add(item);
                }
            }
        }

        private static void BuildTypesWithAttributeList(
              HashSet<string> playerAssemblyNames
            , Dictionary<Type, HashSet<string>> assemblyNameMap
            , SerializedTypeStore typeStore
            , List<SerializedDecoratedMembers<SerializedType>> result
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

                    var item = new SerializedDecoratedMembers<SerializedType> {
                        _assemblyName = assemblyName,
                        _attributeType = new SerializedType(attribType, typeStore),
                    };

                    var matches = item._matches;

                    foreach (var type in filteredTypes)
                    {
                        matches.Add(new SerializedType(type, typeStore));
                    }

                    result.Add(item);
                }
            }
        }

        private static void BuildFieldsWithAttributeList(
              HashSet<string> playerAssemblyNames
            , Dictionary<Type, HashSet<string>> assemblyNameMap
            , SerializedTypeStore typeStore
            , List<SerializedDecoratedMembers<SerializedField>> result
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

                    var item = new SerializedDecoratedMembers<SerializedField> {
                        _assemblyName = assemblyName,
                        _attributeType = new SerializedType(attribType, typeStore),
                    };

                    var matches = item._matches;

                    foreach (var member in filteredMembers)
                    {
                        matches.Add(new SerializedField(member, typeStore));
                    }

                    result.Add(item);
                }
            }
        }

        private static void BuildMethodsWithAttributeList(
              HashSet<string> playerAssemblyNames
            , Dictionary<Type, HashSet<string>> assemblyNameMap
            , SerializedTypeStore typeStore
            , List<SerializedDecoratedMembers<SerializedMethod>> result
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

                    var item = new SerializedDecoratedMembers<SerializedMethod> {
                        _assemblyName = assemblyName,
                        _attributeType = new SerializedType(attribType, typeStore),
                    };

                    var matches = item._matches;

                    foreach (var member in filteredMembers)
                    {
                        matches.Add(new SerializedMethod(member, typeStore));
                    }

                    result.Add(item);
                }
            }
        }
    }
}

#endif
