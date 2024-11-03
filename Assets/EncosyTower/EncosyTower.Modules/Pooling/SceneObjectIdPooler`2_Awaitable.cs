#if UNITY_MATHEMATICS
#if !UNITASK && UNITY_6000_0_OR_NEWER

using System.Threading;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Jobs;
using UnityEngine.SceneManagement;

namespace EncosyTower.Modules.Pooling
{
    partial class SceneObjectIdPooler<TKey, TId>
    {
        private AwaitableCompletionSource _completionSource;

        protected AwaitableCompletionSource CompletionSource
        {
            get
            {
                if (_completionSource == null)
                {
                    _completionSource = new();
                    _completionSource.SetResult();
                }

                return _completionSource;
            }
        }

        public async Awaitable InitializeAsync(Scene scene, CancellationToken token = default)
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

        protected virtual Awaitable OnInitializeAsync() => CompletionSource.Awaitable;
    }
}

#endif
#endif
