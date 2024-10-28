using System.Runtime.CompilerServices;
using Unity.Collections;

namespace EncosyTower.Modules.Logging
{
    public readonly struct CallerInfo
    {
        public readonly string MemberName;
        public readonly string FilePath;
        public readonly int LineNumber;
        public readonly bool IsValid;

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
            IsValid = true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override string ToString()
            => IsValid ? $"{LineNumber} :: {MemberName} :: {FilePath}" : string.Empty;

#if UNITY_COLLECTIONS
        public FixedString4096Bytes ToFixedString()
        {
            var fs = new FixedString4096Bytes();

            if (IsValid)
            {
                fs.Append(LineNumber);
                AppendSeparator(ref fs);
                fs.Append(MemberName);
                AppendSeparator(ref fs);
                fs.Append(FilePath);
            }

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
#endif
    }

    public static class CallerInfoExtensions
    {
        public static CallerInfo GetCallerInfo(
              this object _
            , [CallerLineNumber] int lineNumber = 0
            , [CallerMemberName] string memberName = ""
            , [CallerFilePath] string filePath = ""
        )
        {
            return new CallerInfo(lineNumber, memberName, filePath);
        }
    }
}
