using Unity.Entities;
using UnityEngine;

namespace EncosyTower.Samples.Stats
{
    internal class AffectorPrefabAuthoring : MonoBehaviour
    {
        public GameObject prefab;

        private class Baker : Baker<AffectorPrefabAuthoring>
        {
            public override void Bake(AffectorPrefabAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.None);
                var prefab = GetEntity(authoring.prefab, TransformUsageFlags.None);

                AddComponent(entity, new AffectorPrefabRef { value = prefab });
            }
        }
    }
}
