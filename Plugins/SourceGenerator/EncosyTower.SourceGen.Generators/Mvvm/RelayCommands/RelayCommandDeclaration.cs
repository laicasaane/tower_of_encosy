using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using EncosyTower.SourceGen.Common.Mvvm.Common;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace EncosyTower.SourceGen.Generators.Mvvm.RelayCommands
{
    /// <summary>
    /// Cache-friendly, equatable pipeline model for the RelayCommand source generator.
    /// Holds only primitive values and equatable collections — no <see cref="SyntaxNode"/>
    /// or <see cref="ISymbol"/> references — so that Roslyn's incremental generator engine
    /// can cache and compare instances cheaply across multiple compilations.
    /// </summary>
    public partial struct RelayCommandDeclaration : IEquatable<RelayCommandDeclaration>
    {
        public const string RELAY_COMMAND_ATTRIBUTE = "global::EncosyTower.Mvvm.Input.RelayCommandAttribute";

        /// <summary>Excluded from <see cref="Equals(RelayCommandDeclaration)"/> and
        /// <see cref="GetHashCode"/> — location data is not stable across incremental runs.</summary>
        public LocationInfo location;

        public string className;
        public string hintName;
        public string sourceFilePath;
        public string openingSource;
        public string closingSource;
        public EquatableArray<MemberDeclaration> memberRefs;

        public readonly bool IsValid
            => string.IsNullOrEmpty(className) == false
            && string.IsNullOrEmpty(hintName) == false
            && memberRefs.Count > 0;

        public readonly bool Equals(RelayCommandDeclaration other)
            => string.Equals(className, other.className, StringComparison.Ordinal)
            && string.Equals(hintName, other.hintName, StringComparison.Ordinal)
            && string.Equals(sourceFilePath, other.sourceFilePath, StringComparison.Ordinal)
            && string.Equals(openingSource, other.openingSource, StringComparison.Ordinal)
            && string.Equals(closingSource, other.closingSource, StringComparison.Ordinal)
            && memberRefs.Equals(other.memberRefs);

        public readonly override bool Equals(object obj)
            => obj is RelayCommandDeclaration other && Equals(other);

        public readonly override int GetHashCode()
        {
            var hash = new HashValue();
            hash.Add(className);
            hash.Add(hintName);
            hash.Add(sourceFilePath);
            hash.Add(openingSource);
            hash.Add(closingSource);
            hash.Add(memberRefs);
            return hash.ToHashCode();
        }

        /// <summary>
        /// Extracts all relay-command metadata from the annotated class symbol into a fully
        /// populated, cache-friendly <see cref="RelayCommandDeclaration"/>.
        /// Called once per class inside the <c>ForAttributeWithMetadataName</c> transform.
        /// </summary>
        public static RelayCommandDeclaration Extract(
              GeneratorAttributeSyntaxContext context
            , CancellationToken token
        )
        {
            token.ThrowIfCancellationRequested();

            if (context.TargetNode is not ClassDeclarationSyntax classSyntax
                || context.TargetSymbol is not INamedTypeSymbol classSymbol
            )
            {
                return default;
            }

            var classNameSb = new StringBuilder(classSyntax.Identifier.Text);

            if (classSyntax.TypeParameterList is TypeParameterListSyntax typeParamList
                && typeParamList.Parameters.Count > 0
            )
            {
                classNameSb.Append("<");

                var typeParams = typeParamList.Parameters;
                var last = typeParams.Count - 1;

                for (var i = 0; i <= last; i++)
                {
                    classNameSb.Append(typeParams[i].Identifier.Text);

                    if (i < last)
                    {
                        classNameSb.Append(", ");
                    }
                }

                classNameSb.Append(">");
            }

            var className = classNameSb.ToString();

            var semanticModel = context.SemanticModel;
            var syntaxTree = classSyntax.SyntaxTree;
            var fileTypeName = classSymbol.ToFileName();

            var hintName = syntaxTree.GetGeneratedSourceFileName(
                  RelayCommandGenerator.GENERATOR_NAME
                , classSyntax
                , fileTypeName
            );

            var sourceFilePath = syntaxTree.GetGeneratedSourceFilePath(
                  semanticModel.Compilation.AssemblyName
                , RelayCommandGenerator.GENERATOR_NAME
                , fileTypeName
            );

            TypeCreationHelpers.GenerateOpeningAndClosingSource(
                  classSyntax
                , token
                , out var openingSource
                , out var closingSource
                , printAdditionalUsings: PrintAdditionalUsings
            );

            using var memberRefsBuilder = ImmutableArrayBuilder<MemberDeclaration>.Rent();
            using var diagnosticBuilder = ImmutableArrayBuilder<DiagnosticInfo>.Rent();

            var members = classSymbol.GetMembers();
            var methodMap = new Dictionary<string, IMethodSymbol>();
            var methodCandidates = new List<(IMethodSymbol method, string canExecuteName)>();

            foreach (var member in members)
            {
                if (member is not IMethodSymbol method
                    || method.Parameters.Length > 1
                )
                {
                    continue;
                }

                if (method.Parameters.Length == 1)
                {
                    var parameter = method.Parameters[0];

                    if (parameter.RefKind is not (RefKind.None or RefKind.In))
                    {
                        continue;
                    }
                }

                var relayCommandAttrib = method.GetAttribute(RELAY_COMMAND_ATTRIBUTE);

                if (relayCommandAttrib != null)
                {
                    if (relayCommandAttrib.NamedArguments.Length > 0)
                    {
                        foreach (var kv in relayCommandAttrib.NamedArguments)
                        {
                            if (kv.Key == "CanExecute"
                                && kv.Value.Value is string canExecuteMethodName
                                && canExecuteMethodName != method.Name
                            )
                            {
                                methodCandidates.Add((method, canExecuteMethodName));
                            }
                        }
                    }
                    else
                    {
                        memberRefsBuilder.Add(new MemberDeclaration {
                            location = LocationInfo.From(method.Locations.Length > 0 ? method.Locations[0] : Location.None),
                            methodName = method.Name,
                            paramTypeName = method.Parameters.Length > 0
                                ? method.Parameters[0].Type.ToFullName()
                                : null,
                        });
                    }
                }

                methodMap[method.Name] = method;
            }

            var filtered = new HashSet<string>();

            foreach (var (method, canExecuteName) in methodCandidates)
            {
                filtered.Clear();

                if (methodMap.TryGetValue(canExecuteName, out var canExecuteMethod) == false
                    || canExecuteMethod.ReturnType.SpecialType != SpecialType.System_Boolean
                )
                {
                    continue;
                }

                var isValid = true;

                if (canExecuteMethod.Parameters.Length != 0)
                {
                    if (method.Parameters.Length != canExecuteMethod.Parameters.Length)
                    {
                        continue;
                    }

                    foreach (var param in method.Parameters)
                    {
                        filtered.Add(param.Type.ToFullName());
                    }

                    foreach (var param in canExecuteMethod.Parameters)
                    {
                        if (filtered.Contains(param.Type.ToFullName()) == false)
                        {
                            isValid = false;
                            break;
                        }
                    }
                }

                if (isValid)
                {
                    memberRefsBuilder.Add(new MemberDeclaration {
                        location = LocationInfo.From(method.Locations.Length > 0 ? method.Locations[0] : Location.None),
                        methodName = method.Name,
                        paramTypeName = method.Parameters.Length > 0
                            ? method.Parameters[0].Type.ToFullName()
                            : null,
                        canExecuteMethodName = canExecuteMethod.Name,
                        canExecuteHasParam = canExecuteMethod.Parameters.Length > 0,
                    });
                }
            }

            if (memberRefsBuilder.Count == 0)
            {
                return default;
            }

            var pendingRefs = memberRefsBuilder.ToImmutable();
            using var finalBuilder = ImmutableArrayBuilder<MemberDeclaration>.Rent();

            foreach (var memberRef in pendingRefs)
            {
                if (methodMap.TryGetValue(memberRef.methodName, out var method) == false)
                {
                    continue;
                }

                method.GatherForwardedAttributes(
                      semanticModel
                    , token
                    , true
                    , diagnosticBuilder
                    , out var fieldAttributes
                    , out var propertyAttributes
                    , DiagnosticDescriptors.InvalidFieldOrPropertyTargetedAttributeOnRelayCommandMethod
                );

                finalBuilder.Add(new MemberDeclaration {
                    location = memberRef.location,
                    methodName = memberRef.methodName,
                    paramTypeName = memberRef.paramTypeName,
                    canExecuteMethodName = memberRef.canExecuteMethodName,
                    canExecuteHasParam = memberRef.canExecuteHasParam,
                    forwardedFieldAttributes = fieldAttributes.AsEquatableArray(),
                    forwardedPropertyAttributes = propertyAttributes.AsEquatableArray(),
                });
            }

            if (finalBuilder.Count == 0)
            {
                return default;
            }

            return new RelayCommandDeclaration {
                location = LocationInfo.From(classSyntax.GetLocation()),
                className = className,
                hintName = hintName,
                sourceFilePath = sourceFilePath,
                openingSource = openingSource,
                closingSource = closingSource,
                memberRefs = finalBuilder.ToImmutable().AsEquatableArray(),
            };
        }

        private static void PrintAdditionalUsings(ref Printer p)
        {
            p.PrintEndLine();
            p.Print("#pragma warning disable CS0105 // Using directive appeared previously in this namespace").PrintEndLine();
            p.PrintEndLine();
            p.PrintLine("using System.CodeDom.Compiler;");
            p.PrintLine("using System.Diagnostics.CodeAnalysis;");
            p.PrintLine("using EncosyTower.Mvvm.Input;");
            p.PrintLine("using EncosyTower.Mvvm.Input.SourceGen;");
            p.PrintEndLine();
            p.PrintLine("using EditorBrowsableAttribute = global::System.ComponentModel.EditorBrowsableAttribute;");
            p.PrintLine("using EditorBrowsableState = global::System.ComponentModel.EditorBrowsableState;");
            p.PrintEndLine();
            p.Print("#pragma warning restore CS0105 // Using directive appeared previously in this namespace").PrintEndLine();
            p.PrintEndLine();
        }

        /// <summary>
        /// Cache-friendly, equatable model for a single <c>[RelayCommand]</c>-decorated method.
        /// All symbol-derived data is pre-computed as strings — no <see cref="ISymbol"/> or
        /// <see cref="SyntaxNode"/> references are retained.
        /// </summary>
        public struct MemberDeclaration : IEquatable<MemberDeclaration>
        {
            /// <summary>Excluded from <see cref="Equals(MemberDeclaration)"/> and
            /// <see cref="GetHashCode"/> — location data is not stable across incremental runs.</summary>
            public LocationInfo location;

            /// <summary>Method name, e.g. <c>"OnDoSomething"</c>.</summary>
            public string methodName;

            /// <summary>Fully-qualified parameter type name, or <see langword="null"/> when the
            /// method has no parameters.</summary>
            public string paramTypeName;

            /// <summary>Name of the <c>CanExecute</c> method, or <see langword="null"/> when none.</summary>
            public string canExecuteMethodName;

            /// <summary><see langword="true"/> when the <c>CanExecute</c> method itself has a parameter.</summary>
            public bool canExecuteHasParam;

            public EquatableArray<AttributeInfo> forwardedFieldAttributes;
            public EquatableArray<AttributeInfo> forwardedPropertyAttributes;

            public readonly bool Equals(MemberDeclaration other)
                => string.Equals(methodName, other.methodName, StringComparison.Ordinal)
                && string.Equals(paramTypeName, other.paramTypeName, StringComparison.Ordinal)
                && string.Equals(canExecuteMethodName, other.canExecuteMethodName, StringComparison.Ordinal)
                && canExecuteHasParam == other.canExecuteHasParam
                && forwardedFieldAttributes.Equals(other.forwardedFieldAttributes)
                && forwardedPropertyAttributes.Equals(other.forwardedPropertyAttributes);

            public readonly override bool Equals(object obj)
                => obj is MemberDeclaration other && Equals(other);

            public readonly override int GetHashCode()
            {
                var hash = new HashValue();
                hash.Add(methodName);
                hash.Add(paramTypeName);
                hash.Add(canExecuteMethodName);
                hash.Add(canExecuteHasParam);
                hash.Add(forwardedFieldAttributes);
                hash.Add(forwardedPropertyAttributes);
                return hash.ToHashCode();
            }
        }
    }
}
