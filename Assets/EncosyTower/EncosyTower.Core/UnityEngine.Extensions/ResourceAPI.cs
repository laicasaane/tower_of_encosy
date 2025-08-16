using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using EncosyTower.Collections;
using Unity.Collections;
using UnityEngine;

namespace EncosyTower.UnityExtensions
{
    /// <summary>
    /// Extension APIs in accompanying with <see cref="UnityEngine.Resources"/>.
    /// </summary>
    public static class ResourceAPI
    {
        public static void InstanceIDToObjectList(
              ReadOnlySpan<int> instanceIds
            , [NotNull] List<UnityEngine.Object> objectList
        )
        {
            objectList.Clear();

            if (instanceIds.IsEmpty)
            {
                return;
            }

            objectList.Capacity = Mathf.Max(objectList.Capacity, instanceIds.Length);

            var nativeArray = NativeArray.CreateFrom(instanceIds, Allocator.Temp);

            Resources.InstanceIDToObjectList(nativeArray, objectList);
        }
    }
}
