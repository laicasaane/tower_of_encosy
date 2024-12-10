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
using EncosyTower.Modules.Types.Internals;
using UnityEditor;
using UnityEditor.Compilation;

namespace EncosyTower.Modules.Types.Editor
{
    internal static class SerializedTypeCacheEditor
    {
        public static void Regenerate(this SerializedTypeCache cache)
        {
            var typesDerivedFromTypeList = cache._typesDerivedFromTypeList;
            var typesWithAttributeList = cache._typesWithAttributeList;
            var fieldsWithAttributeList = cache._fieldsWithAttributeList;
            var methodsWithAttributeList = cache._methodsWithAttributeList;
            var typeStore = cache._typeStore;
            var assemblyQualifiedNames = typeStore._assemblyQualifiedNames;

            typesDerivedFromTypeList.Clear();
            typesWithAttributeList.Clear();
            fieldsWithAttributeList.Clear();
            methodsWithAttributeList.Clear();
            assemblyQualifiedNames.Clear();

            //var targetTypesAll = TypeCache.GetTypesWithAttribute<TypeCacheTargetAttribute>().ToArray();
            //var targetTypesAttributes = targetTypesAll.Where(typeof(Attribute).IsAssignableFrom).ToArray();
            //var typeCacheSource = new EditorTypeCacheSource();
            //CacheAllAttributeUsages(targetTypesAttributes, typeCacheSource);

            //var targetTypesNonAttributes = targetTypesAll.Except(targetTypesAttributes).ToArray();
            //CacheAllTypeInheritances(targetTypesNonAttributes, typeCacheSource);
        }

        private static bool HasFlag(AttributeTargets flag, AttributeTargets value)
        {
            return (flag & value) != 0;
        }

        private static bool IsInEditorAssembly(Type type, HashSet<string> playerAssemblies)
        {
            playerAssemblies ??= CompilationPipeline.GetAssemblies(AssembliesType.PlayerWithoutTestAssemblies)
                .Select(a => a.name)
                .Concat(CompilationPipeline
                    .GetPrecompiledAssemblyPaths(CompilationPipeline.PrecompiledAssemblySources.UnityEngine)
                    .Select(Path.GetFileNameWithoutExtension)
                    .Where(n => n.StartsWith("UnityEditor") == false)
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
                .ToHashSet();

            return playerAssemblies.Contains(type.Assembly.GetName().Name) == false
                || type.FullName.StartsWith("UnityEditorInternal");
        }

        //private void CacheAllAttributeUsages(IEnumerable<Type> targetTypes, ITypeCacheSource typeCacheSource)
        //{
        //    /*
        //     * Only cache attributes that are decorated with the TypeCachedAttribute attribute, OR if:
        //     * 1. StrictMode is disabled.
        //     * 2. They aren't in editor assemblies.
        //     * AND
        //     * 3. They pass the predicate filter.
        //     */
        //    foreach (var type in targetTypes) CacheAttributeUsage(type, typeCacheSource);

        //    if (TypeCacheBuilderUtility.StrictMode) return;

        //    var attributeTypes = TypeCache.GetTypesDerivedFrom<Attribute>();

        //    foreach (var type in attributeTypes)
        //    {
        //        if (IsInEditorAssembly(type)) continue;

        //        var passesPredicate = TypeCacheBuilderUtility.ShouldCacheAttributeTypePredicate(type);
        //        if (!passesPredicate) continue;

        //        CacheAttributeUsage(type, typeCacheSource);
        //    }
        //}

        //private void CacheAttributeUsage(Type attributeType, ITypeCacheSource typeCacheSource)
        //{
        //    var attributeUsage = attributeType.GetCustomAttribute<AttributeUsageAttribute>();
        //    var serializedAttributeType = new SerializedType().Build(attributeType, TypeStore);

        //    // Check for Types with attribute
        //    if (HasFlag(attributeUsage.ValidOn, AttributeTargets.Class) ||
        //        HasFlag(attributeUsage.ValidOn, AttributeTargets.Struct))
        //    {
        //        var matches = typeCacheSource.GetTypesWithAttribute(attributeType).Where(t => !IsInEditorAssembly(t));
        //        if (matches.Any())
        //        {
        //            var matchesSerialized = matches
        //                .Select(memberInfo => new SerializedType().Build(memberInfo, TypeStore))
        //                .ToList();
        //            TypesWithAttribute.Add(
        //                new MemberInfoWithAttribute<SerializedType> {
        //                    AttributeType = serializedAttributeType,
        //                    Matches = matchesSerialized
        //                });
        //        }
        //    }

        //    // Check for Methods with attribute
        //    if (HasFlag(attributeUsage.ValidOn, AttributeTargets.Method))
        //    {
        //        var matches = typeCacheSource.GetMethodsWithAttribute(attributeType)
        //            .Where(memberInfo => !IsInEditorAssembly(memberInfo.DeclaringType));
        //        if (matches.Any())
        //        {
        //            var matchesSerialized = matches
        //                .Select(memberInfo => new SerializedMethod().Build(memberInfo, TypeStore))
        //                .ToList();
        //            MethodsWithAttribute.Add(
        //                new MemberInfoWithAttribute<SerializedMethod> {
        //                    AttributeType = serializedAttributeType,
        //                    Matches = matchesSerialized
        //                });
        //        }
        //    }

        //    // Check for Fields with attribute
        //    if (HasFlag(attributeUsage.ValidOn, AttributeTargets.Property))
        //    {
        //        var matches = typeCacheSource.GetPropertiesWithAttribute(attributeType)
        //            .Where(memberInfo => !IsInEditorAssembly(memberInfo.DeclaringType));
        //        if (matches.Any())
        //        {
        //            var matchesSerialized = matches
        //                .Select(memberInfo => new SerializedProperty().Build(memberInfo, TypeStore))
        //                .ToList();
        //            PropertiesWithAttribute.Add(
        //                new MemberInfoWithAttribute<SerializedProperty> {
        //                    AttributeType = serializedAttributeType,
        //                    Matches = matchesSerialized
        //                });
        //        }
        //    }

        //    // Check for Fields with attribute
        //    if (HasFlag(attributeUsage.ValidOn, AttributeTargets.Field))
        //    {
        //        var matches = typeCacheSource.GetFieldsWithAttribute(attributeType)
        //            .Where(memberInfo => !IsInEditorAssembly(memberInfo.DeclaringType));
        //        if (matches.Any())
        //        {
        //            var matchesSerialized = matches
        //                .Select(memberInfo => new SerializedField().Build(memberInfo, TypeStore))
        //                .ToList();
        //            FieldsWithAttribute.Add(
        //                new MemberInfoWithAttribute<SerializedField> {
        //                    AttributeType = serializedAttributeType,
        //                    Matches = matchesSerialized
        //                });
        //        }
        //    }
        //}

        //private void CacheAllTypeInheritances(IEnumerable<Type> targetTypes, ITypeCacheSource typeCacheSource)
        //{
        //    /*
        //     * Only cache types that are decorated with the TypeCachedType attribute, OR if:
        //     * 1. StrictMode is disabled.
        //     * 2. They aren't in editor assemblies.
        //     * AND
        //     * 3. They pass the predicate filter.
        //     */
        //    foreach (var typeCachedType in targetTypes) CacheTypeInheritance(typeCachedType, typeCacheSource);

        //    if (TypeCacheBuilderUtility.StrictMode) return;

        //    var allTypes = typeCacheSource.GetTypesDerivedFrom<object>();
        //    foreach (var type in allTypes)
        //    {
        //        if (IsInEditorAssembly(type)) continue;

        //        var passesPredicate = TypeCacheBuilderUtility.ShouldCacheTypeInheritance(type);
        //        if (!passesPredicate) continue;

        //        CacheTypeInheritance(type, typeCacheSource);
        //    }
        //}

        //private void CacheTypeInheritance(Type type, ITypeCacheSource typeCacheSource)
        //{
        //    var derived = typeCacheSource.GetTypesDerivedFrom(type).Where(t => !IsInEditorAssembly(t));
        //    if (!derived.Any()) return;

        //    var derivedSerialized = derived.Select(t => new SerializedType().Build(t, TypeStore)).ToList();
        //    var parentType = new SerializedType();
        //    parentType.Serialize(type, TypeStore);
        //    TypesDerivedFromType.Add(
        //        new TypesDerivedFromType {
        //            ParentType = parentType,
        //            DerivedTypes = derivedSerialized
        //        });
        //}
    }
}

#endif
