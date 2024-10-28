#if UNITY_ENTITIES && LATIOS_FRAMEWORK

namespace EncosyTower.Modules.Entities
{
    public interface IPhysicsBufferLookups { }

    public interface IPhysicsComponentLookups { }

    public interface IPhysicsEnableableComponentLookups { }
}

namespace EncosyTower.Modules.Entities.Lookups
{
    using Latios.Psyshock;
    using Unity.Entities;

    public interface IPhysicsBufferLookupRO<T>
        where T : unmanaged, IBufferElementData
    {
        void HasBuffer(SafeEntity entity, out Bool<T> result);

        void DidChange(SafeEntity entity, uint version, out Bool<T> result);

        bool TryGetBuffer(SafeEntity entity, out DynamicBuffer<T> bufferData);

        DynamicBuffer<T> GetBuffer(SafeEntity entity, T _);

        void GetBuffer(SafeEntity entity, out DynamicBuffer<T> bufferData);

        void IsBufferEnabled(SafeEntity entity, out Bool<T> result);
    }

    public interface IPhysicsBufferLookupRW<T> : IPhysicsBufferLookupRO<T>
        where T : unmanaged, IBufferElementData
    {
        void SetBufferEnabled(SafeEntity entity, Bool<T> value);
    }

    public interface IPhysicsComponentLookupRO<T>
        where T : unmanaged, IComponentData
    {
        void HasComponent(SafeEntity entity, out Bool<T> result);

        void DidChange(SafeEntity entity, uint version, out Bool<T> result);

        bool TryGetComponent(SafeEntity entity, out T componentData);

        T GetComponentData(SafeEntity entity, T _);

        void GetComponentData(SafeEntity entity, out T componentData);
    }

    public interface IPhysicsComponentLookupRW<T> : IPhysicsComponentLookupRO<T>
        where T : unmanaged, IComponentData
    {
        void SetComponentData(SafeEntity entity, T componentData);

        void GetRefRW(SafeEntity entity, out RefRW<T> result);

        void GetRefRW(SafeEntity entity, T _, out RefRW<T> result);

        void GetRefRWOptional(SafeEntity entity, out RefRW<T> result);

        void GetRefRWOptional(SafeEntity entity, T _, out RefRW<T> result);
    }

    public interface IPhysicsEnableableComponentLookupRO<T>
        where T : unmanaged, IComponentData, IEnableableComponent
    {
        void HasComponent(SafeEntity entity, out Bool<T> result);

        void DidChange(SafeEntity entity, uint version, out Bool<T> result);

        bool TryGetComponent(SafeEntity entity, out T componentData);

        T GetComponentData(SafeEntity entity, T _);

        void GetComponentData(SafeEntity entity, out T componentData);

        void IsComponentEnabled(SafeEntity entity, out Bool<T> result);
    }

    public interface IPhysicsEnableableComponentLookupRW<T> : IPhysicsEnableableComponentLookupRO<T>
        where T : unmanaged, IComponentData, IEnableableComponent
    {
        void SetComponentData(SafeEntity entity, T componentData);

        void GetRefRW(SafeEntity entity, out RefRW<T> result);

        void GetRefRW(SafeEntity entity, T _, out RefRW<T> result);

        void GetRefRWOptional(SafeEntity entity, out RefRW<T> result);

        void GetRefRWOptional(SafeEntity entity, T _, out RefRW<T> result);

        void SetComponentEnabled(SafeEntity entity, Bool<T> value);

        void SetComponentEnabledOptional(SafeEntity entity, Bool<T> value);
    }
}

#endif
