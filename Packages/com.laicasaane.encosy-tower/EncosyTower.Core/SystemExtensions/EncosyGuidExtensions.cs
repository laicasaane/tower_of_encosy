#if UNITY_COLLECTIONS

namespace EncosyTower.SystemExtensions
{
    using System;
    using Unity.Collections;

    public static partial class EncosyGuidExtensions
    {
        /// <summary>
        /// Converts a <see cref="Guid"/> to its equivalent <see cref="FixedString128Bytes"/> representation.
        /// </summary>
        /// <param name="format">
        /// A read-only span containing the character representing one of the following specifiers
        /// that indicates the exact format to use when interpreting the current GUID instance:
        /// "N", "D", "B", "P", or "X".
        /// </param>
        public static FixedString128Bytes ToFixedString(in this Guid self, ReadOnlySpan<char> format)
        {
            var fs = new FixedString128Bytes();

            unsafe
            {
                Span<char> utf16Chars = stackalloc char[68];

                self.TryFormat(utf16Chars, out var utf16CharsWritten, format);
                fs.TryResize(utf16CharsWritten, NativeArrayOptions.UninitializedMemory);

                fixed (char* utf16Buffer = utf16Chars)
                {
                    Unicode.Utf16ToUtf8(
                          utf16Buffer
                        , utf16CharsWritten
                        , fs.GetUnsafePtr()
                        , out var utf8BytesWritten
                        , FixedString128Bytes.UTF8MaxLengthInBytes
                    );
                }
            }

            return fs;
        }
    }
}

#endif
