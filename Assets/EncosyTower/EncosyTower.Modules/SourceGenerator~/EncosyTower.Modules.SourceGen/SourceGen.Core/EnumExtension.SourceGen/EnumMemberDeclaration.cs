namespace EncosyTower.Modules.EnumExtensions.SourceGen
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
