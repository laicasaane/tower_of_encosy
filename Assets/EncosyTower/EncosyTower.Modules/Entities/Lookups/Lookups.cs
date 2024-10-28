#if UNITY_ENTITIES

namespace EncosyTower.Modules.Entities
{
    public interface IBufferLookups { }

    public interface IComponentLookups { }

    public interface IEnableableComponentLookups { }
}

namespace EncosyTower.Modules.Entities.Lookups
{
    using Unity.Entities;

    public interface ILookups
    {
        void Update(ref SystemState state);

        void Update(SystemBase system);
    }

    public interface IBufferLookupRO<T>
        where T : unmanaged, IBufferElementData
    {
        void HasBuffer(Entity entity, out Bool<T> result);

        void DidChange(Entity entity, uint version, out Bool<T> result);

        bool TryGetBuffer(Entity entity, out DynamicBuffer<T> bufferData);

        DynamicBuffer<T> GetBuffer(Entity entity, T _);

        void GetBuffer(Entity entity, out DynamicBuffer<T> bufferData);

        void IsBufferEnabled(Entity entity, out Bool<T> result);
    }

    public interface IBufferLookupRW<T> : IBufferLookupRO<T>
        where T : unmanaged, IBufferElementData
    {
        void SetBufferEnabled(Entity entity, Bool<T> value);
    }

    public interface IComponentLookupRO<T>
        where T : unmanaged, IComponentData
    {
        void HasComponent(Entity entity, out Bool<T> result);

        void HasComponent(SystemHandle systemHandle, out Bool<T> result);

        void DidChange(Entity entity, uint version, out Bool<T> result);

        bool TryGetComponent(Entity entity, out T componentData);

        T GetComponentData(Entity entity, T _);

        T GetComponentData(SystemHandle systemHandle, T _);

        void GetComponentData(Entity entity, out T componentData);

        void GetComponentData(SystemHandle systemHandle, out T componentData);

        void GetRefRO(Entity entity, out RefRO<T> result);

        void GetRefRO(Entity entity, T _, out RefRO<T> result);

        void GetRefROOptional(Entity entity, out RefRO<T> result);

        void GetRefROOptional(Entity entity, T _, out RefRO<T> result);
    }

    public interface IComponentLookupRW<T> : IComponentLookupRO<T>
        where T : unmanaged, IComponentData
    {
        void SetComponentData(Entity entity, T componentData);

        void SetComponentData(SystemHandle systemHandle, T componentData);

        void GetRefRW(Entity entity, out RefRW<T> result);

        void GetRefRW(SystemHandle systemHandle, out RefRW<T> result);

        void GetRefRW(Entity entity, T _, out RefRW<T> result);

        void GetRefRW(SystemHandle systemHandle, T _, out RefRW<T> result);

        void GetRefRWOptional(Entity entity, out RefRW<T> result);

        void GetRefRWOptional(Entity entity, T _, out RefRW<T> result);

        void SetComponentDataOptional(Entity entity, T componentData);

        void SetComponentDataOptional(SystemHandle systemHandle, T componentData);
    }

    public interface IEnableableComponentLookupRO<T>
        where T : unmanaged, IComponentData, IEnableableComponent
    {
        void HasComponent(Entity entity, out Bool<T> result);

        void HasComponent(SystemHandle systemHandle, out Bool<T> result);

        void DidChange(Entity entity, uint version, out Bool<T> result);

        bool TryGetComponent(Entity entity, out T componentData);

        T GetComponentData(Entity entity, T _);

        T GetComponentData(SystemHandle systemHandle, T _);

        void GetComponentData(Entity entity, out T componentData);

        void GetComponentData(SystemHandle systemHandle, out T componentData);

        void GetRefRO(Entity entity, out RefRO<T> result);

        void GetRefRO(Entity entity, T _, out RefRO<T> result);

        void GetRefROOptional(Entity entity, out RefRO<T> result);

        void GetRefROOptional(Entity entity, T _, out RefRO<T> result);

        void IsComponentEnabled(Entity entity, out Bool<T> result);

        void IsComponentEnabled(SystemHandle systemHandle, out Bool<T> result);

        void GetEnabledRefRO(Entity entity, out EnabledRefRO<T> result);

        void GetEnabledRefRO(Entity entity, T _, out EnabledRefRO<T> result);

        void GetEnabledRefROOptional(Entity entity, out EnabledRefRO<T> result);

        void GetEnabledRefROOptional(Entity entity, T _, out EnabledRefRO<T> result);
    }

    public interface IEnableableComponentLookupRW<T> : IEnableableComponentLookupRO<T>
        where T : unmanaged, IComponentData, IEnableableComponent
    {
        void SetComponentData(Entity entity, T componentData);

        void SetComponentData(SystemHandle systemHandle, T componentData);

        void GetRefRW(Entity entity, out RefRW<T> result);

        void GetRefRW(Entity entity, T _, out RefRW<T> result);

        void GetRefRW(SystemHandle systemHandle, out RefRW<T> result);

        void GetRefRW(SystemHandle systemHandle, T _, out RefRW<T> result);

        void GetRefRWOptional(Entity entity, out RefRW<T> result);

        void GetRefRWOptional(Entity entity, T _, out RefRW<T> result);

        void SetComponentDataOptional(Entity entity, T componentData);

        void SetComponentDataOptional(SystemHandle systemHandle, T componentData);

        void SetComponentEnabled(Entity entity, Bool<T> value);

        void SetComponentEnabled(SystemHandle systemHandle, Bool<T> value);

        void GetEnabledRefRW(Entity entity, out EnabledRefRW<T> result);

        void GetEnabledRefRW(Entity entity, T _, out EnabledRefRW<T> result);

        void GetEnabledRefRWOptional(Entity entity, out EnabledRefRW<T> result);

        void GetEnabledRefRWOptional(Entity entity, T _, out EnabledRefRW<T> result);

        void SetComponentEnabledOptional(Entity entity, Bool<T> value);
    }
}

#endif
