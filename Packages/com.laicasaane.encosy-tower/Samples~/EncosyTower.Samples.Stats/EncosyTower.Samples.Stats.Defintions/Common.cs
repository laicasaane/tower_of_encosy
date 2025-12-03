using System;
using EncosyTower.Entities.Stats;
using EncosyTower.EnumExtensions;
using Unity.Entities;

namespace EncosyTower.Samples.Stats
{
    [Flags, EnumExtensions]
    public enum DirectionFlag : byte
    {
        Default        = 0,
        TargetLocation = 1 << 0,
    }

    partial class DirectionFlagExtensions { }

    [Flags, EnumExtensions]
    public enum MotionFlag : byte
    {
        Default           = 0,
        CannotMove        = 1 << 0,
        CannotAttack      = 1 << 1,
        IsFrozen          = 1 << 2,
    }

    partial class MotionFlagExtensions { }

    public struct StatPrefabRef : IComponentData
    {
        public Entity value;
    }

    public struct AffectorPrefabRef : IComponentData
    {
        public Entity value;
    }

    public struct Lifetime : IComponentData
    {
        public float value;
    }

    public struct IsLivingTag : IComponentData, IEnableableComponent { }

    [InternalBufferCapacity(0)]
    public struct ModifierHandle : IBufferElementData
    {
        public StatModifierHandle value;
    }
}
