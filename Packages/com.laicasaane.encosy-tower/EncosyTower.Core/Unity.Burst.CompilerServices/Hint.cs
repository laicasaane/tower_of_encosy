#if !UNITY_BURST

namespace Unity.Burst.CompilerServices
{
    public static class Hint
    {
        public static bool Likely(bool condition)
        {
            return condition;
        }

        public static bool Unlikely(bool condition)
        {
            return condition;
        }

        public static void Assume(bool condition)
        {
        }
    }
}

#endif
