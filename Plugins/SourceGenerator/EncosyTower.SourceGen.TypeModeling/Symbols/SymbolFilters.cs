using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace EncosyTower.SourceGen.TypeModeling
{
    public static class SymbolFilters
    {
        public static IEnumerable<FieldSymbol> WithAttribute(
              this FieldSymbolEnumerable source
            , string fullyQualifiedName
        )
        {
            foreach (var model in source)
            {
                if (model.HasAttribute(fullyQualifiedName))
                    yield return model;
            }
        }

        public static IEnumerable<PropertySymbol> WithAttribute(
              this PropertySymbolEnumerable source
            , string fullyQualifiedName
        )
        {
            foreach (var model in source)
            {
                if (model.HasAttribute(fullyQualifiedName))
                    yield return model;
            }
        }

        public static IEnumerable<MethodSymbol> WithAttribute(
              this MethodSymbolEnumerable source
            , string fullyQualifiedName
        )
        {
            foreach (var model in source)
            {
                if (model.HasAttribute(fullyQualifiedName))
                    yield return model;
            }
        }

        public static IEnumerable<EventSymbol> WithAttribute(
              this EventSymbolEnumerable source
            , string fullyQualifiedName
        )
        {
            foreach (var model in source)
            {
                if (model.HasAttribute(fullyQualifiedName))
                    yield return model;
            }
        }

        public static IEnumerable<FieldSymbol> Public(this FieldSymbolEnumerable source)
        {
            foreach (var model in source)
            {
                if (model.Accessibility == Accessibility.Public)
                    yield return model;
            }
        }

        public static IEnumerable<PropertySymbol> Public(this PropertySymbolEnumerable source)
        {
            foreach (var model in source)
            {
                if (model.Accessibility == Accessibility.Public)
                    yield return model;
            }
        }

        public static IEnumerable<MethodSymbol> Public(this MethodSymbolEnumerable source)
        {
            foreach (var model in source)
            {
                if (model.Accessibility == Accessibility.Public)
                    yield return model;
            }
        }

        public static IEnumerable<EventSymbol> Public(this EventSymbolEnumerable source)
        {
            foreach (var model in source)
            {
                if (model.Accessibility == Accessibility.Public)
                    yield return model;
            }
        }

        public static IEnumerable<FieldSymbol> NonPublic(this FieldSymbolEnumerable source)
        {
            foreach (var model in source)
            {
                if (model.Accessibility != Accessibility.Public)
                    yield return model;
            }
        }

        public static IEnumerable<PropertySymbol> NonPublic(this PropertySymbolEnumerable source)
        {
            foreach (var model in source)
            {
                if (model.Accessibility != Accessibility.Public)
                    yield return model;
            }
        }

        public static IEnumerable<MethodSymbol> NonPublic(this MethodSymbolEnumerable source)
        {
            foreach (var model in source)
            {
                if (model.Accessibility != Accessibility.Public)
                    yield return model;
            }
        }

        public static IEnumerable<EventSymbol> NonPublic(this EventSymbolEnumerable source)
        {
            foreach (var model in source)
            {
                if (model.Accessibility != Accessibility.Public)
                    yield return model;
            }
        }

        public static IEnumerable<FieldSymbol> Static(this FieldSymbolEnumerable source)
        {
            foreach (var model in source)
            {
                if (model.IsStatic)
                    yield return model;
            }
        }

        public static IEnumerable<PropertySymbol> Static(this PropertySymbolEnumerable source)
        {
            foreach (var model in source)
            {
                if (model.IsStatic)
                    yield return model;
            }
        }

        public static IEnumerable<MethodSymbol> Static(this MethodSymbolEnumerable source)
        {
            foreach (var model in source)
            {
                if (model.IsStatic)
                    yield return model;
            }
        }

        public static IEnumerable<EventSymbol> Static(this EventSymbolEnumerable source)
        {
            foreach (var model in source)
            {
                if (model.IsStatic)
                    yield return model;
            }
        }

        public static IEnumerable<FieldSymbol> NonStatic(this FieldSymbolEnumerable source)
        {
            foreach (var model in source)
            {
                if (!model.IsStatic)
                    yield return model;
            }
        }

        public static IEnumerable<PropertySymbol> NonStatic(this PropertySymbolEnumerable source)
        {
            foreach (var model in source)
            {
                if (!model.IsStatic)
                    yield return model;
            }
        }

        public static IEnumerable<MethodSymbol> NonStatic(this MethodSymbolEnumerable source)
        {
            foreach (var model in source)
            {
                if (!model.IsStatic)
                    yield return model;
            }
        }

        public static IEnumerable<EventSymbol> NonStatic(this EventSymbolEnumerable source)
        {
            foreach (var model in source)
            {
                if (!model.IsStatic)
                    yield return model;
            }
        }
    }
}
