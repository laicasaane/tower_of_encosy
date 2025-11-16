using Microsoft.CodeAnalysis;

namespace EncosyTower.SourceGen.Generators.PolyStructs
{
    public class FieldRef
    {
        public ITypeSymbol Type { get; }

        public string Name { get; }

        public string MergedName { get; set; }

        public FieldRef(ITypeSymbol type, string name)
        {
            this.Type = type;
            this.Name = name;
        }
    }
}
