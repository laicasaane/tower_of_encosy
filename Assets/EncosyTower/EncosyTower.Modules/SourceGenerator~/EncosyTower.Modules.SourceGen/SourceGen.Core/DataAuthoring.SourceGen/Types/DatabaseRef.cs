using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace EncosyTower.Modules.DataAuthoring.SourceGen
{
    public class DatabaseRef
    {
        public ClassDeclarationSyntax Syntax { get; }

        public ITypeSymbol Symbol { get; }

        public AttributeData Attribute { get; }

        public NamingStrategy NamingStrategy { get; set; }

        public ImmutableArray<TableRef> Tables { get; private set; }

        /// <summary>
        /// Target Type --map-to--> Converter Ref
        /// </summary>
        public Dictionary<ITypeSymbol, ConverterRef> ConverterMap { get; }

        /// <summary>
        /// TargetTypeFullName --map-to--> ContainingTypeFullName --map-to--> PropertyName(s)
        /// <br/>
        /// ContainingTypeFullName can be empty if it is not defined.
        /// </summary>
        public Dictionary<ITypeSymbol, Dictionary<ITypeSymbol, HashSet<string>>> HorizontalListMap { get; }

        public DatabaseRef(ClassDeclarationSyntax syntax, ITypeSymbol symbol, AttributeData attribute)
        {
            Syntax = syntax;
            Symbol = symbol;
            Attribute = attribute;
            Tables = ImmutableArray<TableRef>.Empty;
            ConverterMap = new(SymbolEqualityComparer.Default);
            HorizontalListMap = new(SymbolEqualityComparer.Default);
        }

        public void SetTables(ImmutableArray<TableRef> tables)
        {
            if (tables.IsDefault)
            {
                return;
            }

            Tables = tables;
        }
    }
}
