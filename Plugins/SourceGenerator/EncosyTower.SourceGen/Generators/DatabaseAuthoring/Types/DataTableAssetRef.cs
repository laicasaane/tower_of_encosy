using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

namespace EncosyTower.SourceGen.Generators.DatabaseAuthoring
{
    public class DataTableAssetRef
    {
        public TableRef Table { get; set; }

        public INamedTypeSymbol TableType => Table.Type;

        public ITypeSymbol IdType => Table.IdType;

        public ITypeSymbol DataType => Table.DataType;

        public ImmutableArray<ITypeSymbol> NestedDataTypes { get; set; }
    }
}
