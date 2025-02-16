using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace EncosyTower.Modules.DataAuthoring.SourceGen
{
    public class TableRef
    {
        public FieldDeclarationSyntax FieldSyntax { get; set; }

        public INamedTypeSymbol Type { get; set; }

        public INamedTypeSymbol BaseType { get; set; }

        public string SheetName { get; set; }

        public NamingStrategy NamingStrategy { get; set; }

        /// <summary>
        /// Target Type --map-to--> Converter Ref
        /// </summary>
        public Dictionary<ITypeSymbol, ConverterRef> ConverterMap { get; } = new(SymbolEqualityComparer.Default);
    }
}
