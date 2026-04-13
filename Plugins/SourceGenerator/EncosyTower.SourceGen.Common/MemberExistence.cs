namespace EncosyTower.SourceGen
{
    public readonly record struct MemberExistence(bool DoesExist, bool IsStatic, bool IsNullable, int ParamCount)
    {
        public MemberExistence DefaultIfNullableIs(bool value)
            => IsNullable == value ? default : this;
    }
}
