using System;
using EncosyTower.Modules;
using EncosyTower.Modules.Mvvm.ViewBinding;
using EncosyTower.Modules.Unions;

namespace EncosyTower.Samples.Mvvm
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
