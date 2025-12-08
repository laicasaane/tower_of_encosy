using System;
using EncosyTower.Entities.Stats;
using Unity.Entities;
using UnityEngine;

namespace EncosyTower.Tests.Entities.Stats
{
    [StatSystem(StatDataSize.Size8)]
    public static partial class StatSystem
    {

    }

    [StatData(StatVariantType.Float)]
    public partial struct Hp { }

    [StatData(typeof(DirectionType), SingleValue = true)]
    public partial struct Direction { }

    public enum DirectionType : byte
    {
        Forward, Backward, Up, Down, Left,
    }

    [Flags]
    public enum MotionFlag : byte
    {
        Default           = 0,
        CannotMove        = 1 << 0,
        CannotAttack      = 1 << 1,
        IsFrozen          = 1 << 2,
    }

    [StatCollection(typeof(StatSystem))]
    public partial struct Stats : IComponentData
    {
        [StatData(StatVariantType.Float)]
        public partial struct Hp { }

        [StatData(StatVariantType.Float)]
        public partial struct MoveSpeed { }

        [StatData(typeof(DirectionType))]
        public partial struct Direction { }

        [StatData(typeof(MotionFlag))]
        public partial struct Motion { }

        partial struct Indices
        {
        }

        partial struct Handles
        {
        }
    }

    internal class StatsAuthoring : MonoBehaviour
    {
        public float hp;
        public float moveSpeed;
        public DirectionType direction;
        public MotionFlag motion;

        private class Baker : Baker<StatsAuthoring>
        {
            public override void Bake(StatsAuthoring authoring)
            {
                var entity = GetEntity(authoring, TransformUsageFlags.None);

                Stats.Baking.Begin(StatSystem.API.BakeStatComponents(this, entity))
                    .SetStat(Stats.Hp.Create(authoring.hp))
                    .SetStat(Stats.MoveSpeed.Create(authoring.moveSpeed))
                    .SetStat(Stats.Direction.Create(authoring.direction))
                    .SetStat(Stats.Motion.Create(authoring.motion))
                    .FinishThenAddComponent<Stats>();

                AddComponent<OmitLinkedEntityGroupFromPrefabInstance>(entity);
            }
        }
    }
}
