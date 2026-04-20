using UE = Unity.Entities;

namespace Samples.Entities.TypeHandles
{
    public class Program
    {
        public static void Main()
        {
            UE.SystemState state = default;
            UE.SystemBase system = default;
            UE.BufferTypeHandle<BufferA> bufferTypeHandle = default;
            UE.ComponentTypeHandle<ComponentA> componentTypeHandle = default;
            UE.SharedComponentTypeHandle<SharedComponentA> sharedComponentTypeHandle = default;
            state.GetBufferTypeHandle<BufferA>();
            state.GetComponentTypeHandle<ComponentA>();
            state.GetSharedComponentTypeHandle<SharedComponentA>();

            system.GetBufferTypeHandle<BufferA>();
            system.GetComponentTypeHandle<ComponentA>();
            system.GetSharedComponentTypeHandle<SharedComponentA>();
        }
    }
}
