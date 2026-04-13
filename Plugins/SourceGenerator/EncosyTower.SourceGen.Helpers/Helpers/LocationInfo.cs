using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace EncosyTower.SourceGen
{
    /// <summary>
    /// A cache-friendly, equatable alternative to <see cref="Location"/> for use as
    /// pipeline model data in Roslyn incremental generators.
    /// <para>
    /// <see cref="Location"/> holds a reference to a <see cref="SyntaxTree"/>, which
    /// roots the compilation graph and prevents GC. This struct extracts only the
    /// primitive data needed to reconstruct a <see cref="Location"/> on demand.
    /// </para>
    /// </summary>
    public struct LocationInfo : IEquatable<LocationInfo>
    {
        public string filePath;
        public int spanStart;
        public int spanLength;
        public int startLine;
        public int startCharacter;
        public int endLine;
        public int endCharacter;

        public readonly bool IsValid
            => string.IsNullOrEmpty(filePath) == false;

        public static LocationInfo From(Location location)
        {
            if (location is null || location.Kind == LocationKind.None)
            {
                return default;
            }

            var span = location.SourceSpan;
            var lineSpan = location.GetLineSpan();
            var startPos = lineSpan.StartLinePosition;
            var endPos = lineSpan.EndLinePosition;

            return new LocationInfo {
                filePath = lineSpan.Path,
                spanStart = span.Start,
                spanLength = span.Length,
                startLine = startPos.Line,
                startCharacter = startPos.Character,
                endLine = endPos.Line,
                endCharacter = endPos.Character,
            };
        }

        public readonly Location ToLocation()
        {
            if (string.IsNullOrEmpty(filePath))
            {
                return Location.None;
            }

            return Location.Create(
                  filePath
                , new TextSpan(spanStart, spanLength)
                , new LinePositionSpan(
                      new LinePosition(startLine, startCharacter)
                    , new LinePosition(endLine, endCharacter)
                )
            );
        }

        public readonly override bool Equals(object obj)
            => obj is LocationInfo other && Equals(other);

        public readonly bool Equals(LocationInfo other)
            => string.Equals(filePath, other.filePath, StringComparison.Ordinal)
            && spanStart == other.spanStart
            && spanLength == other.spanLength
            && startLine == other.startLine
            && startCharacter == other.startCharacter
            && endLine == other.endLine
            && endCharacter == other.endCharacter
            ;

        public readonly override int GetHashCode()
            => HashValue.Combine(
                  filePath
                , spanStart
                , spanLength
                , startLine
                , startCharacter
                , endLine
                , endCharacter
            );
    }
}
