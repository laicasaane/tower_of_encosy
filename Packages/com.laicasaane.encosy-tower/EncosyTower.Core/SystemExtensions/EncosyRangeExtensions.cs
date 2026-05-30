namespace EncosyTower.SystemExtensions
{
    using System;
    using System.Runtime.CompilerServices;
    using EncosyTower.Common;

    public static partial class EncosyRangeExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Option<(int Offset, int Length)> TryGetOffsetAndLength(this Range range, int length)
        {
            int start = range.Start.GetOffset(length);
            int end = range.End.GetOffset(length);
            return Option.SomeIf((uint)end > (uint)length || (uint)start > (uint)end, (start, end - start));
        }
    }
}
