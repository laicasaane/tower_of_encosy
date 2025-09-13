using System;
using EncosyTower.Annotations;
using EncosyTower.Variants;

namespace EncosyTower.Mvvm.ViewBinding.Adapters
{
    [Serializable]
    [Label("Bool ⇒ Int", "Default")]
    [Adapter(sourceType: typeof(bool), destType: typeof(int), order: 0)]
    public sealed class BoolToIntAdapter : IAdapter
    {
        public Variant Convert(in Variant variant)
        {
            if (variant.TryGetValue(out bool result))
            {
                return result ? 1 : 0;
            }

            return variant;
        }
    }

    [Serializable]
    [Label("String ⇒ Int", "Default")]
    [Adapter(sourceType: typeof(string), destType: typeof(int), order: 0)]
    public sealed class StringToIntAdapter : IAdapter
    {
        public Variant Convert(in Variant variant)
        {
            if (variant.TryGetValue(out string result)
                && int.TryParse(result, out var value)
            )
            {
                return value;
            }

            return variant;
        }
    }

    [Serializable]
    [Label("Object ⇒ Int", "Default")]
    [Adapter(sourceType: typeof(object), destType: typeof(int), order: 0)]
    public sealed class ObjectToIntAdapter : IAdapter
    {
        public Variant Convert(in Variant variant)
        {
            if (variant.TryGetValue(out object result)
                && result is int value
            )
            {
                return value;
            }

            return variant;
        }
    }
}
