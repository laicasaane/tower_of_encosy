using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Newtonsoft.Json.Utilities;

namespace EncosyTower.SourceGen.Generators.DatabaseAuthoring
{
    public class DatabaseRef
    {
        public TypeDeclarationSyntax Syntax { get; }

        public ITypeSymbol Symbol { get; }

        public INamedTypeSymbol DatabaseSymbol { get; }

        public AttributeData AuthorAttribute { get; }

        public AttributeData DatabaseAttribute { get; }

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

        public DatabaseRef(
              TypeDeclarationSyntax syntax
            , ITypeSymbol symbol
            , INamedTypeSymbol databaseSymbol
            , AttributeData authorAttribute
            , AttributeData databaseAttribute
        )
        {
            Syntax = syntax;
            Symbol = symbol;
            DatabaseSymbol = databaseSymbol;
            AuthorAttribute = authorAttribute;
            DatabaseAttribute = databaseAttribute;
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
