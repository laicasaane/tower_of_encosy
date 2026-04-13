namespace EncosyTower.SourceGen
{
    public enum EqualityStrategy
    {
        Default = 0,
        Equals,
        Operator,
    }

    public readonly record struct Equality(EqualityStrategy Strategy, bool IsStatic, bool IsNullable);
}
