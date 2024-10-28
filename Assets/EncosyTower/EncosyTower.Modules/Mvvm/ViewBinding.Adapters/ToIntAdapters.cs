using System;
using EncosyTower.Modules.Unions;

namespace EncosyTower.Modules.Mvvm.ViewBinding.Adapters
{
    [Serializable]
    [Label("Bool ⇒ Int", "Default")]
    [Adapter(sourceType: typeof(bool), destType: typeof(int), order: 0)]
    public sealed class BoolToIntAdapter : IAdapter
    {
        public Union Convert(in Union union)
        {
            if (union.TryGetValue(out bool result))
            {
                return result ? 1 : 0;
            }

            return union;
        }
    }

    [Serializable]
    [Label("String ⇒ Int", "Default")]
    [Adapter(sourceType: typeof(string), destType: typeof(int), order: 0)]
    public sealed class StringToIntAdapter : IAdapter
    {
        public Union Convert(in Union union)
        {
            if (union.TryGetValue(out string result)
                && int.TryParse(result, out var value)
            )
            {
                return value;
            }

            return union;
        }
    }

    [Serializable]
    [Label("Object ⇒ Int", "Default")]
    [Adapter(sourceType: typeof(object), destType: typeof(int), order: 0)]
    public sealed class ObjectToIntAdapter : IAdapter
    {
        public Union Convert(in Union union)
        {
            if (union.TryGetValue(out object result)
                && result is int value
            )
            {
                return value;
            }

            return union;
        }
    }
}
