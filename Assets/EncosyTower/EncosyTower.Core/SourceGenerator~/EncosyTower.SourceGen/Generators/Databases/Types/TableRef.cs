using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Newtonsoft.Json.Utilities;

namespace EncosyTower.SourceGen.Generators.Databases
{
    public class TableRef
    {
        public SyntaxNode SyntaxNode { get; set; }

        public INamedTypeSymbol Type { get; set; }

        public INamedTypeSymbol BaseType { get; set; }

        public ITypeSymbol IdType => BaseType.TypeArguments[0];

        public ITypeSymbol DataType => BaseType.TypeArguments[1];

        public string PropertyName { get; set; }

        public NamingStrategy NamingStrategy { get; set; }

        /// <summary>
        /// Target Type --map-to--> Converter Ref
        /// </summary>
        public Dictionary<ITypeSymbol, ConverterRef> ConverterMap { get; } = new(SymbolEqualityComparer.Default);
    }
}
