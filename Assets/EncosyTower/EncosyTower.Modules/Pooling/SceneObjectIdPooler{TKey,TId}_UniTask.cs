#if UNITY_MATHEMATICS
#if UNITASK

using System.Threading;
using Cysharp.Threading.Tasks;
using Unity.Collections;
using UnityEngine.Jobs;
using UnityEngine.SceneManagement;

namespace EncosyTower.Modules.Pooling
{
    partial class SceneObjectIdPooler<TKey, TId>
    {
        public async UniTask InitializeAsync(Scene scene, CancellationToken token = default)
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

                if (prefabOpt.TryValue(out var prefab) == false)
                {
                    continue;
                }

                poolMap[id] = new(scene, prefab) {
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

        protected virtual UniTask OnInitializeAsync() => UniTask.CompletedTask;
    }
}

#endif
#endif
