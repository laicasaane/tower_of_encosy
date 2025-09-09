using System;
using EncosyTower.Annotations;
using EncosyTower.Mvvm.ViewBinding;
using EncosyTower.Unions;

namespace EncosyTower.Samples.Mvvm
{
    [Serializable]
    [Label("Scroll Position ⇒ Text", "Default")]
    [Adapter(sourceType: typeof(float), destType: typeof(string), order: 0)]
    public sealed class ScrollPositionTextAdapter : IAdapter
    {
        public Union Convert(in Union union)
        {
            return union.TryGetValue(out float result)
                ? $"Scroll Position: {result:0.00}"
                : union;
        }
    }
}
