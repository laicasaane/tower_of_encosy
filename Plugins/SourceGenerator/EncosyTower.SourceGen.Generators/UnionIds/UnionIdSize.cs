namespace EncosyTower.SourceGen.Generators.UnionIds
{
    public enum UnionIdSize : byte
    {
        Default = 0,
        UShort = 2,
        UInt = 4,
        ULong = 8,
        UInt3 = UInt * 3,
        ULong2 = ULong * 2,
    }
}
