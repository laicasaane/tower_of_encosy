using System.Collections.Generic;
using System.Collections.Immutable;
using EncosyTower.Modules.SourceGen;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace EncosyTower.Modules.PolyStructs.SourceGen
{
    public class StructRef
    {
        public StructDeclarationSyntax Syntax { get; }

        public INamedTypeSymbol Symbol { get; }

        public string Name { get; }

        public string FullName { get; }

        public string Identifier { get; }

        public ImmutableArray<FieldRef> Fields { get; }

        public Dictionary<INamedTypeSymbol, InterfaceRef> Interfaces { get; }

        public StructRef(
              StructDeclarationSyntax syntax
            , INamedTypeSymbol symbol
        )
        {
            Syntax = syntax;
            Symbol = symbol;
            Name = symbol.Name;
            FullName = symbol.ToFullName();
            Identifier = symbol.ToValidIdentifier();
            Interfaces = new(SymbolEqualityComparer.Default);

            using var fieldArrayBuilder = ImmutableArrayBuilder<FieldRef>.Rent();

            foreach (var member in symbol.GetMembers())
            {
                if (member is IFieldSymbol field && field.IsStatic == false)
                {
                    if (field.AssociatedSymbol is IPropertySymbol property)
                    {
                        fieldArrayBuilder.Add(new FieldRef(property.Type, property.Name));
                    }
                    else
                    {
                        fieldArrayBuilder.Add(new FieldRef(field.Type, field.Name));
                    }

                    continue;
                }
            }

            Fields = fieldArrayBuilder.ToImmutable();
        }
    }
}
