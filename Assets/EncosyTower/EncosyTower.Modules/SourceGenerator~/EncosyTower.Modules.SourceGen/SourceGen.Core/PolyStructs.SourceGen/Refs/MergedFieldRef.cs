using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace EncosyTower.Modules.PolyStructs.SourceGen
{
    public class MergedFieldRef
    {
        public ITypeSymbol Type { get; set; }

        public string Name { get; set; }

        public Dictionary<INamedTypeSymbol, string> StructToFieldMap { get; } = new(SymbolEqualityComparer.Default);
    }
}
