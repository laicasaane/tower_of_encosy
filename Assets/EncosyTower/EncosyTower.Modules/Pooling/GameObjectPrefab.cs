using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using EncosyTower.Modules.Collections;
using EncosyTower.Modules.Collections.Unsafe;
using Unity.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace EncosyTower.Modules.Pooling
{
    public sealed class GameObjectPrefab
    {
        public GameObject Source { get; init; }

        public Transform Parent { get; init; }

        public bool InstantiateInWorldSpace { get; init; }

        public Scene Scene { get; init; }

        public Scene PoolScene { get; init; }

        private bool Validate(Location location)
        {
            return location switch {
                Location.Parent => Parent.IsValid(),
                Location.PoolScene => PoolScene.IsValid(),
                Location.Scene => Scene.IsValid(),
                _ => false,
            };
        }

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

        private void Instantiate(
              GameObject source
            , int count
            , NativeArray<int> instanceIds
            , NativeArray<int> transformIds
            , Location location
        )
        {
            switch (location)
            {
                case Location.PoolScene:
                {
                    GameObject.InstantiateGameObjects(
                          source.GetInstanceID()
                        , count
                        , instanceIds
                        , transformIds
                        , PoolScene
                    );

                    return;
                }

                case Location.Scene:
                {
                    GameObject.InstantiateGameObjects(
                          source.GetInstanceID()
                        , count
                        , instanceIds
                        , transformIds
                        , Scene
                    );

                    return;
                }

                default:
                {
                    GameObject.InstantiateGameObjects(
                          source.GetInstanceID()
                        , count
                        , instanceIds
                        , transformIds
                    );

                    var parent = this.Parent;

                    if (parent.IsValid())
                    {
                        var list = new List<UnityEngine.Object>(transformIds.Length);

                        Resources.InstanceIDToObjectList(transformIds, list);

                        var span = list.AsReadOnlySpanUnsafe();
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

        public bool Instantiate(
              int count
            , Allocator allocator
            , out NativeArray<int> instanceIds
            , out NativeArray<int> transformIds
            , Location tryLocation1
            , Location tryLocation2
            , Location tryLocation3
        )
        {
            if (count <= 0)
            {
                instanceIds = default;
                transformIds = default;
                return false;
            }

            var source = Source;

            if (source.IsInvalid())
            {
                throw new NullReferenceException(nameof(Source));
            }

            instanceIds = NativeArray.CreateFast<int>(count, allocator);
            transformIds = NativeArray.CreateFast<int>(count, allocator);

            if (Validate(tryLocation1))
            {
                Instantiate(source, count, instanceIds, transformIds, tryLocation1);
                return true;
            }

            if (Validate(tryLocation2))
            {
                Instantiate(source, count, instanceIds, transformIds, tryLocation2);
                return true;
            }

            if (Validate(tryLocation3))
            {
                Instantiate(source, count, instanceIds, transformIds, tryLocation3);
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
        public void MoveToScene(Span<int> instanceIdSpan, bool moveToPoolScene = false)
        {
            var scene = moveToPoolScene ? this.PoolScene : this.Scene;
            var length = instanceIdSpan.Length;

            if (scene.IsValid() && length > 0)
            {
                var instanceIds = NativeArray.CreateFast<int>(length, Allocator.Temp);
                instanceIdSpan.CopyTo(instanceIds);

                SceneManager.MoveGameObjectsToScene(instanceIds, scene);
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
