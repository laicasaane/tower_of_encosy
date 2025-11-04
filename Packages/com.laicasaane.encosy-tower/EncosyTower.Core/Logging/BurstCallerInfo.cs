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
    /// <item><see cref="CallerFilePathAttribute"/> (up to 125 UTF8 characters)</item>
    /// </list>
    /// </summary>
    public readonly partial struct BurstCallerInfoSmall
    {
        private readonly FixedString128Bytes _filePath;
        private readonly FixedString128Bytes _memberName;
        private readonly int _lineNumber;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private BurstCallerInfoSmall(
              int lineNumber
            , in FixedString128Bytes memberName
            , in FixedString128Bytes filePath
        )
        {
            _lineNumber = lineNumber;
            _memberName = memberName;
            _filePath = filePath;
        }

        /// <summary>
        /// The value retrieved from <see cref="CallerFilePathAttribute"/>.
        /// Maximum length is 125 UTF8 characters.
        /// </summary>
        public FixedString128Bytes FilePath
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _filePath;
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
        public static BurstCallerInfoSmall GetFull(
              [CallerLineNumber] int lineNumber = 0
            , [CallerMemberName] string memberName = ""
            , [CallerFilePath] string filePath = ""
        )
        {
            BurstCallerInfoAPI.TrimFilePath(filePath, out var filePath1);
            return new BurstCallerInfoSmall(lineNumber, memberName, filePath1);
        }

        /// <summary>
        /// Returns an info with line number, and member name.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BurstCallerInfoSmall GetSlim(
              [CallerLineNumber] int lineNumber = 0
            , [CallerMemberName] string memberName = ""
        )
        {
            return new BurstCallerInfoSmall(lineNumber, memberName, default);
        }

        /// <summary>
        /// Returns an info with only line number.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BurstCallerInfoSmall GetLine([CallerLineNumber] int lineNumber = 0)
        {
            return new BurstCallerInfoSmall(lineNumber, default, default);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override string ToString()
            => ToFixedString().ToString();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public FixedString512Bytes ToFixedString()
        {
            BurstCallerInfoAPI.ToFixedString(
                  LineNumber
                , MemberName
                , FilePath
                , out FixedString512Bytes result
            );

            return result;
        }
    }

    /// <summary>
    /// Represents a Burst-compatible set of information retrieved from:
    /// <list type="bullet">
    /// <item><see cref="CallerLineNumberAttribute"/></item>
    /// <item><see cref="CallerMemberNameAttribute"/> (up to 125 UTF8 characters)</item>
    /// <item><see cref="CallerFilePathAttribute"/> (up to 250 UTF8 characters)</item>
    /// </list>
    /// </summary>
    public readonly partial struct BurstCallerInfoMedium
    {
        private readonly FixedString128Bytes _filePath1;
        private readonly FixedString128Bytes _filePath2;
        private readonly FixedString128Bytes _memberName;
        private readonly int _lineNumber;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private BurstCallerInfoMedium(
              int lineNumber
            , in FixedString128Bytes memberName
            , in FixedString128Bytes filePath1
            , in FixedString128Bytes filePath2
        )
        {
            _lineNumber = lineNumber;
            _memberName = memberName;
            _filePath1 = filePath1;
            _filePath2 = filePath2;
        }

        /// <summary>
        /// The value retrieved from <see cref="CallerFilePathAttribute"/>.
        /// Maximum length is 250 UTF8 characters.
        /// </summary>
        public FixedString512Bytes FilePath
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                var fs = new FixedString512Bytes();
                fs.Append(_filePath1);
                fs.Append(_filePath2);
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
        public static BurstCallerInfoMedium GetFull(
              [CallerLineNumber] int lineNumber = 0
            , [CallerMemberName] string memberName = ""
            , [CallerFilePath] string filePath = ""
        )
        {
            BurstCallerInfoAPI.TrimFilePath(filePath, out var filePath1, out var filePath2);
            return new BurstCallerInfoMedium(lineNumber, memberName, filePath1, filePath2);
        }

        /// <summary>
        /// Returns an info with line number, and member name.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BurstCallerInfoMedium GetSlim(
              [CallerLineNumber] int lineNumber = 0
            , [CallerMemberName] string memberName = ""
        )
        {
            return new BurstCallerInfoMedium(lineNumber, memberName, default, default);
        }

        /// <summary>
        /// Returns an info with only line number.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BurstCallerInfoMedium GetLine([CallerLineNumber] int lineNumber = 0)
        {
            return new BurstCallerInfoMedium(lineNumber, default, default, default);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override string ToString()
            => ToFixedString().ToString();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public FixedString512Bytes ToFixedString()
        {
            BurstCallerInfoAPI.ToFixedString(
                  LineNumber
                , MemberName
                , FilePath
                , out FixedString512Bytes result
            );

            return result;
        }
    }

    /// <summary>
    /// Represents a Burst-compatible set of information retrieved from:
    /// <list type="bullet">
    /// <item><see cref="CallerLineNumberAttribute"/></item>
    /// <item><see cref="CallerMemberNameAttribute"/> (up to 125 UTF8 characters)</item>
    /// <item><see cref="CallerFilePathAttribute"/> (up to 375 UTF8 characters)</item>
    /// </list>
    /// </summary>
    public readonly partial struct BurstCallerInfoLarge
    {
        private readonly FixedString128Bytes _filePath1;
        private readonly FixedString128Bytes _filePath2;
        private readonly FixedString128Bytes _filePath3;
        private readonly FixedString128Bytes _memberName;
        private readonly int _lineNumber;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private BurstCallerInfoLarge(
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
        public static BurstCallerInfoLarge GetFull(
              [CallerLineNumber] int lineNumber = 0
            , [CallerMemberName] string memberName = ""
            , [CallerFilePath] string filePath = ""
        )
        {
            BurstCallerInfoAPI.TrimFilePath(filePath, out var filePath1, out var filePath2, out var filePath3);
            return new BurstCallerInfoLarge(lineNumber, memberName, filePath1, filePath2, filePath3);
        }

        /// <summary>
        /// Returns an info with line number, and member name.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BurstCallerInfoLarge GetSlim(
              [CallerLineNumber] int lineNumber = 0
            , [CallerMemberName] string memberName = ""
        )
        {
            return new BurstCallerInfoLarge(lineNumber, memberName, default, default, default);
        }

        /// <summary>
        /// Returns an info with only line number.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BurstCallerInfoLarge GetLine([CallerLineNumber] int lineNumber = 0)
        {
            return new BurstCallerInfoLarge(lineNumber, default, default, default, default);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override string ToString()
            => ToFixedString().ToString();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public FixedString4096Bytes ToFixedString()
        {
            BurstCallerInfoAPI.ToFixedString(
                  LineNumber
                , MemberName
                , FilePath
                , out FixedString4096Bytes result
            );

            return result;
        }
    }

    internal static class BurstCallerInfoAPI
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void TrimFilePath(
              in FixedString4096Bytes filePath
            , out FixedString128Bytes result
        )
        {
            Split(filePath.AsReadOnlySpan(), out _, out result);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void TrimFilePath(
              in FixedString4096Bytes filePath
            , out FixedString128Bytes result1
            , out FixedString128Bytes result2
        )
        {
            Split(filePath.AsReadOnlySpan(), out var filePathSpan, out result2);
            Split(filePathSpan, out _, out result1);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void TrimFilePath(
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

        public static void ToFixedString<TMemberName, TFilePath, TResult>(
              int lineNumber
            , in TMemberName memberName
            , in TFilePath filePath
            , out TResult result
        )
            where TMemberName : unmanaged, INativeList<byte>, IUTF8Bytes
            where TFilePath : unmanaged, INativeList<byte>, IUTF8Bytes
            where TResult : unmanaged, INativeList<byte>, IUTF8Bytes
        {
            result = new TResult();
            result.Append(lineNumber);

            if (memberName.Length > 0)
            {
                AppendSeparator(ref result);
                result.Append(memberName);
            }

            if (filePath.Length > 0)
            {
                AppendSeparator(ref result);
                result.Append(filePath);
            }

            return;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            static void AppendSeparator(ref TResult fs)
            {
                fs.Append(' ');
                fs.Append(':');
                fs.Append(':');
                fs.Append(' ');
            }
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
