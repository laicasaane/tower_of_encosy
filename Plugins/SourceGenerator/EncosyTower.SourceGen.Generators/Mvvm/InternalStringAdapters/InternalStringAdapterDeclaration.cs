using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace EncosyTower.SourceGen.Generators.Mvvm.InternalStringAdapters
{
    public partial class InternalStringAdapterDeclaration
    {
        public ImmutableArray<StringAdapterSpec> Candidates { get; }

        public InternalStringAdapterDeclaration(
              ImmutableArray<StringAdapterSpec> candidates
            , ImmutableArray<string> existingAdapterTypeNames
        )
        {
            var typeFiltered = new Dictionary<string, StringAdapterSpec>();
            var typesToIgnore = new HashSet<string>(existingAdapterTypeNames, StringComparer.Ordinal);

            foreach (var candidate in candidates)
            {
                if (candidate.IsValid == false)
                {
                    continue;
                }

                var typeName = candidate.fullTypeName;

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

            using var builder = ImmutableArrayBuilder<StringAdapterSpec>.Rent();
            builder.AddRange(typeFiltered.Values);
            Candidates = builder.ToImmutable();
        }
    }
}
