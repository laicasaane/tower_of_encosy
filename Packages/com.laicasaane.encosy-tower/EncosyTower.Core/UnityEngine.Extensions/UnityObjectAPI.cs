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
        public static void ConvertEntityIdsToObjectList(
              ReadOnlySpan<EntityId> entityIds
            , [NotNull] List<UnityObject> objectList
        )
        {
            var idArray = NativeArray.CreateFrom(entityIds, Allocator.Temp);
            ConvertEntityIdsToObjectList(idArray, objectList);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ConvertGameObjectIdsToObjectList(
              ReadOnlySpan<GameObjectId> gameObjectIds
            , [NotNull] List<UnityObject> objectList
        )
        {
            var idArray = NativeArray.CreateFrom(gameObjectIds, Allocator.Temp);
            ConvertGameObjectIdsToObjectList(idArray, objectList);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ConvertTransformIdsToObjectList(
              ReadOnlySpan<TransformId> transformIds
            , [NotNull] List<UnityObject> objectList
        )
        {
            var idArray = NativeArray.CreateFrom(transformIds, Allocator.Temp);
            ConvertTransformIdsToObjectList(idArray, objectList);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ConvertGameObjectIdsToGameObjects(
              ReadOnlySpan<GameObjectId> gameObjectIds
            , Span<GameObject> gameObjects
            , [NotNull] List<UnityObject> temporaryList
        )
        {
            var idArray = NativeArray.CreateFrom(gameObjectIds, Allocator.Temp);
            ConvertGameObjectIdsToGameObjects(idArray, gameObjects, temporaryList);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ConvertTransformIdsToTransforms(
              ReadOnlySpan<TransformId> transformIds
            , Span<Transform> transforms
            , [NotNull] List<UnityObject> temporaryList
        )
        {
            var idArray = NativeArray.CreateFrom(transformIds, Allocator.Temp);
            ConvertTransformIdsToTransforms(idArray, transforms, temporaryList);
        }

        public static void ConvertEntityIdsToObjectList(
              NativeArray<EntityId> entityIds
            , [NotNull] List<UnityObject> objectList
        )
        {
            if (entityIds.Length < 1)
            {
                return;
            }

            ConvertEntityIdsToObjectListInternal(entityIds, objectList);
        }

        public static void ConvertGameObjectIdsToObjectList(
              NativeArray<GameObjectId> gameObjectIds
            , [NotNull] List<UnityObject> objectList
        )
        {
            if (gameObjectIds.Length < 1)
            {
                return;
            }

            ConvertEntityIdsToObjectListInternal(gameObjectIds.Reinterpret<EntityId>(), objectList);
        }

        public static void ConvertTransformIdsToObjectList(
              NativeArray<TransformId> transformIds
            , [NotNull] List<UnityObject> objectList
        )
        {
            if (transformIds.Length < 1)
            {
                return;
            }

            ConvertEntityIdsToObjectListInternal(transformIds.Reinterpret<EntityId>(), objectList);
        }

        public static void ConvertGameObjectIdsToGameObjects(
              NativeArray<GameObjectId> gameObjectIds
            , Span<GameObject> gameObjects
            , [NotNull] List<UnityObject> temporaryList
        )
        {
            var length = gameObjects.Length;

            Checks.IsTrue(length == gameObjects.Length, "'gameObjectIds' and 'gameObjects' do not have the same size");

            if (length < 1)
            {
                return;
            }

            ConvertEntityIdsToObjectListInternal(gameObjectIds.Reinterpret<EntityId>(), temporaryList);

            var objects = temporaryList.AsReadOnlySpan();

            for (var i = 0; i < length; i++)
            {
                gameObjects[i] = (objects[i] as GameObject).AssumeValid();
            }

            temporaryList.Clear();
        }

        public static void ConvertTransformIdsToTransforms(
              NativeArray<TransformId> transformIds
            , Span<Transform> transforms
            , [NotNull] List<UnityObject> temporaryList
        )
        {
            var length = transforms.Length;

            Checks.IsTrue(length == transforms.Length, "'transformIds' and 'transforms' do not have the same size");

            if (length < 1)
            {
                return;
            }

            ConvertEntityIdsToObjectListInternal(transformIds.Reinterpret<EntityId>(), temporaryList);

            var objects = temporaryList.AsReadOnlySpan();

            for (var i = 0; i < length; i++)
            {
                transforms[i] = (objects[i] as Transform).AssumeValid();
            }

            temporaryList.Clear();
        }

        private static void ConvertEntityIdsToObjectListInternal(
              NativeArray<EntityId> entityIds
            , List<UnityObject> objectList
        )
        {
            objectList.Clear();
            objectList.IncreaseCapacityTo(entityIds.Length);

#if UNITY_6000_3_OR_NEWER
            Resources.EntityIdsToObjectList(entityIds, objectList);
#else
            Resources.InstanceIDToObjectList(entityIds, objectList);
#endif
        }
    }
}
