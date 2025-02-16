using System;
using EncosyTower.Annotations;
using EncosyTower.Unions;

namespace EncosyTower.Mvvm.ViewBinding.Adapters.Unity
{
    [Serializable]
    [Label("Double ⇒ mm:ss", "Default")]
    [Adapter(sourceType: typeof(double), destType: typeof(string), order: 0)]
    public class DoubleToMinuteSecondStringAdapter : IAdapter
    {
        public Union Convert(in Union union)
        {
            return union.TryGetValue(out double totalSeconds)
                ? TimeSpan.FromSeconds(totalSeconds).ToString(@"mm\:ss")
                : union;
        }
    }
}
