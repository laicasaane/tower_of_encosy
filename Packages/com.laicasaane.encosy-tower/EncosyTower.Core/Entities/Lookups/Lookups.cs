#if UNITY_ENTITIES

namespace EncosyTower.Entities
{
    public interface IBufferLookups { }

    public interface IComponentLookups { }

    public interface IEnableableBufferLookups { }

    public interface IEnableableComponentLookups { }
}

namespace EncosyTower.Entities.Lookups
{
    using EncosyTower.Common;
    using Unity.Entities;

    public interface ILookups
    {
        void Update(ref SystemState state);

        void Update(SystemBase system);
    }

    public interface IBufferLookup
    {
        /// <inheritdoc cref="BufferLookup{T}.EntityExists(Entity)"/>
        bool EntityExists(Entity entity);
    }

    public interface IBufferLookupRO<T> : IBufferLookup
        where T : unmanaged, IBufferElementData
    {
        /// <inheritdoc cref="BufferLookup{T}.HasBuffer(Entity)"/>
        void HasBuffer(Entity entity, out Bool<T> result);

        /// <inheritdoc cref="BufferLookup{T}.HasBuffer(Entity, out bool)"/>
        void HasBuffer(Entity entity, out Bool<T> result, out bool entityExists);

        /// <inheritdoc cref="BufferLookup{T}.DidChange(Entity, uint)"/>
        void DidChange(Entity entity, uint version, out Bool<T> result);

        /// <inheritdoc cref="BufferLookup{T}.TryGetBuffer(Entity, out DynamicBuffer{T})"/>
        bool TryGetBuffer(Entity entity, out DynamicBuffer<T> bufferData);

        /// <inheritdoc cref="BufferLookup{T}.TryGetBuffer(Entity, out DynamicBuffer{T}, out bool)"/>
        bool TryGetBuffer(Entity entity, out DynamicBuffer<T> bufferData, out bool entityExists);

        /// <summary>
        /// Gets the <see cref="DynamicBuffer{T}"/> instance of type T for the specified entity.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <exception cref="System.ArgumentException">Thrown if entity does not have a buffer component of type T.</exception>
        /// <remarks>
        /// Normally, you cannot write to buffers accessed using a BufferLookup instance
        /// in a parallel Job. This restriction is in place because multiple threads could
        /// write to the same buffer, leading to a race condition and nondeterministic results.
        /// However, when you are certain that your algorithm cannot write to the same buffer
        /// from different threads, you can manually disable this safety check by putting
        /// the [NativeDisableParallelForRestriction] attribute on the BufferLookup field
        /// in the Job.
        /// </remarks>
        /// <seealso href="https://docs.unity3d.com/ScriptReference/Unity.Collections.NativeDisableParallelForRestrictionAttribute.html" />
        DynamicBuffer<T> GetBuffer(Entity entity, T _);

        /// <inheritdoc cref="GetBuffer(Entity, T)"/>
        void GetBuffer(Entity entity, out DynamicBuffer<T> bufferData);
    }

    public interface IBufferLookupRW<T> : IBufferLookupRO<T>, IBufferLookup
        where T : unmanaged, IBufferElementData
    {
    }

    public interface IComponentLookup
    {
        /// <inheritdoc cref="ComponentLookup{T}.EntityExists(Entity)"/>
        bool EntityExists(Entity entity);
    }

    public interface IComponentLookupRO<T> : IComponentLookup
        where T : unmanaged, IComponentData
    {
        /// <inheritdoc cref="ComponentLookup{T}.HasComponent(Entity)"/>
        void HasComponent(Entity entity, out Bool<T> result);

        /// <inheritdoc cref="ComponentLookup{T}.HasComponent(Entity, out bool)"/>
        void HasComponent(Entity entity, out Bool<T> result, out bool entityExists);

        /// <inheritdoc cref="ComponentLookup{T}.HasComponent(SystemHandle)"/>
        void HasComponent(SystemHandle systemHandle, out Bool<T> result);

        /// <inheritdoc cref="ComponentLookup{T}.DidChange(Entity, uint)"/>
        void DidChange(Entity entity, uint version, out Bool<T> result);

        /// <inheritdoc cref="ComponentLookup{T}.TryGetComponent(Entity, out T)"/>
        bool TryGetComponent(Entity entity, out T componentData);

        /// <inheritdoc cref="ComponentLookup{T}.TryGetComponent(Entity, out T, out bool)"/>
        bool TryGetComponent(Entity entity, out T componentData, out bool entityExists);

        /// <summary>
        /// Gets the <see cref="IComponentData"/> instance of type T for the specified entity.
        /// </summary>
        /// <param name="entity">The entity.</param>
        T GetComponentData(Entity entity, T _);

        /// <summary>
        /// Gets the <see cref="IComponentData"/> instance of type T for the specified system's associated entity.
        /// </summary>
        /// <param name="systemHandle">The system handle.</param>
        T GetComponentData(SystemHandle systemHandle, T _);

        /// <inheritdoc cref="GetComponentData(Entity, T)" />
        void GetComponentData(Entity entity, out T componentData);

        /// <inheritdoc cref="GetComponentData(SystemHandle, T)" />
        void GetComponentData(SystemHandle systemHandle, out T componentData);

        /// <inheritdoc cref="ComponentLookup{T}.GetRefRO(Entity)"/>
        void GetRefRO(Entity entity, out RefRO<T> result);

        /// <inheritdoc cref="ComponentLookup{T}.GetRefRO(Entity)"/>
        void GetRefRO(Entity entity, T _, out RefRO<T> result);

        /// <inheritdoc cref="ComponentLookup{T}.GetRefROOptional(Entity)"/>
        void GetRefROOptional(Entity entity, out RefRO<T> result);

        /// <inheritdoc cref="ComponentLookup{T}.GetRefROOptional(Entity)"/>
        void GetRefROOptional(Entity entity, T _, out RefRO<T> result);
    }

    public interface IComponentLookupRW<T> : IComponentLookupRO<T>, IComponentLookup
        where T : unmanaged, IComponentData
    {
        /// <summary>
        /// Sets the <see cref="IComponentData"/> instance of type T for the specified entity.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <remarks>
        /// Normally, you cannot write to components accessed using a ComponentLookup instance in a parallel Job.
        /// This restriction is in place because multiple threads could write to the same component,
        /// leading to a race condition and nondeterministic results.
        /// However, when you are certain that your algorithm cannot write to the same component
        /// from different threads, you can manually disable this safety check by putting the
        /// [NativeDisableParallelForRestriction] attribute on the ComponentLookup field in the Job.
        /// </remarks>
        /// <seealso href="https://docs.unity3d.com/ScriptReference/Unity.Collections.NativeDisableParallelForRestrictionAttribute.html" />
        void SetComponentData(Entity entity, T componentData);

        /// <summary>
        /// Sets the <see cref="IComponentData"/> instance of type T for the specified system's associated entity.
        /// </summary>
        /// <param name="systemHandle">The system handle.</param>
        /// <remarks>
        /// Normally, you cannot write to components accessed using a ComponentDataFromEntity instance in a parallel Job.
        /// This restriction is in place because multiple threads could write to the same component,
        /// leading to a race condition and nondeterministic results.
        /// However, when you are certain that your algorithm cannot write to the same component
        /// from different threads, you can manually disable this safety check by putting the
        /// [NativeDisableParallelForRestriction] attribute on the ComponentDataFromEntity field in the Job.
        /// </remarks>
        /// <seealso href="https://docs.unity3d.com/ScriptReference/Unity.Collections.NativeDisableParallelForRestrictionAttribute.html" />
        void SetComponentData(SystemHandle systemHandle, T componentData);

        /// <inheritdoc cref="ComponentLookup{T}.GetRefRW(Entity)"/>
        void GetRefRW(Entity entity, out RefRW<T> result);

        /// <inheritdoc cref="ComponentLookup{T}.GetRefRW(SystemHandle)"/>
        void GetRefRW(SystemHandle systemHandle, out RefRW<T> result);

        /// <inheritdoc cref="ComponentLookup{T}.GetRefRW(Entity)"/>
        void GetRefRW(Entity entity, T _, out RefRW<T> result);

        /// <inheritdoc cref="ComponentLookup{T}.GetRefRW(SystemHandle)"/>
        void GetRefRW(SystemHandle systemHandle, T _, out RefRW<T> result);

        /// <inheritdoc cref="ComponentLookup{T}.GetRefRWOptional(Entity)"/>
        void GetRefRWOptional(Entity entity, out RefRW<T> result);

        /// <inheritdoc cref="ComponentLookup{T}.GetRefRWOptional(Entity)"/>
        void GetRefRWOptional(Entity entity, T _, out RefRW<T> result);

        /// <inheritdoc cref="SetComponentData(Entity, T)"/>
        void SetComponentDataOptional(Entity entity, T componentData);

        /// <inheritdoc cref="SetComponentData(SystemHandle, T)"/>
        void SetComponentDataOptional(SystemHandle systemHandle, T componentData);
    }

    public interface IEnableableBufferLookupRO<T> : IBufferLookupRO<T>
        where T : unmanaged, IBufferElementData, IEnableableComponent
    {
        /// <inheritdoc cref="BufferLookup{T}.IsBufferEnabled(Entity)"/>
        void IsBufferEnabled(Entity entity, out Bool<T> result);

        /// <inheritdoc cref="BufferLookup{T}.GetEnabledRefRO{T2}(Entity)"/>
        void GetEnabledRefRO(Entity entity, out EnabledRefRO<T> result);

        /// <inheritdoc cref="BufferLookup{T}.GetEnabledRefRO{T2}(Entity)"/>
        void GetEnabledRefRO(Entity entity, T _, out EnabledRefRO<T> result);

        /// <inheritdoc cref="BufferLookup{T}.GetEnabledRefROOptional{T2}(Entity)"/>
        void GetEnabledRefROOptional(Entity entity, out EnabledRefRO<T> result);

        /// <inheritdoc cref="BufferLookup{T}.GetEnabledRefROOptional{T2}(Entity)"/>
        void GetEnabledRefROOptional(Entity entity, T _, out EnabledRefRO<T> result);
    }

    public interface IEnableableBufferLookupRW<T> : IEnableableBufferLookupRO<T>, IBufferLookupRW<T>
        where T : unmanaged, IBufferElementData, IEnableableComponent
    {
        /// <inheritdoc cref="BufferLookup{T}.SetBufferEnabled(Entity, bool)"/>
        void SetBufferEnabled(Entity entity, Bool<T> value);

        /// <inheritdoc cref="BufferLookup{T}.GetEnabledRefRW{T2}(Entity)"/>
        void GetEnabledRefRW(Entity entity, out EnabledRefRW<T> result);

        /// <inheritdoc cref="BufferLookup{T}.GetEnabledRefRW{T2}(Entity)"/>
        void GetEnabledRefRW(Entity entity, T _, out EnabledRefRW<T> result);

        /// <inheritdoc cref="BufferLookup{T}.GetEnabledRefRWOptional{T2}(Entity)"/>
        void GetEnabledRefRWOptional(Entity entity, out EnabledRefRW<T> result);

        /// <inheritdoc cref="BufferLookup{T}.GetEnabledRefRWOptional{T2}(Entity)"/>
        void GetEnabledRefRWOptional(Entity entity, T _, out EnabledRefRW<T> result);
    }

    public interface IEnableableComponentLookupRO<T> : IComponentLookupRO<T>
        where T : unmanaged, IComponentData, IEnableableComponent
    {
        /// <inheritdoc cref="ComponentLookup{T}.IsComponentEnabled(Entity)"/>
        void IsComponentEnabled(Entity entity, out Bool<T> result);

        /// <inheritdoc cref="ComponentLookup{T}.IsComponentEnabled(SystemHandle)"/>
        void IsComponentEnabled(SystemHandle systemHandle, out Bool<T> result);

        /// <inheritdoc cref="ComponentLookup{T}.GetEnabledRefRO{T2}(Entity)"/>
        void GetEnabledRefRO(Entity entity, out EnabledRefRO<T> result);

        /// <inheritdoc cref="ComponentLookup{T}.GetEnabledRefRO{T2}(Entity)"/>
        void GetEnabledRefRO(Entity entity, T _, out EnabledRefRO<T> result);

        /// <inheritdoc cref="ComponentLookup{T}.GetEnabledRefROOptional{T2}(Entity)"/>
        void GetEnabledRefROOptional(Entity entity, out EnabledRefRO<T> result);

        /// <inheritdoc cref="ComponentLookup{T}.GetEnabledRefROOptional{T2}(Entity)"/>
        void GetEnabledRefROOptional(Entity entity, T _, out EnabledRefRO<T> result);
    }

    public interface IEnableableComponentLookupRW<T> : IEnableableComponentLookupRO<T>, IComponentLookupRW<T>
        where T : unmanaged, IComponentData, IEnableableComponent
    {
        /// <inheritdoc cref="ComponentLookup{T}.SetComponentEnabled(Entity, bool)"/>
        void SetComponentEnabled(Entity entity, Bool<T> value);

        /// <inheritdoc cref="ComponentLookup{T}.SetComponentEnabled(SystemHandle, bool)"/>
        void SetComponentEnabled(SystemHandle systemHandle, Bool<T> value);

        /// <inheritdoc cref="ComponentLookup{T}.GetEnabledRefRW{T2}(Entity)"/>
        void GetEnabledRefRW(Entity entity, out EnabledRefRW<T> result);

        /// <inheritdoc cref="ComponentLookup{T}.GetEnabledRefRW{T2}(Entity)"/>
        void GetEnabledRefRW(Entity entity, T _, out EnabledRefRW<T> result);

        /// <inheritdoc cref="ComponentLookup{T}.GetEnabledRefRWOptional{T2}(Entity)"/>
        void GetEnabledRefRWOptional(Entity entity, out EnabledRefRW<T> result);

        /// <inheritdoc cref="ComponentLookup{T}.GetEnabledRefRWOptional{T2}(Entity)"/>
        void GetEnabledRefRWOptional(Entity entity, T _, out EnabledRefRW<T> result);

        /// <inheritdoc cref="SetComponentEnabled(Entity, Bool{T})"/>
        void SetComponentEnabledOptional(Entity entity, Bool<T> value);
    }
}

#endif
