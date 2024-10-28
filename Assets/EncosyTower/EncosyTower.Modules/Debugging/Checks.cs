#if !DEBUG && !ENABLE_UNITY_COLLECTIONS_CHECKS && !UNITY_DOTS_DEBUG
#define DISABLE_CHECKS
#endif

namespace EncosyTower.Modules
{
    using System;

    using Debug = UnityEngine.Debug;

    [System.Diagnostics.DebuggerStepThrough]
    public static partial class Checks
    {
#if DISABLE_CHECKS
        [System.Diagnostics.Conditional("__NEVER_DEFINED__")]
#endif
        public static void IsTrue(bool condition)
        {
            Debug.Assert(condition);
        }

#if DISABLE_CHECKS
        [System.Diagnostics.Conditional("__NEVER_DEFINED__")]
#endif
        public static void IsTrue(bool condition, string message)
        {
            Debug.Assert(condition, message);
        }

#if DISABLE_CHECKS
        [System.Diagnostics.Conditional("__NEVER_DEFINED__")]
#endif
        public static void IndexInRange(int index, int length)
        {
            if ((uint)index >= (uint)length)
            {
                throw new IndexOutOfRangeException($"Index {index} is out of range in container of '{length}' Length.");
            }
        }

#if DISABLE_CHECKS
        [System.Diagnostics.Conditional("__NEVER_DEFINED__")]
#endif
        public static void OneIndexInRange(int index, int length)
        {
            if (index < 1 || (uint)index >= (uint)length)
            {
                throw new IndexOutOfRangeException($"Index {index} is out of range in container of '{length}' Length.");
            }
        }
    }
}

#if UNITY_BURST

namespace EncosyTower.Modules
{
    using System.Runtime.CompilerServices;
    using Unity.Burst.CompilerServices;

    public static partial class Checks
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void BurstAssume(bool assumption)
        {
            IsTrue(assumption);
            Hint.Assume(assumption);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void BurstAssume(bool assumption, string message)
        {
            IsTrue(assumption, message);
            Hint.Assume(assumption);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [return: AssumeRange(0L, 2147483647L)]
        public static int BurstAssumePositive(int value)
        {
            return value;
        }
    }
}

#endif
