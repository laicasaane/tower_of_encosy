#if UNITY_MATHEMATICS
#if UNITASK || UNITY_6000_0_OR_NEWER

using System.Threading;
using EncosyTower.Tasks;
using Unity.Collections;
using UnityEngine.Jobs;
using UnityEngine.SceneManagement;

namespace EncosyTower.Pooling
{
#if UNITASK
    using UnityTask = Cysharp.Threading.Tasks.UniTask;
#else
    using UnityTask = UnityEngine.Awaitable;
#endif

    partial class SceneObjectIdManager<TKey, TId>
    {
        public async UnityTask InitializeAsync(Scene scene, CancellationToken token = default)
        {
            DisposeNativeCollections();

            var entries = GetEntries();
            var poolMap = _poolMap;
            var trimCloneSuffix = TrimCloneSuffix;

            foreach (var (id, key) in entries)
            {
                if (poolMap.ContainsKey(id))
                {
                    continue;
                }

                var prefabOpt = await key.TryLoadAsync(token);

                if (prefabOpt.TryGetValue(out var prefab) == false)
                {
                    continue;
                }

                poolMap[id] = new SceneObjectIdPool() {
                    Scene = scene,
                    Source = prefab,
                    TrimCloneSuffix = trimCloneSuffix,
                };
            }

            var capacity = EstimateCapacity(poolMap.Count) + 1;

            TransformAccessArray.Allocate(capacity, -1, out _transformArray);
            _goInfoMap = new(capacity, Allocator.Persistent);
            _positions = new(capacity, Allocator.Persistent);

            CreateDefaultGameObject(scene);

            await OnInitializeAsync();
        }

        protected virtual UnityTask OnInitializeAsync() => UnityTasks.GetCompleted();
    }
}

#endif
#endif
