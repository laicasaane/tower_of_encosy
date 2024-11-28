using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using EncosyTower.Modules.Unions;
using NUnit.Framework;
using Unity.PerformanceTesting;
using UnityEngine.Profiling;

namespace EncosyTower.Tests.Unions
{
    public class UnionTests
    {
        [Test, Performance]
        public void Union_Performance()
        {
            var i = 0;

            Measure.Method(() => {
                Profiler.BeginSample("Union");
                {
                    var u = new Union(i);
                    Process(u);
                    i++;
                }
                Profiler.EndSample();
            })
                .WarmupCount(1)
                .IterationsPerMeasurement(5000)
                .MeasurementCount(20)
                .Run();
        }

        [Test, Performance]
        public void UnionBig_Performance()
        {
            var i = 0;

            Measure.Method(() => {
                Profiler.BeginSample("UnionBig");
                {
                    var u = new UnionBig(i);
                    Process(u);
                    i++;
                }
                Profiler.EndSample();
            })
                .WarmupCount(1)
                .IterationsPerMeasurement(5000)
                .MeasurementCount(20)
                .Run();
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void Process(in Union v)
        {
            _ = v.Int;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void Process(in UnionBig v)
        {
            _ = v.intValue;
        }

        [StructLayout(LayoutKind.Explicit, Size = 9996)]
        private struct Big { }

        [StructLayout(LayoutKind.Explicit)]
        private struct UnionBig
        {
            [FieldOffset(0)] public int intValue;
            [FieldOffset(0)] public Big bigValue;

            public UnionBig(int intValue)
            {
                this.intValue = intValue;
            }
        }
    }
}
