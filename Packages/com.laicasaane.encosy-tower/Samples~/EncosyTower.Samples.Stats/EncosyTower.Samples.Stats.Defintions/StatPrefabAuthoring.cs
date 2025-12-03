using Unity.Entities;
using UnityEngine;

namespace EncosyTower.Samples.Stats
{
    internal class StatPrefabAuthoring : MonoBehaviour
    {
        public GameObject prefab;

        private class Baker : Baker<StatPrefabAuthoring>
        {
            public override void Bake(StatPrefabAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.None);
                var prefab = GetEntity(authoring.prefab, TransformUsageFlags.None);

                AddComponent(entity, new StatPrefabRef { value = prefab });
            }
        }
    }
}
