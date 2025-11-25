using System;
using EncosyTower.Entities.Stats;
using EncosyTower.EnumExtensions;

namespace EncosyTower.Samples.Stats
{
    // NOTICE: Below are stat data, not stat component.
    // They act as a type-safe interface to the StatSystem.Stat buffer element.
    // StatSystem.Stat uses a union storage to store stat values.

    // NOTICE: These type should be use with StatHandle<TStatData>.
    // For example: StatHandle<Hp>, StatHandle<MoveSpeed>, etc.

    [StatData(StatVariantType.Half)]
    public partial struct Hp { }

    [StatData(StatVariantType.Half)]
    public partial struct MoveSpeed { }

    [StatData(typeof(DirectionFlag))]
    public partial struct DirectionFlags { }

    [StatData(typeof(MotionFlag))]
    public partial struct MotionFlags { }

    [Flags, EnumExtensions]
    public enum DirectionFlag : byte
    {
        None           = 0b_0000_0000,
        TargetLocation = 0b_0000_0001,
        Upward         = 0b_0000_0010,
        Rightward      = 0b_0000_0100,
        Leftward       = 0b_0000_1000,
    }

    partial class DirectionFlagExtensions { }

    [Flags, EnumExtensions]
    public enum MotionFlag : byte
    {
        Default           = 0b_0000_0000,
        CannotMove        = 0b_0001_0000,
        CannotAttack      = 0b_0010_0000,
        IsFrozen          = 0b_0100_0000,
    }

    partial class MotionFlagExtensions { }

}
