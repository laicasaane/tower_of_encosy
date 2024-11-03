using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

namespace EncosyTower.Modules.DataAuthoring.SourceGen
{
    public class DataTableAssetRef
    {
        public ITypeSymbol Symbol { get; set; }

        public ITypeSymbol IdType { get; set; }

        public ITypeSymbol DataType { get; set; }

        public ImmutableArray<ITypeSymbol> NestedDataTypes { get; set; }
    }
}
