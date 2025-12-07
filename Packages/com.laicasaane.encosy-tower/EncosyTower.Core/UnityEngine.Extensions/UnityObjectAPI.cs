using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using EncosyTower.Collections;
using EncosyTower.Collections.Extensions;
using EncosyTower.Debugging;
using Unity.Collections;
using UnityEngine;

namespace EncosyTower.UnityExtensions
{
    using UnityObject = UnityEngine.Object;

#if UNITY_6000_2_OR_NEWER
    using GameObjectId = UnityEntityId<GameObject>;
    using TransformId = UnityEntityId<Transform>;
#else
    using GameObjectId = UnityInstanceId<GameObject>;
    using TransformId = UnityInstanceId<Transform>;
#endif

#if UNITY_6000_3_OR_NEWER
    using EntityId = UnityEngine.EntityId;
#else
    using EntityId = System.Int32;
#endif

    public static class UnityObjectAPI
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ConvertIdsToObjects(
              ReadOnlySpan<GameObjectId> gameObjectIds
            , Span<GameObject> gameObjects
            , [NotNull] List<UnityObject> temporaryList
        )
        {
            var idArray = NativeArray.CreateFrom(gameObjectIds, Allocator.Temp);
            ConvertIdsToObjects(idArray, gameObjects, temporaryList);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ConvertIdsToObjects(
              ReadOnlySpan<TransformId> transformIds
            , Span<Transform> transforms
            , [NotNull] List<UnityObject> temporaryList
        )
        {
            var idArray = NativeArray.CreateFrom(transformIds, Allocator.Temp);
            ConvertIdsToObjects(idArray, transforms, temporaryList);
        }

        public static void ConvertIdsToObjects(
              NativeArray<GameObjectId> gameObjectIds
            , Span<GameObject> gameObjects
            , [NotNull] List<UnityObject> temporaryList
        )
        {
            var length = gameObjects.Length;

            Checks.IsTrue(length == gameObjects.Length, "arrays do not have the same size");
            Checks.IsTrue(length > 0, "arrays do not have enough space to contain the items");

            temporaryList.Clear();
            temporaryList.IncreaseCapacityTo(length);

#if UNITY_6000_3_OR_NEWER
            Resources.EntityIdsToObjectList(gameObjectIds.Reinterpret<EntityId>(), temporaryList);
#else
            Resources.InstanceIDToObjectList(gameObjectIds.Reinterpret<EntityId>(), temporaryList);
#endif

            var objects = temporaryList.AsReadOnlySpan();

            for (var i = 0; i < length; i++)
            {
                gameObjects[i] = (objects[i] as GameObject).AssumeValid();
            }

            temporaryList.Clear();
        }

        public static void ConvertIdsToObjects(
              NativeArray<TransformId> transformIds
            , Span<Transform> transforms
            , [NotNull] List<UnityObject> temporaryList
        )
        {
            var length = transforms.Length;

            Checks.IsTrue(length == transforms.Length, "arrays do not have the same size");
            Checks.IsTrue(length > 0, "arrays do not have enough space to contain the items");

            temporaryList.Clear();
            temporaryList.IncreaseCapacityTo(length);

#if UNITY_6000_3_OR_NEWER
            Resources.EntityIdsToObjectList(transformIds.Reinterpret<EntityId>(), temporaryList);
#else
            Resources.InstanceIDToObjectList(transformIds.Reinterpret<EntityId>(), temporaryList);
#endif

            var objects = temporaryList.AsReadOnlySpan();

            for (var i = 0; i < length; i++)
            {
                transforms[i] = (objects[i] as Transform).AssumeValid();
            }

            temporaryList.Clear();
        }
    }
}
