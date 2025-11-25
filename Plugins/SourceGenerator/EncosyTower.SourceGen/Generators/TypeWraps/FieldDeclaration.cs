using Microsoft.CodeAnalysis;

namespace EncosyTower.SourceGen.Generators.TypeWraps
{
    public struct FieldDeclaration
    {
        public string name;
        public string typeName;
        public bool sameType;
        public bool isConst;
        public bool isStatic;
        public bool isReadOnly;

        public static FieldDeclaration Create(IFieldSymbol field, INamedTypeSymbol fieldTypeSymbol)
        {
            return new FieldDeclaration {
                name = field.Name,
                typeName = field.Type.ToFullName(),
                sameType = SymbolEqualityComparer.Default.Equals(field.Type, fieldTypeSymbol),
                isConst = field.IsConst,
                isStatic = field.IsStatic,
                isReadOnly = field.IsReadOnly,
            };
        }
    }
}
