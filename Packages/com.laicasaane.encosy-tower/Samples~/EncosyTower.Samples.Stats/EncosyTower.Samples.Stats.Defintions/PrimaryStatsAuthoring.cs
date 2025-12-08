using Unity.Entities;
using UnityEngine;

namespace EncosyTower.Samples.Stats
{
    internal class PrimaryStatsAuthoring : MonoBehaviour
    {
        public float hp;
        public float moveSpeed;
        public DirectionFlag direction;
        public MotionFlag motion;

        private class Baker : Baker<PrimaryStatsAuthoring>
        {
            public override void Bake(PrimaryStatsAuthoring authoring)
            {
                var entity = GetEntity(authoring, TransformUsageFlags.None);

                Stats.Baking.Begin(StatSystem.API.BakeStatComponents(this, entity))
                    .SetStat(Stats.Hp.Create(authoring.hp))
                    .SetStat(Stats.MoveSpeed.Create(authoring.moveSpeed))
                    .SetStat(Stats.DirectionFlags.Create(authoring.direction))
                    .SetStat(Stats.MotionFlags.Create(authoring.motion))
                    .FinishThenAddComponent<PrimaryStats>();

                AddComponent<OmitLinkedEntityGroupFromPrefabInstance>(entity);
            }
        }
    }
}
