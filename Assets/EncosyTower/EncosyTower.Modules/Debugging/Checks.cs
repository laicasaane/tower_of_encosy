#if !(UNITY_EDITOR || DEBUG) || DISABLE_ENCOSY_CHECKS
#define __ENCOSY_NO_VALIDATION__
#else
#define __ENCOSY_VALIDATION__
#endif

namespace EncosyTower.Modules
{
    using System;
    using System.Diagnostics;
    using JetBrains.Annotations;

    using Debug = UnityEngine.Debug;

    [DebuggerStepThrough]
    public static partial class Checks
    {
        [AssertionMethod]
        [ContractAnnotation("condition:false=>halt")]
#if __ENCOSY_NO_VALIDATION__
        [System.Diagnostics.Conditional("__NEVER_DEFINED__")]
#endif
        public static void IsTrue(bool condition)
        {
            Debug.Assert(condition);
        }

        [AssertionMethod]
        [ContractAnnotation("condition:false=>halt")]
#if __ENCOSY_NO_VALIDATION__
        [System.Diagnostics.Conditional("__NEVER_DEFINED__")]
#endif
        public static void IsTrue(bool condition, string message)
        {
            Debug.Assert(condition, message);
        }

        [AssertionMethod]
#if __ENCOSY_NO_VALIDATION__
        [System.Diagnostics.Conditional("__NEVER_DEFINED__")]
#endif
        public static void IndexInRange(int index, int length)
        {
            if ((uint)index >= (uint)length)
            {
                throw new IndexOutOfRangeException($"Index {index} is out of range in container of '{length}' Length.");
            }
        }

        [AssertionMethod]
#if __ENCOSY_NO_VALIDATION__
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
        [return: AssumeRange(0L, 2147483647L)]
        public static int BurstAssumePositive(int value)
        {
            return value;
        }
    }
}

#endif
