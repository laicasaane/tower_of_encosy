#if UNITY_MATHEMATICS
#if UNITASK

using System.Threading;
using Cysharp.Threading.Tasks;
using Unity.Collections;
using UnityEngine.Jobs;
using UnityEngine.SceneManagement;

namespace EncosyTower.Modules.Pooling
{
    partial class SceneObjectIdPooler<TKey>
    {
        public async UniTask InitializeAsync(Scene scene, CancellationToken token = default)
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

        protected virtual UniTask OnInitializeAsync() => UniTask.CompletedTask;
    }
}

#endif
#endif
