using Unity.Entities;
using UnityEngine;

namespace EncosyTower.Samples.Stats
{
    internal class AffectorStatIndicesAuthoring : MonoBehaviour
    {
        private class Baker : Baker<AffectorStatIndicesAuthoring>
        {
            public override void Bake(AffectorStatIndicesAuthoring authoring)
            {
                var entity = GetEntity(authoring, TransformUsageFlags.None);
                var statBaker = StatSystem.API.BakeStatComponents(this, entity);

                AddComponent(entity, new AffectorStatIndices {
                    value = new StatIndices {
                        hp = statBaker.CreateStatHandle(new Hp(), false).index,
                        moveSpeed = statBaker.CreateStatHandle(new MoveSpeed(), false).index,
                        direction = statBaker.CreateStatHandle(new DirectionFlags(), false).index,
                        motion = statBaker.CreateStatHandle(new MotionFlags(), false).index,
                    }
                });

                AddComponent<Lifetime>(entity);
                AddComponent<IsLivingTag>(entity);
                AddBuffer<ModifierHandle>(entity);
                AddComponent<OmitLinkedEntityGroupFromPrefabInstance>(entity);
            }
        }
    }
}
