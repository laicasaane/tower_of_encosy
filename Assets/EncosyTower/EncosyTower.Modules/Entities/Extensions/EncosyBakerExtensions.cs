#if UNITY_ENTITIES

using System.Diagnostics.CodeAnalysis;
using Unity.Entities;

namespace EncosyTower.Modules.Entities
{
    public static class EncosyBakerExtensions
    {
        public static void AddComponentEnabled<T>([NotNull] this IBaker self, Entity entity, bool value)
            where T : struct, IComponentData, IEnableableComponent
        {
            self.AddComponent<T>(entity);
            self.SetComponentEnabled<T>(entity, value);
        }

        public static void AddComponentEnabled<T>([NotNull] this IBaker self, Entity entity, Bool<T> component)
            where T : struct, IComponentData, IEnableableComponent
        {
            self.AddComponent<T>(entity);
            self.SetComponentEnabled<T>(entity, component);
        }
    }
}

#endif
