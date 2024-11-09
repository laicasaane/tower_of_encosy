using System.Collections.Generic;
using System.Collections.Immutable;
using EncosyTower.Modules.SourceGen;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace EncosyTower.Modules.Mvvm.InternalUnionSourceGen
{
    public partial class InternalUnionDeclaration
    {
        public ImmutableArray<TypeRef> ValueTypeRefs { get; }

        public ImmutableArray<TypeRef> RefTypeRefs { get; }

        public InternalUnionDeclaration(
              ImmutableArray<TypeRef> candidates
            , ImmutableArray<ITypeSymbol> candidatesToIgnore
        )
        {
            var valueTypeFiltered = new Dictionary<string, TypeRef>();
            var refTypeFiltered = new Dictionary<string, TypeRef>();
            var ignoredTypes = new HashSet<string>();

            foreach (var candidate in candidatesToIgnore)
            {
                ignoredTypes.Add(candidate.ToFullName());
            }

            foreach (var candidate in candidates)
            {
                var symbol = candidate.Symbol;
                var typeName = symbol.ToFullName();

                if (typeName.ToUnionType().IsNativeUnionType()
                    || ignoredTypes.Contains(typeName)
                )
                {
                    continue;
                }

                if (symbol.IsUnmanagedType)
                {
                    var size = 0;
                    symbol.GetUnmanagedSize(ref size);
                    candidate.UnmanagedSize = size;

                    if (valueTypeFiltered.ContainsKey(typeName) == false)
                    {
                        valueTypeFiltered[typeName] = candidate;
                    }
                }
                else
                {
                    if (refTypeFiltered.ContainsKey(typeName) == false)
                    {
                        refTypeFiltered[typeName] = candidate;
                    }
                }
            }

            using var valueTypeRefs = ImmutableArrayBuilder<TypeRef>.Rent();
            using var refTypeRefs = ImmutableArrayBuilder<TypeRef>.Rent();

            valueTypeRefs.AddRange(valueTypeFiltered.Values);
            refTypeRefs.AddRange(refTypeFiltered.Values);

            ValueTypeRefs = valueTypeRefs.ToImmutable();
            RefTypeRefs = refTypeRefs.ToImmutable();
        }
    }

    public class TypeRef
    {
        public TypeSyntax Syntax { get; set; }

        public ITypeSymbol Symbol { get; set; }

        public int? UnmanagedSize { get; set; }
    }
}
