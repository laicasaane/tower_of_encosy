using System.Collections.Generic;
using System.Collections.Immutable;
using EncosyTower.SourceGen.TypeModeling.Internal;
using Microsoft.CodeAnalysis;

namespace EncosyTower.SourceGen.TypeModeling.Symbols
{
    public readonly struct AttributeSymbol
    {
        private readonly AttributeData _data;

        internal AttributeSymbol(AttributeData data)
        {
            _data = data;
        }

        public bool Exists => _data != null;

        public string FullName
            => _data?.AttributeClass?.ToDisplayString(SymbolFormats.FullyQualified) ?? string.Empty;

        public ImmutableArray<TypedConstant> ConstructorArguments
            => _data?.ConstructorArguments ?? ImmutableArray<TypedConstant>.Empty;

        public ImmutableArray<KeyValuePair<string, TypedConstant>> NamedArguments
            => _data?.NamedArguments ?? ImmutableArray<KeyValuePair<string, TypedConstant>>.Empty;
    }
}
