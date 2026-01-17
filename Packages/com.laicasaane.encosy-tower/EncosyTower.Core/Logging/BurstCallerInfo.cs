#if UNITY_BURST && UNITY_COLLECTIONS

using System;
using System.Runtime.CompilerServices;
using EncosyTower.Collections;
using Unity.Collections;
using Unity.Mathematics;

namespace EncosyTower.Logging
{
    /// <summary>
    /// Represents a Burst-compatible set of information retrieved from:
    /// <list type="bullet">
    /// <item><see cref="CallerLineNumberAttribute"/></item>
    /// <item><see cref="CallerMemberNameAttribute"/> (up to 125 UTF8 characters)</item>
    /// <item><see cref="CallerFilePathAttribute"/> (up to 375 UTF8 characters)</item>
    /// </list>
    /// </summary>
    public readonly partial struct BurstCallerInfo
    {
        private readonly FixedString128Bytes _filePath1;
        private readonly FixedString128Bytes _filePath2;
        private readonly FixedString128Bytes _filePath3;
        private readonly FixedString128Bytes _memberName;
        private readonly int _lineNumber;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private BurstCallerInfo(
              int lineNumber
            , in FixedString128Bytes memberName
            , in FixedString128Bytes filePath1
            , in FixedString128Bytes filePath2
            , in FixedString128Bytes filePath3
        )
        {
            _lineNumber = lineNumber;
            _memberName = memberName;
            _filePath1 = filePath1;
            _filePath2 = filePath2;
            _filePath3 = filePath3;
        }

        /// <summary>
        /// The value retrieved from <see cref="CallerFilePathAttribute"/>.
        /// Maximum length is 375 UTF8 characters.
        /// </summary>
        public FixedString512Bytes FilePath
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                var fs = new FixedString512Bytes();
                fs.Append(_filePath1);
                fs.Append(_filePath2);
                fs.Append(_filePath3);
                return fs;
            }
        }

        /// <summary>
        /// The value retrieved from <see cref="CallerMemberNameAttribute"/>.
        /// Maximum length is 125 UTF8 characters.
        /// </summary>
        public FixedString128Bytes MemberName
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _memberName;
        }

        /// <summary>
        /// The value retrieved from <see cref="CallerLineNumberAttribute"/>.
        /// </summary>
        public int LineNumber
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _lineNumber;
        }

        /// <summary>
        /// Returns an info with line number, member name, and file path.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BurstCallerInfo GetFull(
              [CallerLineNumber] int lineNumber = 0
            , [CallerMemberName] string memberName = ""
            , [CallerFilePath] string filePath = ""
        )
        {
            TrimFilePath(filePath, out var filePath1, out var filePath2, out var filePath3);
            return new BurstCallerInfo(lineNumber, memberName, filePath1, filePath2, filePath3);
        }

        /// <summary>
        /// Returns an info with line number, and member name.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BurstCallerInfo GetSlim(
              [CallerLineNumber] int lineNumber = 0
            , [CallerMemberName] string memberName = ""
        )
        {
            return new BurstCallerInfo(lineNumber, memberName, default, default, default);
        }

        /// <summary>
        /// Returns an info with only line number.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BurstCallerInfo GetLine([CallerLineNumber] int lineNumber = 0)
        {
            return new BurstCallerInfo(lineNumber, default, default, default, default);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override string ToString()
            => ToFixedString().ToString();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public FixedString4096Bytes ToFixedString()
        {
            var memberName = MemberName;
            var filePath = FilePath;
            var lineNumber = LineNumber;
            var fs = new FixedString4096Bytes();

            if (memberName.Length > 0)
            {
                fs.Append(memberName);
            }

            fs.Append(' ');
            fs.Append('(');
            fs.Append('a');
            fs.Append('t');
            fs.Append(' ');

            if (filePath.Length > 0)
            {
                fs.Append(filePath);
            }

            fs.Append(':');
            fs.Append(lineNumber);
            fs.Append(')');

            return fs;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void TrimFilePath(
              in FixedString4096Bytes filePath
            , out FixedString128Bytes result1
            , out FixedString128Bytes result2
            , out FixedString128Bytes result3
        )
        {
            Split(filePath.AsReadOnlySpan(), out var filePathSpan2, out result3);
            Split(filePathSpan2, out var filePathSpan1, out result2);
            Split(filePathSpan1, out _, out result1);
        }

        private static void Split(
              ReadOnlySpan<byte> source
            , out ReadOnlySpan<byte> prefix
            , out FixedString128Bytes suffix
        )
        {
            suffix = new FixedString128Bytes();

            var sourceLength = source.Length;
            var startIndex = math.max(sourceLength - suffix.Capacity, 0);
            var length = math.min(sourceLength, suffix.Capacity);
            var chars = source.Slice(startIndex, length);

            prefix = source[..startIndex];
            suffix.Append(chars);
        }
    }
}

#endif
