namespace EncosyTower.Samples.Pooling
{
    using UnityEngine;

    public partial class NativeGameObjectPooler : MonoBehaviour
    {
        [SerializeField] private GameObject _template;
        [SerializeField] private uint _capacity = 100;
        [SerializeField] private uint _spawnAmount = 100;
        [SerializeField] private uint _despawnAmount = 50;

        private void Awake()
        {
            _capacity = (uint)Mathf.Max(_capacity, 1);

            OnAwake();
        }

        private void OnDestroy()
        {
            OnDispose();
        }

        public void Spawn()
        {
            OnSpawn();
        }

        public void Despawn()
        {
            OnDespawn();
        }

        partial void OnAwake();

        partial void OnDispose();

        partial void OnSpawn();

        partial void OnDespawn();
    }
}

#if UNITY_COLLECTIONS

namespace EncosyTower.Samples.Pooling
{
    using EncosyTower.Pooling;
    using EncosyTower.Pooling.Native;
    using Unity.Collections;
    using Unity.Mathematics;
    using Unity.Profiling;
    using UnityEngine;
    using UnityEngine.SceneManagement;

    partial class NativeGameObjectPooler
    {
        private static readonly ProfilerMarker s_rentMarker = new("Native GameObject Pooler Rent");
        private static readonly ProfilerMarker s_returnMarker = new("Native GameObject Pooler Return");

        private NativeGameObjectPool _pool;
        private NativeList<GameObjectInfo> _result;

        partial void OnAwake()
        {
            var rentScene = SceneManager.CreateScene("scene-rented");
            var returnScene = SceneManager.CreateScene("scene-returned");

            _result = new NativeList<GameObjectInfo>(Allocator.Persistent);

            _pool = new NativeGameObjectPool(
                  _template.GetInstanceID()
                , (int)_capacity
                , float3.zero
                , quaternion.identity
                , new float3(1f)
                , rentScene
                , returnScene
            );
        }

        partial void OnDispose()
        {
            _pool.Dispose();
            _result.Dispose();
        }

        partial void OnSpawn()
        {
            s_rentMarker.Begin();
            {
                _pool.Rent((int)_spawnAmount, _result, NativeRentingOptions.Everything);
            }
            s_rentMarker.End();
        }

        partial void OnDespawn()
        {
            s_returnMarker.Begin();
            {
                var length = Mathf.Min(_result.Length, (int)_despawnAmount);
                var startIndex = _result.Length - length;
                var slice = _result.AsArray().Slice(startIndex, length);

                _pool.Return(slice, NativeReturningOptions.Everything);
                _result.RemoveRange(startIndex, length);
            }
            s_returnMarker.End();
        }
    }
}

#endif
