using System;

namespace EncosyTower.Entities
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, Inherited = false, AllowMultiple = false)]
    public sealed class ISystemAttribute : Attribute
    {
    }
}
