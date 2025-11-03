using EncosyTower.Entities;
using Unity.Entities;

namespace EncosyTower.Tests.Lookups
{
    public struct ComponentA : IComponentData { }

    public struct ComponentB : IComponentData { }

    public struct BufferA : IBufferElementData { }

    public struct BufferB : IBufferElementData { }

    public struct EnableableBufferA : IBufferElementData, IEnableableComponent { }

    public struct EnableableBufferB : IBufferElementData, IEnableableComponent { }

    public struct EnableableComponentA : IComponentData, IEnableableComponent { }

    public struct EnableableComponentB : IComponentData, IEnableableComponent { }

    [Lookup(typeof(BufferA), true)]
    [Lookup(typeof(BufferB), false)]
    public partial struct BufferLookups : IBufferLookups { }

    [Lookup(typeof(EnableableBufferA), true)]
    [Lookup(typeof(EnableableBufferB), false)]
    public partial struct EnableableBufferLookups : IEnableableBufferLookups { }

    [Lookup(typeof(ComponentA), true)]
    [Lookup(typeof(ComponentB), false)]
    public partial struct ComponentLookups : IComponentLookups { }

    [Lookup(typeof(EnableableComponentA), true)]
    [Lookup(typeof(EnableableComponentB), false)]
    public partial struct EnableableComponentLookups : IEnableableComponentLookups { }

    [Lookup(typeof(BufferA), true)]
    [Lookup(typeof(BufferB), false)]
    public partial struct PhysicsBufferLookups : IPhysicsBufferLookups { }

    [Lookup(typeof(ComponentA), true)]
    [Lookup(typeof(ComponentB), false)]
    public partial struct PhysicsComponentLookups : IPhysicsComponentLookups { }

    [Lookup(typeof(EnableableComponentA), true)]
    [Lookup(typeof(EnableableComponentB), false)]
    public partial struct PhysicsEnableableComponentLookups : IPhysicsEnableableComponentLookups { }
}
