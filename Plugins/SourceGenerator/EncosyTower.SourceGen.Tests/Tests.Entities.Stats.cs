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

    [StatCollection(typeof(StatSystem), 1000)]
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

        partial struct TypeId { }

        partial struct Indices { }

        partial struct StatIndices { }

        partial struct StatHandles { }

        static partial class Baker { }

        partial struct Baker<T> { }

        static partial class Accessor { }

        partial struct Accessor<T> { }
    }

    static partial class StatsExtensions
    {
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

                Stats.Baker.Bake(this, entity)
                    .CreateStat(Stats.Hp.Create(authoring.hp))
                    .CreateStat(Stats.MoveSpeed.Create(authoring.moveSpeed))
                    .CreateStat(Stats.Direction.Create(authoring.direction))
                    .CreateStat(Stats.Motion.Create(authoring.motion))
                    .CreateComponentData<Stats>()
                    .AddComponentToEntity();

                AddComponent<OmitLinkedEntityGroupFromPrefabInstance>(entity);
            }
        }
    }
}
