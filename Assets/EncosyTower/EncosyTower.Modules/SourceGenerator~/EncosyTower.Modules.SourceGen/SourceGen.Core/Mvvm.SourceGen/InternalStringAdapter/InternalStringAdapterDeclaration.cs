using System.Collections.Generic;
using System.Collections.Immutable;
using EncosyTower.Modules.SourceGen;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace EncosyTower.Modules.Mvvm.InternalStringAdapterSourceGen
{
    public partial class InternalStringAdapterDeclaration
    {
        public ImmutableArray<TypeRef> TypeRefs { get; }

        public InternalStringAdapterDeclaration(
              ImmutableArray<TypeRef> candidates
            , ImmutableArray<INamedTypeSymbol> candidatesToIgnore
        )
        {
            var typeFiltered = new Dictionary<string, TypeRef>();
            var typesToIgnore = new HashSet<string>();

            foreach (var candidate in candidatesToIgnore)
            {
                typesToIgnore.Add(candidate.ToFullName());
            }

            foreach (var candidate in candidates)
            {
                var symbol = candidate.Symbol;
                var typeName = symbol.ToFullName();

                if (typeName.ToUnionType().IsNativeUnionType()
                    || typesToIgnore.Contains(typeName)
                )
                {
                    continue;
                }

                if (typeFiltered.ContainsKey(typeName) == false)
                {
                    typeFiltered[typeName] = candidate;
                }
            }

            using var typeRefs = ImmutableArrayBuilder<TypeRef>.Rent();
            typeRefs.AddRange(typeFiltered.Values);
            TypeRefs = typeRefs.ToImmutable();
        }
    }

    public class TypeRef
    {
        public TypeSyntax Syntax { get; set; }

        public ITypeSymbol Symbol { get; set; }
    }
}
