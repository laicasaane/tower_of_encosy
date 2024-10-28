using System;

namespace EncosyTower.Modules.TypeWrap.SourceGen
{
    [Flags]
    public enum OperatorKind
    {
        None               = 0,
        UnaryPlus          = 1 << 1,
        UnaryMinus         = 1 << 2,
        Negation           = 1 << 3,
        OnesComplement     = 1 << 4,
        Increment          = 1 << 5,
        Decrement          = 1 << 6,
        True               = 1 << 7,
        False              = 1 << 8,
        Addition           = 1 << 9,
        Substraction       = 1 << 10,
        Multiplication     = 1 << 11,
        Division           = 1 << 12,
        Remainder          = 1 << 13,
        LogicalAnd         = 1 << 14,
        LogicalOr          = 1 << 15,
        LogicalXor         = 1 << 16,
        BitwiseAnd         = 1 << 17,
        BitwiseOr          = 1 << 18,
        BitwiseXor         = 1 << 19,
        LeftShift          = 1 << 20,
        RightShift         = 1 << 21,
        UnsignedRightShift = 1 << 22,
        Equal              = 1 << 23,
        NotEqual           = 1 << 24,
        Greater            = 1 << 25,
        Lesser             = 1 << 26,
        GreaterEqual       = 1 << 27,
        LesserEqual        = 1 << 28,
    }

    public static class OperatorKinds
    {
        private static readonly OperatorKind[] s_all;

        static OperatorKinds()
        {
            s_all = (OperatorKind[])Enum.GetValues(typeof(OperatorKind));

            var all = s_all.AsSpan();
            var excludeFirst = all.Slice(1);
            excludeFirst.CopyTo(all);

            Array.Resize(ref s_all, s_all.Length - 1);
        }

        public static ReadOnlySpan<OperatorKind> All => s_all;
    }
}
