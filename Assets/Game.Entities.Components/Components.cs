using Latios;
using Latios.Kinemation;
using Latios.Transforms;
using Module.GameCommon;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace Module.EntityComponents
{
    public struct EntityId : IComponentData
    {
        public ResId.Serializable value;
    }

    public struct MoveDirection : IComponentData
    {
        public half3 value;
    }

    public struct MoveSpeed : IComponentData
    {
        public half value;
    }

    public struct PlayerInput : IComponentData
    {

    }

    public struct CharAnimTypeBlob
    {
        public BlobArray<CharAnim> types;
    }

    public struct CharAnimTypeBlobRef : IComponentData
    {
        public BlobAssetReference<CharAnimTypeBlob> value;
    }

    public struct AnimClipBlobRef : IComponentData
    {
        public BlobAssetReference<SkeletonClipSetBlob> value;
    }

    public struct CharAnimState : IComponentData
    {
        public CharAnim value;
        public half startTime;
        public half previousDt;
    }

    [InternalBufferCapacity(4)]
    public struct IkHandElement : IBufferElementData
    {
        public FixedString32Bytes boneName;
        public EntityWith<WorldTransform> entity;
        public byte id;
    }
}
