using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Newtonsoft.Json.Utilities;

namespace EncosyTower.SourceGen.Generators.Databases
{
    public class DatabaseRef
    {
        public TypeDeclarationSyntax Syntax { get; }

        public ITypeSymbol Symbol { get; }

        public AttributeData Attribute { get; }

        public NamingStrategy NamingStrategy { get; set; }

        public ImmutableArray<TableRef> Tables { get; private set; }

        public string AssetName { get; set; }

        public DatabaseRef(TypeDeclarationSyntax syntax, ITypeSymbol symbol, AttributeData attribute)
        {
            Syntax = syntax;
            Symbol = symbol;
            Attribute = attribute;
            Tables = ImmutableArray<TableRef>.Empty;
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
