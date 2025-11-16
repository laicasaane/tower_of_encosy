using Microsoft.CodeAnalysis;

namespace EncosyTower.SourceGen.Generators.DatabaseAuthoring
{
    public class MemberRef
    {
        public TypeRef TypeRef { get; } = new();

        public bool TypeHasParameterlessConstructor { get; set; }

        public ISymbol Symbol { get; set; }

        public string PropertyName { get; set; }

        public ConverterRef ConverterRef { get; } = new();

        public TypeRef SelectTypeRef()
        {
            if (ConverterRef.Kind == ConverterKind.None)
            {
                return TypeRef;
            }

            return ConverterRef.SourceTypeRef;
        }
    }
}
