using Latios.Transforms;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace Module.EntityAuthoring
{
    internal class PrefabAuthoring : MonoBehaviour
    {
        public GameObject prefab;
        public bool withoutTransform;

        private void OnValidate()
        {
            if (prefab)
            {
                this.name = prefab.name;
            }
        }

        private sealed class Baker : Baker<PrefabAuthoring>
        {
            public override void Bake(PrefabAuthoring authoring)
            {
                var entity = GetEntity(authoring, TransformUsageFlags.None);
                AddComponent<BakingOnlyEntity>(entity);

                var prefab = GetEntity(authoring.prefab, TransformUsageFlags.Dynamic);

                if (authoring.withoutTransform)
                {
                    AddComponent(entity, new NeedsRemoveTransform {
                        target = prefab,
                    });
                }
            }
        }
    }

    internal struct NeedsRemoveTransform : IComponentData
    {
        public Entity target;
    }

    [WorldSystemFilter(WorldSystemFilterFlags.BakingSystem)]
    [UpdateInGroup(typeof(PostBakingSystemGroup))]
    internal partial struct RemoveLegSystem : ISystem
    {
        private EntityQuery _query;

        public void OnCreate(ref SystemState state)
        {
            _query = SystemAPI.QueryBuilder()
                .WithAll<NeedsRemoveTransform>()
                .Build();
        }

        public void OnUpdate(ref SystemState state)
        {
            var em = state.EntityManager;
            var arr = _query.ToComponentDataArray<NeedsRemoveTransform>(Allocator.Temp);

            foreach (var item in arr)
            {
                var target = item.target;
                em.RemoveComponent<LinkedEntityGroup>(target);
                em.RemoveComponent<WorldTransform>(target);
            }
        }
    }
}
