#if UNITY_BURST && UNITY_COLLECTIONS

using System.Runtime.CompilerServices;
using Unity.Collections;

namespace EncosyTower.Logging
{
    public readonly partial struct BurstCallerInfo
    {
        public readonly FixedString128Bytes FilePath;
        public readonly FixedString64Bytes MemberName;
        public readonly int LineNumber;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public BurstCallerInfo(
              int lineNumber
            , in FixedString64Bytes memberName
            , in FixedString128Bytes filePath
        )
        {
            MemberName = memberName;
            FilePath = filePath;
            LineNumber = lineNumber;
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
            return new BurstCallerInfo(lineNumber, memberName, filePath);
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
            return new BurstCallerInfo(lineNumber, memberName, string.Empty);
        }

        /// <summary>
        /// Returns an info with only line number.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BurstCallerInfo GetLine([CallerLineNumber] int lineNumber = 0)
        {
            return new BurstCallerInfo(lineNumber, string.Empty, string.Empty);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override string ToString()
            => $"{LineNumber} :: {MemberName} :: {FilePath}";

        public FixedString4096Bytes ToFixedString()
        {
            var fs = new FixedString4096Bytes();

            fs.Append(LineNumber);
            AppendSeparator(ref fs);
            fs.Append(MemberName);
            AppendSeparator(ref fs);
            fs.Append(FilePath);

            return fs;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            static void AppendSeparator(ref FixedString4096Bytes fs)
            {
                fs.Append(' ');
                fs.Append(':');
                fs.Append(':');
                fs.Append(' ');
            }
        }
    }
}

#endif
