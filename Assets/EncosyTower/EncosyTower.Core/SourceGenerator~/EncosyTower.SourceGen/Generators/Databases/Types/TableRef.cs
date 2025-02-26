using Microsoft.CodeAnalysis;
using Newtonsoft.Json.Utilities;

namespace EncosyTower.SourceGen.Generators.Databases
{
    public class TableRef
    {
        public INamedTypeSymbol Type { get; set; }

        public string PropertyName { get; set; }

        public NamingStrategy NamingStrategy { get; set; }
    }
}
