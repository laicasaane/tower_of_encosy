using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using EncosyTower.SourceGen.Common.Mvvm.Common;
using EncosyTower.SourceGen.TypeModeling.Models;
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
    public partial struct RelayCommandSpec : IEquatable<RelayCommandSpec>
    {
        public const string RELAY_COMMAND_ATTRIBUTE = "global::EncosyTower.Mvvm.Input.RelayCommandAttribute";

        /// <summary>Excluded from <see cref="Equals(RelayCommandSpec)"/> and
        /// <see cref="GetHashCode"/> — location data is not stable across incremental runs.</summary>
        public LocationInfo location;

        public string className;
        public string hintName;
        public string sourceFilePath;
        public string openingSource;
        public string closingSource;
        public EquatableArray<MemberSpec> memberRefs;

        public readonly bool IsValid
            => string.IsNullOrEmpty(className) == false
            && string.IsNullOrEmpty(hintName) == false
            && memberRefs.Count > 0;

        public readonly bool Equals(RelayCommandSpec other)
            => string.Equals(className, other.className, StringComparison.Ordinal)
            && string.Equals(hintName, other.hintName, StringComparison.Ordinal)
            && string.Equals(sourceFilePath, other.sourceFilePath, StringComparison.Ordinal)
            && string.Equals(openingSource, other.openingSource, StringComparison.Ordinal)
            && string.Equals(closingSource, other.closingSource, StringComparison.Ordinal)
            && memberRefs.Equals(other.memberRefs);

        public readonly override bool Equals(object obj)
            => obj is RelayCommandSpec other && Equals(other);

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
        /// populated, cache-friendly <see cref="RelayCommandSpec"/>.
        /// Called once per class inside the <c>ForAttributeWithMetadataName</c> transform.
        /// </summary>
        public static RelayCommandSpec Extract(
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

            using var memberRefsBuilder = ImmutableArrayBuilder<MemberSpec>.Rent();
            using var diagnosticBuilder = ImmutableArrayBuilder<DiagnosticInfo>.Rent();

            var methodModelMap = new Dictionary<string, MethodModel>();
            var methodCandidates = new List<(string methodName, string canExecuteName)>();

            var typeModel = classSymbol.ToModel(
                  token
                , new ModelOptions(ModelParts.Methods | ModelParts.Attributes)
            );
            var typeMethods = typeModel.Methods;
            var typeMethodCount = typeMethods.Count;

            for (var i = 0; i < typeMethodCount; i++)
            {
                token.ThrowIfCancellationRequested();

                var methodModel = typeMethods[i];
                var paramCount = methodModel.Parameters.Count;
                methodModelMap[methodModel.Name] = methodModel;

                if (paramCount > 1)
                {
                    continue;
                }

                if (paramCount == 1)
                {
                    var paramRefKind = methodModel.Parameters[0].RefKind;

                    if (paramRefKind is not (RefKind.None or RefKind.In))
                    {
                        continue;
                    }
                }

                var methodAttrs = methodModel.Attributes;
                var methodAttrCount = methodAttrs.Count;
                AttributeModel relayCommandAttr = default;
                var hasRelayCommandAttr = false;

                for (var j = 0; j < methodAttrCount; j++)
                {
                    if (string.Equals(methodAttrs[j].FullName, RELAY_COMMAND_ATTRIBUTE, StringComparison.Ordinal))
                    {
                        relayCommandAttr = methodAttrs[j];
                        hasRelayCommandAttr = true;
                        break;
                    }
                }

                if (hasRelayCommandAttr == false)
                {
                    continue;
                }

                var namedArgs = relayCommandAttr.NamedArguments;
                var namedArgCount = namedArgs.Count;

                if (namedArgCount > 0)
                {
                    for (var j = 0; j < namedArgCount; j++)
                    {
                        var namedArg = namedArgs[j];

                        if (string.Equals(namedArg.Name, "CanExecute", StringComparison.Ordinal)
                            && string.Equals(namedArg.Value, methodModel.Name, StringComparison.Ordinal) == false
                            && string.IsNullOrEmpty(namedArg.Value) == false
                        )
                        {
                            methodCandidates.Add((methodModel.Name, namedArg.Value));
                            break;
                        }
                    }
                }
                else
                {
                    memberRefsBuilder.Add(new MemberSpec {
                        location = default,
                        methodName = methodModel.Name,
                        paramTypeName = paramCount > 0
                            ? methodModel.Parameters[0].TypeFullName
                            : null,
                    });
                }
            }

            var methodMap = new Dictionary<string, IMethodSymbol>();
            var allMembers = classSymbol.GetMembers();
            var allMemberCount = allMembers.Length;

            for (var i = 0; i < allMemberCount; i++)
            {
                if (allMembers[i] is IMethodSymbol sym && sym.Parameters.Length <= 1)
                {
                    methodMap[sym.Name] = sym;
                }
            }

            var filtered = new HashSet<string>();

            foreach (var (methodName, canExecuteName) in methodCandidates)
            {
                filtered.Clear();

                if (methodModelMap.TryGetValue(canExecuteName, out var canExecuteMethodModel) == false)
                {
                    continue;
                }

                if (string.Equals(
                      canExecuteMethodModel.ReturnTypeFullName
                    , "global::System.Boolean"
                    , StringComparison.Ordinal
                ) == false)
                {
                    continue;
                }

                var isValid = true;

                if (canExecuteMethodModel.Parameters.Count != 0)
                {
                    if (methodModelMap.TryGetValue(methodName, out var commandMethodModel) == false
                        || commandMethodModel.Parameters.Count != canExecuteMethodModel.Parameters.Count
                    )
                    {
                        continue;
                    }

                    var cmdParams = commandMethodModel.Parameters;
                    var cmdParamCount = cmdParams.Count;

                    for (var k = 0; k < cmdParamCount; k++)
                    {
                        filtered.Add(cmdParams[k].TypeFullName);
                    }

                    var ceParams = canExecuteMethodModel.Parameters;
                    var ceParamCount = ceParams.Count;

                    for (var k = 0; k < ceParamCount; k++)
                    {
                        if (filtered.Contains(ceParams[k].TypeFullName) == false)
                        {
                            isValid = false;
                            break;
                        }
                    }
                }

                if (isValid)
                {
                    memberRefsBuilder.Add(new MemberSpec {
                        location = default,
                        methodName = methodName,
                        paramTypeName = methodModelMap.TryGetValue(methodName, out var m) && m.Parameters.Count > 0
                            ? m.Parameters[0].TypeFullName
                            : null,
                        canExecuteMethodName = canExecuteMethodModel.Name,
                        canExecuteHasParam = canExecuteMethodModel.Parameters.Count > 0,
                    });
                }
            }

            if (memberRefsBuilder.Count == 0)
            {
                return default;
            }

            var pendingRefs = memberRefsBuilder.ToImmutable();
            using var finalBuilder = ImmutableArrayBuilder<MemberSpec>.Rent();
            var pendingRefCount = pendingRefs.Length;

            for (var i = 0; i < pendingRefCount; i++)
            {
                var memberRef = pendingRefs[i];

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

                var loc = method.Locations.Length > 0
                    ? LocationInfo.From(method.Locations[0])
                    : default;

                finalBuilder.Add(new MemberSpec {
                    location = loc,
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

            return new RelayCommandSpec {
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
            p.PrintLine("using SCDC = global::System.CodeDom.Compiler;");
            p.PrintLine("using SCM = global::System.ComponentModel;");
            p.PrintLine("using SDCA = global::System.Diagnostics.CodeAnalysis;");
            p.PrintLine("using ETMI = global::EncosyTower.Mvvm.Input;");
            p.PrintLine("using ETMISG = global::EncosyTower.Mvvm.Input.SourceGen;");
            p.PrintEndLine();
            p.Print("#pragma warning restore CS0105 // Using directive appeared previously in this namespace").PrintEndLine();
            p.PrintEndLine();
        }

        /// <summary>
        /// Cache-friendly, equatable model for a single <c>[RelayCommand]</c>-decorated method.
        /// All symbol-derived data is pre-computed as strings — no <see cref="ISymbol"/> or
        /// <see cref="SyntaxNode"/> references are retained.
        /// </summary>
        public struct MemberSpec : IEquatable<MemberSpec>
        {
            /// <summary>Excluded from <see cref="Equals(MemberSpec)"/> and
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

            public readonly bool Equals(MemberSpec other)
                => string.Equals(methodName, other.methodName, StringComparison.Ordinal)
                && string.Equals(paramTypeName, other.paramTypeName, StringComparison.Ordinal)
                && string.Equals(canExecuteMethodName, other.canExecuteMethodName, StringComparison.Ordinal)
                && canExecuteHasParam == other.canExecuteHasParam
                && forwardedFieldAttributes.Equals(other.forwardedFieldAttributes)
                && forwardedPropertyAttributes.Equals(other.forwardedPropertyAttributes);

            public readonly override bool Equals(object obj)
                => obj is MemberSpec other && Equals(other);

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
