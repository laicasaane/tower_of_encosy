using System.Runtime.CompilerServices;
using EncosyTower.Common;
using Unity.Mathematics;
using UnityEngine.SceneManagement;

namespace EncosyTower.Pooling.Native
{
#if UNITY_6000_2_OR_NEWER
    using EntityId = UnityEngine.EntityId;
#else
    using EntityId = System.Int32;
#endif

    public struct NativePrefabInfo
    {
        public readonly float3 Position;
        public readonly float3 Scale;
        public readonly quaternion Rotation;
        public readonly EntityId EntityId;

        private Scene _rentScene;
        private Scene _returnScene;
        private ByteBool _hasRentScene;
        private ByteBool _hasReturnScene;

        public NativePrefabInfo(
              EntityId entityId
            , in float3 position
            , in quaternion rotation
            , in float3 scale
            , Option<Scene> rentScene = default
            , Option<Scene> returnScene = default
        )
        {
            Position = position;
            Scale = scale;
            Rotation = rotation;
            EntityId = entityId;

            _hasRentScene = rentScene.TryGetValue(out var rentSceneValue);
            _rentScene = rentSceneValue;

            _hasReturnScene = returnScene.TryGetValue(out var returnSceneValue);
            _returnScene = returnSceneValue;
        }

        public Option<Scene> RentScene
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            readonly get
            {
                return _hasRentScene ? _rentScene : Option.None;
            }

            set
            {
                _hasRentScene = value.TryGetValue(out var scene);
                _rentScene = scene;
            }
        }

        public Option<Scene> ReturnScene
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            readonly get
            {
                return _hasReturnScene ? _returnScene : Option.None;
            }

            set
            {
                _hasReturnScene = value.TryGetValue(out var scene);
                _returnScene = scene;
            }
        }
    }
}
