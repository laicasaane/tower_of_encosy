using System;
using EncosyTower.Entities.Stats;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Samples.Entities.Stats
{
    [StatSystem(StatDataSize.Size8)]
    public static partial class StatSystem { }

    public enum DirectionType : byte { Forward, Backward, Up, Down, Left, }

    [Flags]
    public enum MotionFlag : byte
    {
        Default      = 0,
        CannotMove   = 1 << 0,
        CannotAttack = 1 << 1,
        IsFrozen     = 1 << 2,
    }

    [StatCollection(typeof(StatSystem), 1000)]
    public partial struct Stats : IComponentData
    {
        [StatData(StatVariantType.Float)]   public partial struct Hp { }
        [StatData(StatVariantType.Float)]   public partial struct MoveSpeed { }
        [StatData(typeof(DirectionType))]   public partial struct Direction { }
        [StatData(StatVariantType.Half2)]   public partial struct DirectionVector { }
        [StatData(typeof(MotionFlag))]      public partial struct Motion { }

        partial struct TypeId { }
        partial struct Indices { }
        partial struct StatIndices { }
        partial struct StatHandles { }

        static partial class Options
        {
            partial struct Data { }
            partial struct ProduceChangeEvents { }
        }

        static partial class Baker { }
        partial struct Baker<T> { }
        static partial class Accessor { }
        partial struct Accessor<T> { }
        partial struct Reader<T> { }
    }

    static partial class StatsExtensions { }

    internal class StatsAuthoring : MonoBehaviour
    {
        public float hp;
        public float moveSpeed;
        public DirectionType direction;
        public float2 directionVector;
        public MotionFlag motion;

        private class Baker : Baker<StatsAuthoring>
        {
            public override void Bake(StatsAuthoring authoring)
            {
                var entity = GetEntity(authoring, TransformUsageFlags.None);

                Stats.Baker.Bake(this, entity)
                    .CreateStat(Stats.Hp.Params.Create(authoring.hp))
                    .CreateStat(Stats.MoveSpeed.Params.Create(authoring.moveSpeed))
                    .CreateStat(Stats.Direction.Params.Create(authoring.direction))
                    .CreateStat(Stats.DirectionVector.Params.Create((half2)authoring.directionVector))
                    .CreateStat(Stats.Motion.Params.Create(authoring.motion))
                    .CreateComponentData<Stats>()
                    .AddComponentToEntity();

                AddComponent<OmitLinkedEntityGroupFromPrefabInstance>(entity);
            }
        }
    }
}
