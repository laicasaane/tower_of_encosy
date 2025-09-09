﻿using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace EncosyTower.SourceGen.Generators.DataTableAssets
{
    public class DataTableAssetRef
    {
        public TypeDeclarationSyntax Syntax { get; set; }

        public ITypeSymbol Symbol { get; set; }

        public ITypeSymbol IdType { get; set; }

        public ITypeSymbol DataType { get; set; }

        public ITypeSymbol ConvertedIdType { get; set; }
    }
}
