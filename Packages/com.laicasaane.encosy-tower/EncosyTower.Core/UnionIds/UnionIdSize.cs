namespace EncosyTower.UnionIds
{
    /// <summary>
    /// Specifies the backing storage size in bytes of a struct decorated with <c>[UnionId]</c>.
    /// The generator reads this value from the <c>Size</c> named argument of the attribute
    /// and uses it to determine the struct's unmanaged memory layout.
    /// </summary>
    public enum UnionIdSize : byte
    {
        /// <summary>
        /// No explicit size is set. The generator treats this as a fallback and resolves it
        /// to 4 bytes at runtime, equivalent to <see cref="UInt"/>.
        /// </summary>
        Default = 0,

        /// <summary>
        /// 2-byte backing storage (16-bit unsigned integer).
        /// </summary>
        /// <remarks>
        /// Equivalent to an <see cref="ushort"/>.
        /// </remarks>
        UShort = 2,

        /// <summary>
        /// 4-byte backing storage (32-bit unsigned integer).
        /// This is also the effective size when <see cref="Default"/> is used.
        /// </summary>
        /// <remarks>
        /// Equivalent to an <see cref="uint"/>.
        /// </remarks>
        UInt = 4,

        /// <summary>
        /// 8-byte backing storage (64-bit unsigned integer).
        /// </summary>
        /// <remarks>
        /// Equivalent to an <see cref="ulong"/>.
        /// </remarks>
        ULong = 8,

        /// <summary>
        /// 12-byte backing storage (three 32-bit unsigned integers).
        /// </summary>
        /// <remarks>
        /// Equivalent to three <see cref="uint"/>.
        /// </remarks>
        UInt3 = UInt * 3,

        /// <summary>
        /// 16-byte backing storage (two 64-bit unsigned integers).
        /// </summary>
        /// <remarks>
        /// Equivalent to two <see cref="ulong"/> or four <see cref="uint"/>.
        /// </remarks>
        ULong2 = ULong * 2,
    }
}
