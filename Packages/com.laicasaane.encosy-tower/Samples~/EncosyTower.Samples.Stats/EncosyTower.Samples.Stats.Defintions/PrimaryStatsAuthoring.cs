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

                Stats.Baker.Bake(this, entity)
                    .CreateStats(
                          Stats.Hp.Create(authoring.hp)
                        , Stats.MoveSpeed.Create(authoring.moveSpeed)
                        , Stats.DirectionFlags.Create(authoring.direction)
                        , Stats.MotionFlags.Create(authoring.motion)
                    )
                    .CreateComponentData<PrimaryStats>()
                    .AddComponentToEntity();

                AddComponent<OmitLinkedEntityGroupFromPrefabInstance>(entity);
            }
        }
    }
}
