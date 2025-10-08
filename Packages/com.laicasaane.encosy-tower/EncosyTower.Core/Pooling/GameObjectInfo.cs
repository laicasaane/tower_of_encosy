using System;
using System.Runtime.InteropServices;
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

    [Serializable, StructLayout(LayoutKind.Explicit, Size = 4 + 4 + 4)]
    public struct GameObjectInfo
    {
        public const int OFFSET_GAMEOBJECT_ID = 0;
        public const int OFFSET_TRANSFORM_ID = 0 + 4;
        public const int OFFSET_TRANSFORM_ARRAY_INDEX = 0 + 4 + 4;

        [FieldOffset(OFFSET_GAMEOBJECT_ID)]         public GameObjectId gameObjectId;
        [FieldOffset(OFFSET_TRANSFORM_ID)]          public TransformId transformId;
        [FieldOffset(OFFSET_TRANSFORM_ARRAY_INDEX)] public int transformArrayIndex;
    }
}
