using System.Runtime.CompilerServices;
using EncosyTower.Common;

namespace EncosyTower.Logging
{
    public readonly partial struct CallerInfo
    {
        /// <summary>
        /// The value retrieved from <see cref="CallerFilePathAttribute"/>.
        /// </summary>
        public readonly string FilePath;

        /// <summary>
        /// The value retrieved from <see cref="CallerMemberNameAttribute"/>.
        /// </summary>
        public readonly string MemberName;

        /// <summary>
        /// The value retrieved from <see cref="CallerLineNumberAttribute"/>.
        /// </summary>
        public readonly int LineNumber;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private CallerInfo(
              int lineNumber
            , string memberName
            , string filePath
        )
        {
            LineNumber = lineNumber;
            MemberName = memberName;
            FilePath = filePath;
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
        {
            if (MemberName.IsNotEmpty() && FilePath.IsNotEmpty())
            {
                return $"{LineNumber} :: {MemberName} :: {FilePath}";
            }

            if (MemberName.IsNotEmpty())
            {
                return $"{LineNumber} :: {MemberName}";
            }

            if (FilePath.IsNotEmpty())
            {
                return $"{LineNumber} :: {FilePath}";
            }

            return LineNumber.ToString();
        }
    }
}
