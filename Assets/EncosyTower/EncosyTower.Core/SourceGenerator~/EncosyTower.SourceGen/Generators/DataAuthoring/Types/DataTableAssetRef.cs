using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

namespace EncosyTower.SourceGen.Generators.DataAuthoring
{
    public class DataTableAssetRef
    {
        public ITypeSymbol Symbol { get; set; }

        public ITypeSymbol IdType { get; set; }

        public ITypeSymbol DataType { get; set; }

        public ImmutableArray<ITypeSymbol> NestedDataTypes { get; set; }
    }
}
