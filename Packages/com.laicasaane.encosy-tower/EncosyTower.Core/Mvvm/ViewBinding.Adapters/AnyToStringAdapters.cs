using System;
using EncosyTower.Annotations;
using EncosyTower.Variants;

namespace EncosyTower.Mvvm.ViewBinding.Adapters
{
    [Serializable]
    [Label("String ⇒ String", "Default")]
    [Adapter(sourceType: typeof(string), destType: typeof(string), order: 0)]
    public sealed class StringToStringAdapter : IAdapter
    {
        public Variant Convert(in Variant variant)
        {
            if (variant.TryGetValue(out string result))
            {
                return result;
            }

            return variant;
        }
    }

    [Serializable]
    [Label("Object ⇒ String", "Default")]
    [Adapter(sourceType: typeof(object), destType: typeof(string), order: 0)]
    public sealed class ObjectToStringAdapter : IAdapter
    {
        public Variant Convert(in Variant variant)
        {
            if (variant.TryGetValue(out object result))
            {
                return result.ToString();
            }

            return variant;
        }
    }
}
