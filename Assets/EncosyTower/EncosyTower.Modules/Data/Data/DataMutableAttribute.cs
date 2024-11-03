using System;

namespace EncosyTower.Modules.Data
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false, Inherited = false)]
    public sealed class DataMutableAttribute : Attribute
    {
        public bool WithoutPropertySetter { get; }

        public DataMutableAttribute() : this(false)
        {
        }

        public DataMutableAttribute(bool withoutPropertySetter)
        {
            this.WithoutPropertySetter = withoutPropertySetter;
        }
    }
}
