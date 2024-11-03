using EncosyTower.Modules.SourceGen;
using Microsoft.CodeAnalysis;

namespace EncosyTower.Modules.DataAuthoring.SourceGen
{
    public class ConverterRef
    {
        public ConverterKind Kind { get; set; }

        public ITypeSymbol ConverterType { get; set; }

        public ITypeSymbol TargetType { get; set; }

        public TypeRef SourceTypeRef { get; } = new();

        public string Convert(string expression)
        {
            if (ConverterType == null)
            {
                return expression;
            }

            if (Kind == ConverterKind.Instance)
            {
                return $"new {ConverterType.ToFullName()}().Convert({expression})";
            }

            if (Kind == ConverterKind.Static)
            {
                return $"{ConverterType.ToFullName()}.Convert({expression})";
            }

            return expression;
        }

        public void CopyFrom(ConverterRef source)
        {
            if (source == null) return;

            Kind = source.Kind;
            ConverterType = source.ConverterType;
            TargetType = source.TargetType;
            SourceTypeRef.CopyFrom(source.SourceTypeRef);
        }
    }
}
