using EncosyTower.Modules.Entities;
using Unity.Entities;

namespace EncosyTower.Modules.Tests.Lookups
{
    public struct ComponentA : IComponentData { }

    public struct ComponentB : IComponentData { }

    public struct BufferA : IBufferElementData { }

    public struct BufferB : IBufferElementData { }

    public struct EnableableComponentA : IComponentData, IEnableableComponent { }

    public struct EnableableComponentB : IComponentData, IEnableableComponent { }

    [Lookup(typeof(ComponentA), true)]
    [Lookup(typeof(ComponentB), false)]
    public partial struct ComponentLookups : IComponentLookups { }

    [Lookup(typeof(BufferA), true)]
    [Lookup(typeof(BufferB), false)]
    public partial struct BufferLookups : IBufferLookups { }

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
