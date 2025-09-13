using System;
using EncosyTower.Annotations;
using EncosyTower.Common;
using EncosyTower.Variants;

namespace EncosyTower.Mvvm.ViewBinding.Adapters
{
    [Serializable]
    [Label("Bool ⇒ Bool", "Default")]
    [Adapter(sourceType: typeof(bool), destType: typeof(bool), order: 0)]
    public sealed class BoolToBoolAdapter : IAdapter
    {
        public Variant Convert(in Variant variant)
        {
            if (variant.TryGetValue(out bool result))
            {
                return result;
            }

            return variant;
        }
    }

    [Serializable]
    [Label("String ⇒ Bool", "Default")]
    [Adapter(sourceType: typeof(string), destType: typeof(bool), order: 0)]
    public sealed class StringToBoolAdapter : IAdapter
    {
        public Variant Convert(in Variant variant)
        {
            if (variant.TryGetValue(out string result))
            {
                if (bool.TryParse(result, out var value))
                {
                    return value;
                }

                return result.IsNotEmpty();
            }

            return variant;
        }
    }

    [Serializable]
    [Label("Object ⇒ Bool", "Default")]
    [Adapter(sourceType: typeof(object), destType: typeof(bool), order: 0)]
    public sealed class ObjectToBoolAdapter : IAdapter
    {
        public Variant Convert(in Variant variant)
        {
            if (variant.TryGetValue(out object result))
            {
                return result != null;
            }

            return variant;
        }
    }

    [Serializable]
    [Label("Bool ⇒ ! Bool", "Default")]
    [Adapter(sourceType: typeof(bool), destType: typeof(bool), order: 1)]
    public sealed class BoolToNotBoolAdapter : IAdapter
    {
        public Variant Convert(in Variant variant)
        {
            if (variant.TryGetValue(out bool result))
            {
                return !result;
            }

            return variant;
        }
    }

    [Serializable]
    [Label("String ⇒ ! Bool", "Default")]
    [Adapter(sourceType: typeof(string), destType: typeof(bool), order: 1)]
    public sealed class StringToNotBoolAdapter : IAdapter
    {
        public Variant Convert(in Variant variant)
        {
            if (variant.TryGetValue(out string result))
            {
                if (bool.TryParse(result, out var value))
                {
                    return !value;
                }

                return result.IsNotEmpty();
            }

            return variant;
        }
    }

    [Serializable]
    [Label("Object ⇒ ! Bool", "Default")]
    [Adapter(sourceType: typeof(object), destType: typeof(bool), order: 1)]
    public sealed class ObjectToNotBoolAdapter : IAdapter
    {
        public Variant Convert(in Variant variant)
        {
            if (variant.TryGetValue(out object result))
            {
                return result == null;
            }

            return variant;
        }
    }
}
