using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace EncosyTower.SourceGen.Generators.DatabaseAuthoring
{
    public class DataTableAssetRefList
    {
        public readonly List<string> FieldNames = new();
        public readonly ITypeSymbol DataType;

        public DataTableAssetRefList(ITypeSymbol dataType)
        {
            DataType = dataType;
        }
    }
}
