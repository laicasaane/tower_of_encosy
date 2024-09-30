using System;
using System.Runtime.CompilerServices;

namespace Module.Core
{
    public static class CoreMemoryExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ObjectMemoryGetter BeginGet(this Memory<object> buffer)
            => new(buffer, 0);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ObjectSpanGetter BeginGetSpan(this Memory<object> buffer)
            => new(buffer.Span, 0);
    }
}