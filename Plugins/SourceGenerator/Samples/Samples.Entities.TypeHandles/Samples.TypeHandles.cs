using EncosyTower.Entities;
using Unity.Entities;

namespace Samples.Entities.TypeHandles
{
    public struct ComponentA : IComponentData { }
    public struct ComponentB : IComponentData { }
    public struct BufferA : IBufferElementData { }
    public struct BufferB : IBufferElementData { }
    public struct EnableableBufferA : IBufferElementData, IEnableableComponent { }
    public struct EnableableBufferB : IBufferElementData, IEnableableComponent { }
    public struct EnableableComponentA : IComponentData, IEnableableComponent { }
    public struct EnableableComponentB : IComponentData, IEnableableComponent { }
    public struct SharedComponentA : ISharedComponentData { }
    public struct SharedComponentB : ISharedComponentData { }

    [TypeHandle(typeof(ComponentA), true)]
    [TypeHandle(typeof(ComponentB), false)]
    [TypeHandle(typeof(BufferA), true)]
    [TypeHandle(typeof(BufferB), false)]
    [TypeHandle(typeof(EnableableBufferA), true)]
    [TypeHandle(typeof(EnableableBufferB), false)]
    [TypeHandle(typeof(EnableableComponentA), true)]
    [TypeHandle(typeof(EnableableComponentB), false)]
    [TypeHandle(typeof(SharedComponentA), false)]
    [TypeHandle(typeof(SharedComponentB), false)]
    public partial struct TypeHandles { }
}
