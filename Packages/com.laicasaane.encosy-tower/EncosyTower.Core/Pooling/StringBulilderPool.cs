using System.Runtime.CompilerServices;
using System.Text;
using UnityEngine.Pool;

namespace EncosyTower.Pooling
{
    public static partial class StringBuilderPool
    {
        private static readonly ObjectPool<StringBuilder> s_pool = new(
              createFunc: static () => new StringBuilder(1024)
            , actionOnGet: static x => x.Clear()
        );

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static StringBuilder Rent()
        {
            return s_pool.Get();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static PooledObject<StringBuilder> Rent(out StringBuilder result)
        {
            return new PooledObject<StringBuilder>(result = Rent(), s_pool);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Return(StringBuilder sb)
        {
            s_pool.Release(sb);
        }
    }
}
