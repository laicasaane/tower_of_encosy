#if UNITY_MATHEMATICS

using EncosyTower.Common;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace EncosyTower.Pooling
{
    internal readonly record struct PoolRecord(
          SceneObjectPool Pool
        , float3 DefaultPosition
        , float3 DefaultRotation
        , float3 DefaultScale
    );

    public readonly record struct KeyEntry<TKey>(
          TKey Key
        , Scene Scene
        , uint InitialCapacity
        , Option<Vector3> DefaultPosition = default
        , Option<Vector3> DefaultRotation = default
        , Option<Vector3> DefaultScale = default
    );

    public readonly record struct KeyAmount<TKey>(TKey Key, int EstimatedAmount);
}

#endif
