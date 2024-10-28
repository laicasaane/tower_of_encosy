using System;
using Module.Core;
using Module.Core.Mvvm.ViewBinding;
using Module.Core.Unions;

namespace Tests.Module.Mvvm
{
    [Serializable]
    [Label("Scroll Position ⇒ Text", "Default")]
    [Adapter(sourceType: typeof(float), destType: typeof(string), order: 0)]
    public sealed class ScrollPositionTextAdapter : IAdapter
    {
        public Union Convert(in Union union)
        {
            if (union.TryGetValue(out float result))
            {
                return $"Scroll Position: {result:0.00}";
            }

            return union;
        }
    }
}
