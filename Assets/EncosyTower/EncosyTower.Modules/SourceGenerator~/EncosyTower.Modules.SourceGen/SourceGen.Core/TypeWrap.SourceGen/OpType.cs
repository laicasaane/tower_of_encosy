namespace EncosyTower.Modules.TypeWrap.SourceGen
{
    public readonly struct OpType
    {
        public readonly string Value;
        public readonly bool IsWrapper;

        public OpType(string value, bool isWrapper = false)
        {
            Value = value;
            IsWrapper = isWrapper;
        }
    }
}
