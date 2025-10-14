namespace EncosyTower.Logging
{
    using System.Runtime.CompilerServices;

    public readonly partial struct CallerInfo
    {
        public readonly string MemberName;
        public readonly string FilePath;
        public readonly int LineNumber;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public CallerInfo(
              int lineNumber
            , string memberName
            , string filePath
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
        public static CallerInfo GetFull(
              [CallerLineNumber] int lineNumber = 0
            , [CallerMemberName] string memberName = ""
            , [CallerFilePath] string filePath = ""
        )
        {
            return new CallerInfo(lineNumber, memberName, filePath);
        }

        /// <summary>
        /// Returns an info with line number, and member name.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static CallerInfo GetSlim(
              [CallerLineNumber] int lineNumber = 0
            , [CallerMemberName] string memberName = ""
        )
        {
            return new CallerInfo(lineNumber, memberName, string.Empty);
        }

        /// <summary>
        /// Returns an info with only line number.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static CallerInfo GetLine([CallerLineNumber] int lineNumber = 0)
        {
            return new CallerInfo(lineNumber, string.Empty, string.Empty);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override string ToString()
            => $"{LineNumber} :: {MemberName} :: {FilePath}";
    }
}

#if UNITY_COLLECTIONS

namespace EncosyTower.Logging
{
    using System.Runtime.CompilerServices;
    using Unity.Collections;

    partial struct CallerInfo
    {
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
