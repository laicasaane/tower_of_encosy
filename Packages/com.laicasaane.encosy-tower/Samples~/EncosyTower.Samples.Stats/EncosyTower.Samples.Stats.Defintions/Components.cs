using EncosyTower.Entities.Stats;
using Unity.Entities;

namespace EncosyTower.Samples.Stats
{
    public struct StatIndices
    {
        public const uint COUNT = 4;
        public const uint LAST_STAT_OF_TYPE_HALF = 1;

        public StatIndex<Hp> hp;
        public StatIndex<MoveSpeed> moveSpeed;
        public StatIndex<DirectionFlags> direction;
        public StatIndex<MotionFlags> motion;

        public readonly bool TryGetHandle(int index, out StatIndex statHandle)
        {
            switch (index)
            {
                case 0: statHandle = hp; return true;
                case 1: statHandle = moveSpeed; return true;
                case 2: statHandle = direction; return true;
                case 3: statHandle = motion; return true;
                default: statHandle = default; return false;
            }
        }
    }

    public struct PrimaryStatIndices : IComponentData
    {
        public StatIndices value;
    }

    public struct AffectorStatIndices : IComponentData
    {
        public StatIndices value;
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
