using Unity.Entities;
using UnityEngine;

namespace EncosyTower.Samples.Stats
{
    internal class AffectorStatsAuthoring : MonoBehaviour
    {
        private class Baker : Baker<AffectorStatsAuthoring>
        {
            public override void Bake(AffectorStatsAuthoring authoring)
            {
                var entity = GetEntity(authoring, TransformUsageFlags.None);
                var statIndices = Stats.Baking.Begin(StatSystem.API.BakeStatComponents(this, entity))
                    .SetStat(Stats.Hp.Create())
                    .SetStat(Stats.MoveSpeed.Create())
                    .SetStat(Stats.DirectionFlags.Create())
                    .SetStat(Stats.MotionFlags.Create())
                    .FinishThenAddComponent<AffectorStats>();

                //AddComponent(entity, new AffectorStats { value = new })
                AddComponent<Lifetime>(entity);
                AddComponent<IsLivingTag>(entity);
                AddBuffer<ModifierHandle>(entity);
                AddComponent<OmitLinkedEntityGroupFromPrefabInstance>(entity);
            }
        }
    }
}
