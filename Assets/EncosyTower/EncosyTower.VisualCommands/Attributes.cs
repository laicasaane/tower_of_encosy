using System;

namespace EncosyTower.VisualCommands
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public sealed class VisualOrderAttribute : Attribute
    {
        public int Order { get; }

        public VisualOrderAttribute(int order)
        {
            Order = order;
        }
    }

    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public sealed class VisualIgnoredAttribute : Attribute { }
}
