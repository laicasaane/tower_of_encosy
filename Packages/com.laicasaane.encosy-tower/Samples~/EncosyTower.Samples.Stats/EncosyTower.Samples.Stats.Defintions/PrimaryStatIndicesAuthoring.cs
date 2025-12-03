using Unity.Entities;
using UnityEngine;

namespace EncosyTower.Samples.Stats
{
    internal class PrimaryStatIndicesAuthoring : MonoBehaviour
    {
        public float hp;
        public float moveSpeed;
        public DirectionFlag direction;
        public MotionFlag motion;

        private class Baker : Baker<PrimaryStatIndicesAuthoring>
        {
            public override void Bake(PrimaryStatIndicesAuthoring authoring)
            {
                var entity = GetEntity(authoring, TransformUsageFlags.None);
                var statBaker = StatSystem.API.BakeStatComponents(this, entity);

                AddComponent(entity, new PrimaryStatIndices {
                    value = new StatIndices {
                        hp = statBaker.CreateStatHandle(new Hp(authoring.hp), false).index,
                        moveSpeed = statBaker.CreateStatHandle(new MoveSpeed(authoring.moveSpeed), false).index,
                        direction = statBaker.CreateStatHandle(new DirectionFlags(authoring.direction), false).index,
                        motion = statBaker.CreateStatHandle(new MotionFlags(authoring.motion), false).index,
                    }
                });

                AddComponent<OmitLinkedEntityGroupFromPrefabInstance>(entity);
            }
        }
    }
}
