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

    partial class SceneObjectIdPooler<TKey>
    {
        public async UnityTask InitializeAsync(Scene scene, CancellationToken token = default)
        {
            DisposeNativeCollections();

            var prefabOpt = await GetKey().TryLoadAsync(token);

            if (prefabOpt.TryValue(out var prefab) == false)
            {
                return;
            }

            _pool = new SceneObjectIdPool() {
                Scene = scene,
                Source = prefab,
                TrimCloneSuffix = TrimCloneSuffix,
            };

            var capacity = EstimateCapacity() + 1;

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
