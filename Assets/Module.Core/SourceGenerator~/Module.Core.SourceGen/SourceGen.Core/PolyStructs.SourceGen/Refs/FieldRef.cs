using Microsoft.CodeAnalysis;

namespace Module.Core.PolyStructs.SourceGen
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
