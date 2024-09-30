using System;
using Module.Core.Unions;

namespace Module.Core.Mvvm.ViewBinding.Adapters
{
    [Serializable]
    [Label("String ⇒ String", "Default")]
    [Adapter(sourceType: typeof(string), destType: typeof(string), order: 0)]
    public sealed class StringToStringAdapter : IAdapter
    {
        public Union Convert(in Union union)
        {
            if (union.TryGetValue(out string result))
            {
                return result;
            }

            return union;
        }
    }

    [Serializable]
    [Label("Object ⇒ String", "Default")]
    [Adapter(sourceType: typeof(object), destType: typeof(string), order: 0)]
    public sealed class ObjectToStringAdapter : IAdapter
    {
        public Union Convert(in Union union)
        {
            if (union.TryGetValue(out object result))
            {
                return result.ToString();
            }

            return union;
        }
    }
}
