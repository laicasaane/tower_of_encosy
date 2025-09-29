using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using EncosyTower.Collections;
using EncosyTower.Collections.Extensions;
using EncosyTower.UnityExtensions;
using Unity.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace EncosyTower.Pooling
{
    public sealed partial class GameObjectPrefab
    {
        public GameObject Source { get; init; }

        public Transform Parent { get; init; }

        public bool InstantiateInWorldSpace { get; init; }

        public Scene Scene { get; init; }

        public Scene PoolScene { get; init; }

        public GameObject Instantiate(Location location)
        {
            var source = Source;

            if (source.IsInvalid())
            {
                throw new NullReferenceException(nameof(Source));
            }

            switch (location)
            {
                case Location.PoolScene:
                {
                    var result = UnityEngine.Object.Instantiate(source);
                    SceneManager.MoveGameObjectToScene(result, PoolScene);
                    return result;
                }

                case Location.Scene:
                {
                    var result = UnityEngine.Object.Instantiate(source);
                    SceneManager.MoveGameObjectToScene(result, Scene);
                    return result;
                }

                default:
                {
                    return UnityEngine.Object.Instantiate(source, Parent, InstantiateInWorldSpace);
                }
            }
        }

        public GameObject Instantiate(Location tryLocation1, Location tryLocation2, Location tryLocation3)
        {
            if (Validate(tryLocation1))
            {
                return Instantiate(tryLocation1);
            }

            if (Validate(tryLocation2))
            {
                return Instantiate(tryLocation2);
            }

            if (Validate(tryLocation3))
            {
                return Instantiate(tryLocation3);
            }

            return Instantiate(Location.Parent);
        }


#if UNITY_6000_2_OR_NEWER
        public bool Instantiate(
              int count
            , Allocator allocator
            , out NativeArray<EntityId> gameObjectIds
            , out NativeArray<EntityId> transformIds
            , Location tryLocation1
            , Location tryLocation2
            , Location tryLocation3
        )
        {
            if (count <= 0)
            {
                gameObjectIds = default;
                transformIds = default;
                return false;
            }

            var source = Source;

            if (source.IsInvalid())
            {
                throw new NullReferenceException(nameof(Source));
            }

            gameObjectIds = NativeArray.CreateFast<EntityId>(count, allocator);
            transformIds = NativeArray.CreateFast<EntityId>(count, allocator);

            var reGameObjectIds = gameObjectIds.Reinterpret<int>();
            var reTransformIds = transformIds.Reinterpret<int>();

            if (Validate(tryLocation1))
            {
                Instantiate(source, count, reGameObjectIds, reTransformIds, tryLocation1);
                return true;
            }

            if (Validate(tryLocation2))
            {
                Instantiate(source, count, reGameObjectIds, reTransformIds, tryLocation2);
                return true;
            }

            if (Validate(tryLocation3))
            {
                Instantiate(source, count, reGameObjectIds, reTransformIds, tryLocation3);
                return true;
            }

            return false;
        }
#endif

        public bool Instantiate(
              int count
            , Allocator allocator
            , out NativeArray<int> gameObjectIds
            , out NativeArray<int> transformIds
            , Location tryLocation1
            , Location tryLocation2
            , Location tryLocation3
        )
        {
            if (count <= 0)
            {
                gameObjectIds = default;
                transformIds = default;
                return false;
            }

            var source = Source;

            if (source.IsInvalid())
            {
                throw new NullReferenceException(nameof(Source));
            }

            gameObjectIds = NativeArray.CreateFast<int>(count, allocator);
            transformIds = NativeArray.CreateFast<int>(count, allocator);

            if (Validate(tryLocation1))
            {
                Instantiate(source, count, gameObjectIds, transformIds, tryLocation1);
                return true;
            }

            if (Validate(tryLocation2))
            {
                Instantiate(source, count, gameObjectIds, transformIds, tryLocation2);
                return true;
            }

            if (Validate(tryLocation3))
            {
                Instantiate(source, count, gameObjectIds, transformIds, tryLocation3);
                return true;
            }

            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void MoveToScene([NotNull] GameObject go, bool moveToPoolScene = false)
        {
            if (go.IsInvalid())
            {
                return;
            }

            var scene = moveToPoolScene ? this.PoolScene : this.Scene;

            if (scene.IsValid() && go.scene != scene)
            {
                SceneManager.MoveGameObjectToScene(go, scene);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void MoveToScene(Span<int> gameObjectIds, bool moveToPoolScene = false)
        {
            var scene = moveToPoolScene ? this.PoolScene : this.Scene;
            var length = gameObjectIds.Length;

            if (scene.IsValid() && length > 0)
            {
                var instanceIds = NativeArray.CreateFast<int>(length, Allocator.Temp);
                gameObjectIds.CopyTo(instanceIds);

                SceneManager.MoveGameObjectsToScene(instanceIds, scene);
            }
        }

        private bool Validate(Location location)
        {
            return location switch {
                Location.Parent => Parent.IsValid(),
                Location.PoolScene => PoolScene.IsValid(),
                Location.Scene => Scene.IsValid(),
                _ => false,
            };
        }

        private void Instantiate(
              GameObject source
            , int count
            , NativeArray<int> gameObjectIds
            , NativeArray<int> transformIds
            , Location location
        )
        {
            switch (location)
            {
                case Location.PoolScene:
                {
                    GameObject.InstantiateGameObjects(
#if UNITY_6000_2_OR_NEWER
                          source.GetEntityId()
#else
                          source.GetInstanceID()
#endif
                        , count
                        , gameObjectIds
                        , transformIds
                        , PoolScene
                    );

                    return;
                }

                case Location.Scene:
                {
                    GameObject.InstantiateGameObjects(
#if UNITY_6000_2_OR_NEWER
                          source.GetEntityId()
#else
                          source.GetInstanceID()
#endif
                        , count
                        , gameObjectIds
                        , transformIds
                        , Scene
                    );

                    return;
                }

                default:
                {
                    GameObject.InstantiateGameObjects(
#if UNITY_6000_2_OR_NEWER
                          source.GetEntityId()
#else
                          source.GetInstanceID()
#endif
                        , count
                        , gameObjectIds
                        , transformIds
                    );

                    var parent = this.Parent;

                    if (parent.IsValid())
                    {
                        var list = new List<UnityEngine.Object>(transformIds.Length);

                        Resources.InstanceIDToObjectList(transformIds, list);

                        var span = list.AsReadOnlySpan();
                        var spanLength = span.Length;
                        var inWorldSpace = InstantiateInWorldSpace;

                        for (var i = 0; i < spanLength; i++)
                        {
                            var transform = span[i] as Transform;

                            if (transform.IsInvalid())
                            {
                                continue;
                            }

                            transform.SetParent(parent, inWorldSpace);
                        }
                    }

                    return;
                }
            }
        }

        public enum Location : byte
        {
            Parent = 0,
            PoolScene,
            Scene,
        }
    }
}
