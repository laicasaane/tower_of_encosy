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

                Stats.Baker.Bake(this, entity)
                    .CreateAllStats()
                    .CreateComponentData<AffectorStats>()
                    .AddComponentToEntity();

                //AddComponent(entity, new AffectorStats { value = new })
                AddComponent<Lifetime>(entity);
                AddComponent<IsLivingTag>(entity);
                AddBuffer<ModifierHandle>(entity);
                AddComponent<OmitLinkedEntityGroupFromPrefabInstance>(entity);
            }
        }
    }
}
