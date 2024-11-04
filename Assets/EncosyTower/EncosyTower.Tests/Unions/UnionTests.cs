using EncosyTower.Modules.Unions;
using NUnit.Framework;
using Unity.PerformanceTesting;

namespace EncosyTower.Tests
{
    public class UnionTests
    {
        [Test, Performance]
        public void Union_Performance()
        {
            int i = 0;

            Measure.Method(() => {
                var u = new Union(i);
                Union_Int(u);
                i++;
            })
                .WarmupCount(1)
                .IterationsPerMeasurement(5000)
                .MeasurementCount(20)
                .Run();
        }

        private static void Union_Int(in Union union)
        {
            _ = union.Int;
        }
    }
}
