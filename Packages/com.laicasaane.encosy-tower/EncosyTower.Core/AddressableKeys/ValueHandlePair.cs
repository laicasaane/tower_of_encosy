#if UNITY_ADDRESSABLES

using UnityEngine.ResourceManagement.AsyncOperations;

namespace EncosyTower.AddressableKeys
{
    public readonly record struct ValueHandlePair<TValue>(
          TValue Value
        , AsyncOperationHandle<TValue> Handle
    );

    public readonly record struct ValueHandlePair<TValue, THandle>(
          TValue Value
        , AsyncOperationHandle<THandle> Handle
    );
}

#endif
