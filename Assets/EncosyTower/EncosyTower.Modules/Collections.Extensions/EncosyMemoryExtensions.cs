using System;
using System.Runtime.CompilerServices;

namespace EncosyTower.Modules
{
    public static class EncosyMemoryExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ObjectMemoryGetter BeginGet(this Memory<object> buffer)
            => new(buffer, 0);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ObjectSpanGetter BeginGetSpan(this Memory<object> buffer)
            => new(buffer.Span, 0);
    }
}