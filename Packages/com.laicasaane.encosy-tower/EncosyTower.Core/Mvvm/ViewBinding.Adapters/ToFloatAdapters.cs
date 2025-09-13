using System;
using EncosyTower.Annotations;
using EncosyTower.Variants;

namespace EncosyTower.Mvvm.ViewBinding.Adapters
{
    [Serializable]
    [Label("Bool ⇒ Float", "Default")]
    [Adapter(sourceType: typeof(bool), destType: typeof(float), order: 0)]
    public sealed class BoolToFloatAdapter : IAdapter
    {
        public Variant Convert(in Variant variant)
        {
            if (variant.TryGetValue(out bool result))
            {
                return result ? 1f : 0f;
            }

            return variant;
        }
    }

    [Serializable]
    [Label("String ⇒ Float", "Default")]
    [Adapter(sourceType: typeof(string), destType: typeof(float), order: 0)]
    public sealed class StringToFloatAdapter : IAdapter
    {
        public Variant Convert(in Variant variant)
        {
            if (variant.TryGetValue(out string result)
                && float.TryParse(result, out var value)
            )
            {
                return value;
            }

            return variant;
        }
    }

    [Serializable]
    [Label("Object ⇒ Float", "Default")]
    [Adapter(sourceType: typeof(object), destType: typeof(float), order: 0)]
    public sealed class ObjectToFloatAdapter : IAdapter
    {
        public Variant Convert(in Variant variant)
        {
            if (variant.TryGetValue(out object result)
                && result is float value
            )
            {
                return value;
            }

            return variant;
        }
    }
}
