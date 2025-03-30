using System;
using System.Diagnostics.CodeAnalysis;
using EncosyTower.StringIds;

namespace EncosyTower.VisualCommands
{
    public enum VisualPropertyType : byte
    {
        Undefined,
        Bool,
        Bounds,
        BoundsInt,
        DateTime,
        Double,
        Enum,
        Float,
        Integer,
        Long,
        Rect,
        RectInt,
        String,
        UnsignedInteger,
        UnsignedLong,
        Vector2,
        Vector2Int,
        Vector3,
        Vector3Int,
        Vector4,
    }

    public record VisualPropertyData(
          [NotNull] string Name
        , [NotNull] string PropertyName
        , VisualPropertyType Type
        , int Order
        , string Label
        , Enum DefaultEnumValue = default
    );

    public record VisualDirectoryData([NotNull] string Name, string Label, StringId Id);

    public record VisualCommandData(
          string Name
        , [NotNull] IVisualCommand Command
        , [NotNull] VisualPropertyData[] Properties
        , int Order
        , string Label
        , VisualDirectoryData Directory
    );
}
