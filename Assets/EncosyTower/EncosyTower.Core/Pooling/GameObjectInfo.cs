using EncosyTower.UnityExtensions;
using UnityEngine;

namespace EncosyTower.Pooling
{
#if UNITY_6000_2_OR_NEWER
    using GameObjectId = UnityEntityId<GameObject>;
    using TransformId = UnityEntityId<Transform>;
#else
    using GameObjectId = UnityInstanceId<GameObject>;
    using TransformId = UnityInstanceId<Transform>;
#endif

    public struct GameObjectInfo
    {
        public GameObjectId gameObjectId;
        public TransformId transformId;
        public int transformArrayIndex;
    }
}
