namespace EncosyTower.SourceGen.Generators.EnumExtensions
{
    public struct EnumMemberDeclaration
    {
        public string name;
        public string displayName;
        public ulong order;

        public override string ToString()
            => name;
    }
}
