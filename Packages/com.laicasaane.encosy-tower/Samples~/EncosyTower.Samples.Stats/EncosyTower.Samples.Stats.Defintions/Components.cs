using EncosyTower.Entities.Stats;
using Unity.Entities;

namespace EncosyTower.Samples.Stats
{
    public struct PrimaryStats : IComponentData
    {
        public Stats value;
    }

    public struct AffectorStats : IComponentData
    {
        public Stats value;
    }

    public struct StatSpawnCommand : IComponentData
    {
        public int statAmount;
        public int affectorAmount;
    }

    public struct RandomRef : IComponentData
    {
        public Unity.Mathematics.Random value;
    }

    [StatCollection(typeof(StatSystem))]
    public partial struct Stats
    {
        // NOTICE: Below are stat data, not stat component.
        // They act as a type-safe interface to the StatSystem.Stat buffer element.
        // StatSystem.Stat uses a union storage to store stat values.

        // NOTICE: These type should be use with StatHandle<TStatData>.
        // For example: StatHandle<Hp>, StatHandle<MoveSpeed>, etc.

        [StatData(StatVariantType.Float)]
        public partial struct Hp { }

        [StatData(StatVariantType.Float)]
        public partial struct MoveSpeed { }

        [StatData(typeof(DirectionFlag))]
        public partial struct DirectionFlags { }

        [StatData(typeof(MotionFlag))]
        public partial struct MotionFlags { }
    }
}
