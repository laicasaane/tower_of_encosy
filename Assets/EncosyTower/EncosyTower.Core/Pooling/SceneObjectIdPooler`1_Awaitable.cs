#if UNITY_MATHEMATICS
#if !UNITASK && UNITY_6000_0_OR_NEWER

using System.Threading;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Jobs;
using UnityEngine.SceneManagement;

namespace EncosyTower.Pooling
{
    partial class SceneObjectIdPooler<TKey>
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

            var prefabOpt = await GetKey().TryLoadAsync(token);

            if (prefabOpt.TryValue(out var prefab) == false)
            {
                return;
            }

            _pool = new(scene, prefab) {
                TrimCloneSuffix = TrimCloneSuffix,
            };

            var capacity = EstimateCapacity() + 1;

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
