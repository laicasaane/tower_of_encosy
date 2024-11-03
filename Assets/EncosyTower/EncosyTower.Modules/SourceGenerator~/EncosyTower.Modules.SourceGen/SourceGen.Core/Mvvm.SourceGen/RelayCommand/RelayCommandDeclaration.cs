using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;
using System.Threading;
using EncosyTower.Modules.SourceGen;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace EncosyTower.Modules.Mvvm.RelayCommandSourceGen
{
    public partial class RelayCommandDeclaration
    {
        public const string RELAY_COMMAND_ATTRIBUTE = "global::EncosyTower.Modules.Mvvm.Input.RelayCommandAttribute";

        public ClassDeclarationSyntax Syntax { get; }

        public INamedTypeSymbol Symbol { get; }

        public string ClassName { get; }

        public string FullyQualifiedName { get; }

        public bool IsValid { get; }

        public ImmutableArray<MemberRef> MemberRefs { get; }

        public RelayCommandDeclaration(
              ClassDeclarationSyntax candidate
            , SemanticModel semanticModel
            , CancellationToken token
        )
        {
            using var memberRefs = ImmutableArrayBuilder<MemberRef>.Rent();
            using var diagnosticBuilder = ImmutableArrayBuilder<DiagnosticInfo>.Rent();

            Syntax = candidate;
            Symbol = semanticModel.GetDeclaredSymbol(candidate, token);

            var classNameSb = new StringBuilder(Syntax.Identifier.Text);

            if (candidate.TypeParameterList is TypeParameterListSyntax typeParamList
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

            ClassName = classNameSb.ToString();
            FullyQualifiedName = Symbol.ToFullName();

            var members = Symbol.GetMembers();
            var methodMap = new Dictionary<string, IMethodSymbol>();
            var methodCandidates = new List<(IMethodSymbol, string)>();

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
                        memberRefs.Add(new MemberRef { Member = method });
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
                    memberRefs.Add(new MemberRef {
                        Member = method,
                        CanExecuteMethod = canExecuteMethod,
                    });
                }
            }

            MemberRefs = memberRefs.ToImmutable();
            IsValid = MemberRefs.Length > 0;

            foreach (var memberRef in MemberRefs)
            {
                memberRef.Member.GatherForwardedAttributes(
                      semanticModel
                    , token
                    , true
                    , diagnosticBuilder
                    , out var fieldAttributes
                    , out var propertyAttributes
                    , DiagnosticDescriptors.InvalidFieldOrPropertyTargetedAttributeOnRelayCommandMethod
                );

                memberRef.ForwardedFieldAttributes = fieldAttributes;
                memberRef.ForwardedPropertyAttributes = propertyAttributes;
            }
        }

        public class MemberRef
        {
            public IMethodSymbol Member { get; set; }

            public IMethodSymbol CanExecuteMethod { get; set; }

            public ImmutableArray<AttributeInfo> ForwardedFieldAttributes { get; set; }

            public ImmutableArray<AttributeInfo> ForwardedPropertyAttributes { get; set; }
        }
    }
}
