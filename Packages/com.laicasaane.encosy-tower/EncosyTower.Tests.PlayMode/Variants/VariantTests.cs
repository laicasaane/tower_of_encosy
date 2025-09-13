using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using EncosyTower.Variants;
using NUnit.Framework;
using Unity.PerformanceTesting;
using UnityEngine.Profiling;

namespace EncosyTower.Tests.Variants
{
    public class VariantTests
    {
        [Test, Performance]
        public void Variant_Performance()
        {
            var i = 0;

            Measure.Method(() => {
                Profiler.BeginSample("Variant");
                {
                    var u = new Variant(i);
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
        public void VariantBig_Performance()
        {
            var i = 0;

            Measure.Method(() => {
                Profiler.BeginSample("VariantBig");
                {
                    var u = new VariantBig(i);
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
        private static void Process(in Variant v)
        {
            _ = v.Int;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void Process(in VariantBig v)
        {
            _ = v.Int;
        }

        [StructLayout(LayoutKind.Explicit, Size = 9996)]
        private struct Big { }

        [StructLayout(LayoutKind.Explicit)]
        private readonly struct VariantBig
        {
            [FieldOffset(0)] public readonly VariantBase Base;
            [FieldOffset(0)] public readonly Big Big;
            [FieldOffset(0)] public readonly bool Bool;
            [FieldOffset(0)] public readonly byte Byte;
            [FieldOffset(0)] public readonly sbyte SByte;
            [FieldOffset(0)] public readonly char Char;
            [FieldOffset(0)] public readonly double Double;
            [FieldOffset(0)] public readonly float Float;
            [FieldOffset(0)] public readonly int Int;
            [FieldOffset(0)] public readonly uint UInt;
            [FieldOffset(0)] public readonly long Long;
            [FieldOffset(0)] public readonly ulong ULong;
            [FieldOffset(0)] public readonly short Short;
            [FieldOffset(0)] public readonly ushort UShort;

            public VariantBig(int intValue) : this()
            {
                Int = intValue;
            }
        }
    }
}
