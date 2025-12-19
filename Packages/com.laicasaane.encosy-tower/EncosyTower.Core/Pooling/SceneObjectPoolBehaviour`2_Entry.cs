#if UNITY_MATHEMATICS

using EncosyTower.Common;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace EncosyTower.Pooling
{
    partial class SceneObjectPoolBehaviour<TKey, TKeyComparer>
    {
        protected readonly record struct Entry(
              TKey Key
            , Scene Scene
            , uint InitialCapacity
            , Option<Vector3> DefaultPosition = default
            , Option<Vector3> DefaultRotation = default
            , Option<Vector3> DefaultScale = default
        );

        private readonly record struct PoolRecord(
              SceneObjectPool Pool
            , float3 DefaultPosition
            , float3 DefaultRotation
            , float3 DefaultScale
        );
    }
}

#endif
