using Microsoft.CodeAnalysis;

namespace EncosyTower.SourceGen.Generators.DatabaseAuthoring
{
    public class TypeRef
    {
        public ITypeSymbol Type { get; set; }

        public CollectionTypeRef CollectionTypeRef { get; } = new();

        public void CopyFrom(TypeRef source)
        {
            if (source == null) return;

            Type = source.Type;
            CollectionTypeRef.CopyFrom(source.CollectionTypeRef);
        }
    }
}
