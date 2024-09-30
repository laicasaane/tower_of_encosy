using System;
using Module.Core.Mvvm.ViewBinding;
using Module.Core.Unions;

namespace Module.Core.Extended.Mvvm.ViewBinding.Adapters.Unity
{
    [Serializable]
    [Label("Double ⇒ mm:ss", "Default")]
    [Adapter(sourceType: typeof(double), destType: typeof(string), order: 0)]
    public class DoubleToMinuteSecondStringAdapter : IAdapter
    {
        public Union Convert(in Union union)
        {
            if (union.TryGetValue(out double totalSeconds))
            {
                return TimeSpan.FromSeconds(totalSeconds).ToString(@"mm\:ss");
            }

            return union;
        }
    }
}
