using System;

namespace EncosyTower.Data
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, Inherited = false)]
    public sealed class DataMutableAttribute : Attribute
    {
        public DataMutableOptions Options { get; }

        public DataMutableAttribute(DataMutableOptions options = DataMutableOptions.Default)
        {
            Options = options;
        }
    }

    [Flags]
    public enum DataMutableOptions
    {
        Default = 0,
        WithoutPropertySetters = 1 << 0,
        WithReadOnlyView = 1 << 1,
    }
}
