#if UNITY_ENTITIES

using Unity.Collections;
using Unity.Entities;

namespace EncosyTower.Modules.Entities
{
    public static class EncosyEntityManagerExtensions
    {
        public static void SetComponentEnabled<T>(
              this EntityManager em
            , in NativeArray<Entity> entities
            , bool value
        )
            where T :
#if UNITY_DISABLE_MANAGED_COMPONENTS
            unmanaged,
#endif
            IComponentData, IEnableableComponent
        {
            var length = entities.Length;

            for (var i = 0; i < length; i++)
            {
                em.SetComponentEnabled<T>(entities[i], value);
            }
        }

        public static void SetComponentEnabled<T>(
              this EntityManager em
            , in NativeArray<Entity>.ReadOnly entities
            , bool value
        )
            where T :
#if UNITY_DISABLE_MANAGED_COMPONENTS
            unmanaged,
#endif
            IComponentData, IEnableableComponent
        {
            var length = entities.Length;

            for (var i = 0; i < length; i++)
            {
                em.SetComponentEnabled<T>(entities[i], value);
            }
        }

        public static void SetComponentEnabled<T>(
              this EntityManager em
            , in NativeArray<Entity> entities
            , Bool<T> component
        )
            where T :
#if UNITY_DISABLE_MANAGED_COMPONENTS
            unmanaged,
#endif
            IComponentData, IEnableableComponent
        {
            var length = entities.Length;

            for (var i = 0; i < length; i++)
            {
                em.SetComponentEnabled<T>(entities[i], component);
            }
        }

        public static void SetComponentEnabled<T>(
              this EntityManager em
            , in NativeArray<Entity>.ReadOnly entities
            , Bool<T> component
        )
            where T :
#if UNITY_DISABLE_MANAGED_COMPONENTS
            unmanaged,
#endif
            IComponentData, IEnableableComponent
        {
            var length = entities.Length;

            for (var i = 0; i < length; i++)
            {
                em.SetComponentEnabled<T>(entities[i], component);
            }
        }
    }
}

#endif
