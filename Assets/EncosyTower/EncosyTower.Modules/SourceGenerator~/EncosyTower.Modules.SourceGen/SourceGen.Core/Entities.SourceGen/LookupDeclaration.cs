using System.Collections.Generic;
using System.Collections.Immutable;
using EncosyTower.Modules.SourceGen;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace EncosyTower.Modules.Entities.SourceGen
{
    internal abstract class LookupDeclaration
    {
        private const string LOOKUP_ATTRIBUTE = "global::EncosyTower.Modules.Entities.LookupAttribute";

        protected abstract string Interface { get; }

        protected virtual string Interface2 { get => string.Empty; }

        public abstract string InterfaceLookupRO { get; }

        public abstract string InterfaceLookupRW { get; }

        public StructDeclarationSyntax Syntax { get; }

        public INamedTypeSymbol Symbol { get; }

        public string FullyQualifiedName { get; private set; }

        public ImmutableArray<TypeRef> TypeRefs { get; private set; }

        public LookupDeclaration(
              SourceProductionContext context
            , StructDeclarationSyntax candidate
            , SemanticModel semanticModel
        )
        {
            Syntax = candidate;
            Symbol = semanticModel.GetDeclaredSymbol(candidate, context.CancellationToken);
            FullyQualifiedName = Symbol.ToFullName();

            using var typeRefs = ImmutableArrayBuilder<TypeRef>.Rent();

            var attributes = Symbol.GetAttributes(LOOKUP_ATTRIBUTE);
            var typeHash = new HashSet<INamedTypeSymbol>(SymbolEqualityComparer.Default);

            foreach (var attribute in attributes)
            {
                var args = attribute.ConstructorArguments;

                if (args.Length < 1 || args[0].Value is not INamedTypeSymbol type)
                {
                    context.ReportDiagnostic(
                          LookupDiagnosticDescriptors.NotTypeOfExpression
                        , attribute.ApplicationSyntaxReference.GetSyntax()
                    );
                    continue;
                }

                if (type.IsUnboundGenericType)
                {
                    context.ReportDiagnostic(
                          LookupDiagnosticDescriptors.OpenGenericTypeNotSupported
                        , attribute.ApplicationSyntaxReference.GetSyntax()
                        , type.Name
                    );
                    continue;
                }

                if (type.IsUnmanagedType == false)
                {
                    context.ReportDiagnostic(
                          LookupDiagnosticDescriptors.ManagedTypeNotSupported
                        , attribute.ApplicationSyntaxReference.GetSyntax()
                        , type.Name
                    );
                    continue;
                }

                if (type.InheritsFromInterface(Interface) == false)
                {
                    context.ReportDiagnostic(
                          LookupDiagnosticDescriptors.IncompatInterface
                        , attribute.ApplicationSyntaxReference.GetSyntax()
                        , type.Name
                        , Interface.Remove(0, 8)
                    );
                    continue;
                }

                var isNullOrEmptyInterface2 = string.IsNullOrEmpty(Interface2);

                if (isNullOrEmptyInterface2 == false
                    && type.InheritsFromInterface(Interface2) == false
                )
                {
                    if (isNullOrEmptyInterface2 == false)
                    {
                        context.ReportDiagnostic(
                              LookupDiagnosticDescriptors.IncompatInterface
                            , attribute.ApplicationSyntaxReference.GetSyntax()
                            , type.Name
                            , Interface2.Remove(0, 8)
                        );
                    }
                    continue;
                }

                var isReadOnly = args.Length > 1 && (bool)args[1].Value;

                if (typeHash.Add(type))
                {
                    typeRefs.Add(new TypeRef {
                        Symbol = type,
                        IsReadOnly = isReadOnly
                    });
                }
            }

            TypeRefs = typeRefs.ToImmutable();
        }

        public class TypeRef
        {
            public INamedTypeSymbol Symbol { get; set; }

            public bool IsReadOnly { get; set; }
        }
    }
}
