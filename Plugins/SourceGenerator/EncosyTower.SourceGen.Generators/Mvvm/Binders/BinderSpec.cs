using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace EncosyTower.SourceGen.Generators.Mvvm.Binders
{
    public partial struct BinderSpec : IEquatable<BinderSpec>
    {
        public const string IBINDER_INTERFACE = "global::EncosyTower.Mvvm.ViewBinding.IBinder";
        public const string BINDER_ATTRIBUTE = "global::EncosyTower.Mvvm.ViewBinding.BinderAttribute";
        public const string BINDING_PROPERTY_ATTRIBUTE = "global::EncosyTower.Mvvm.ViewBinding.BindingPropertyAttribute";
        public const string BINDING_COMMAND_ATTRIBUTE = "global::EncosyTower.Mvvm.ViewBinding.BindingCommandAttribute";
        public const string BINDING_PROPERTY = "global::EncosyTower.Mvvm.ViewBinding.BindingProperty";
        public const string BINDING_COMMAND = "global::EncosyTower.Mvvm.ViewBinding.BindingCommand";
        public const string CONVERTER = "global::EncosyTower.Mvvm.ViewBinding.Converter";
        public const string VARIANT_TYPE = "global::EncosyTower.Variants.Variant";
        public const string MONO_BEHAVIOUR_TYPE = "global::UnityEngine.MonoBehaviour";

        public LocationInfo location;
        public string openingSource;
        public string closingSource;
        public string hintName;
        public string className;
        public string simpleTypeName;
        public string typeIdentifier;
        public bool hasBaseBinder;
        public bool hasOnBindPropertyFailedMethod;
        public bool hasOnBindCommandFailedMethod;
        public bool isSealed;
        public EquatableArray<BindingPropertySpec> bindingPropertyRefs;
        public EquatableArray<BindingCommandSpec> bindingCommandRefs;
        public EquatableArray<NonVariantTypeSpec> nonVariantTypes;

        public readonly bool IsValid
            => string.IsNullOrEmpty(typeIdentifier) == false
            && string.IsNullOrEmpty(simpleTypeName) == false;

        public static BinderSpec Extract(
              GeneratorAttributeSyntaxContext context
            , CancellationToken token
        )
        {
            token.ThrowIfCancellationRequested();

            if (context.TargetNode is not ClassDeclarationSyntax classSyntax
                || context.TargetSymbol is not INamedTypeSymbol symbol
            )
            {
                return default;
            }

            var bindingPropertyNames = new HashSet<string>(StringComparer.Ordinal);
            var converterNames = new HashSet<string>(StringComparer.Ordinal);
            var bindingCommandNames = new HashSet<string>(StringComparer.Ordinal);

            var semanticModel = context.SemanticModel;

            TypeCreationHelpers.GenerateOpeningAndClosingSource(
                  classSyntax
                , token
                , out var openingSource
                , out var closingSource
                , printAdditionalUsings: PrintAdditionalUsings
            );

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
            var hasBaseBinder = false;

            if (symbol.BaseType != null
                && symbol.BaseType.TypeKind == TypeKind.Class
                && symbol.BaseType.HasAttribute(BINDER_ATTRIBUTE)
            )
            {
                hasBaseBinder = true;
            }

            var hasOnBindPropertyFailedMethod = false;
            var hasOnBindCommandFailedMethod = false;
            var baseType = symbol;
            var isCurrentType = true;

            while (baseType != null)
            {
                token.ThrowIfCancellationRequested();

                foreach (var member in baseType.GetMembers("OnBindPropertyFailed"))
                {
                    token.ThrowIfCancellationRequested();

                    if (member is IMethodSymbol checkMethod
                        && isCurrentType == false
                        && checkMethod.DeclaredAccessibility is (Accessibility.Public or Accessibility.Protected)
                        && checkMethod.Parameters.Length == 1
                        && checkMethod.Parameters[0].Type.HasFullName(BINDING_PROPERTY, token)
                    )
                    {
                        hasOnBindPropertyFailedMethod = true;
                        break;
                    }
                }

                foreach (var member in baseType.GetMembers("OnBindCommandFailed"))
                {
                    token.ThrowIfCancellationRequested();

                    if (member is IMethodSymbol checkMethod
                        && isCurrentType == false
                        && checkMethod.DeclaredAccessibility is (Accessibility.Public or Accessibility.Protected)
                        && checkMethod.Parameters.Length == 1
                        && checkMethod.Parameters[0].Type.HasFullName(BINDING_COMMAND, token)
                    )
                    {
                        hasOnBindCommandFailedMethod = true;
                        break;
                    }
                }

                isCurrentType = false;
                baseType = baseType.BaseType;
            }

            token.ThrowIfCancellationRequested();

            var members = symbol.GetMembers();
            using var tempPropMethods = ImmutableArrayBuilder<BinderPropertyScanEntry>.Rent();
            using var tempCmdMethods = ImmutableArrayBuilder<BinderCommandScanEntry>.Rent();
            var nonVariantTypeFilter = new Dictionary<string, NonVariantTypeSpec>(members.Length, StringComparer.Ordinal);

            foreach (var member in members)
            {
                token.ThrowIfCancellationRequested();

                if (member is IMethodSymbol method)
                {
                    var bindingPropAttribute = method.GetAttribute(BINDING_PROPERTY_ATTRIBUTE, token);
                    var bindingCommandAttribute = method.GetAttribute(BINDING_COMMAND_ATTRIBUTE, token);

                    if (bindingPropAttribute != null && method.Parameters.Length < 2)
                    {
                        var paramFullTypeName = string.Empty;
                        var variantConverterPropertyName = string.Empty;
                        var paramRefKind = RefKind.None;
                        var isVariant = true;

                        if (method.Parameters.Length == 1)
                        {
                            var parameter = method.Parameters[0];

                            if (parameter.RefKind is not (RefKind.None or RefKind.In or RefKind.Ref))
                            {
                                continue;
                            }

                            paramRefKind = parameter.RefKind;
                            paramFullTypeName = parameter.Type.ToFullName();
                            isVariant = paramFullTypeName == VARIANT_TYPE;

                            if (isVariant == false)
                            {
                                variantConverterPropertyName = parameter.Type
                                    .ToValidIdentifier().AsSpan().MakeFirstCharUpperCase();
                            }
                        }

                        tempPropMethods.Add(new BinderPropertyScanEntry {
                            method = method,
                            paramFullTypeName = paramFullTypeName,
                            paramRefKind = paramRefKind,
                            variantConverterPropertyName = variantConverterPropertyName,
                            isParameterTypeVariant = isVariant,
                        });

                        if (isVariant == false && string.IsNullOrEmpty(paramFullTypeName) == false
                            && nonVariantTypeFilter.ContainsKey(paramFullTypeName) == false
                        )
                        {
                            nonVariantTypeFilter[paramFullTypeName] = new NonVariantTypeSpec {
                                fullTypeName = paramFullTypeName,
                                converterPropertyName = variantConverterPropertyName,
                            };
                        }
                    }
                    else if (bindingCommandAttribute != null
                        && method.IsPartialDefinition
                        && method.ReturnsVoid
                        && method.Parameters.Length < 2
                    )
                    {
                        var paramFullTypeName = string.Empty;
                        var paramRefKind = RefKind.None;
                        var paramName = string.Empty;

                        if (method.Parameters.Length == 1
                            && method.Parameters[0].RefKind is (RefKind.None or RefKind.In or RefKind.Ref)
                        )
                        {
                            var parameter = method.Parameters[0];
                            paramFullTypeName = parameter.Type.ToFullName();
                            paramRefKind = parameter.RefKind;
                            paramName = parameter.Name;
                        }

                        tempCmdMethods.Add(new BinderCommandScanEntry {
                            method = method,
                            paramFullTypeName = paramFullTypeName,
                            paramRefKind = paramRefKind,
                            paramName = paramName,
                        });
                    }

                    continue;
                }

                if (member is IFieldSymbol field)
                {
                    var typeName = field.Type.ToFullName();

                    if (typeName == BINDING_PROPERTY)
                    {
                        bindingPropertyNames.Add(field.Name);
                    }
                    else if (typeName == CONVERTER)
                    {
                        converterNames.Add(field.Name);
                    }
                    else if (typeName == BINDING_COMMAND)
                    {
                        bindingCommandNames.Add(field.Name);
                    }
                }
            }

            token.ThrowIfCancellationRequested();

            using var propRefBuilder = ImmutableArrayBuilder<BindingPropertySpec>.Rent();

            foreach (var entry in tempPropMethods.WrittenSpan)
            {
                token.ThrowIfCancellationRequested();

                var methodName = entry.method.Name;

                entry.method.GatherForwardedAttributes(
                      semanticModel
                    , token
                    , true
                    , out var fieldAttributes
                    , out _
                );

                propRefBuilder.Add(new BindingPropertySpec {
                    methodName = methodName,
                    paramFullTypeName = entry.paramFullTypeName,
                    paramRefKind = entry.paramRefKind,
                    variantConverterPropertyName = entry.variantConverterPropertyName,
                    isParameterTypeVariant = entry.isParameterTypeVariant,
                    skipBindingProperty = bindingPropertyNames.Contains($"_bindingFieldFor{methodName}"),
                    skipConverter = converterNames.Contains($"_converterFor{methodName}"),
                    forwardedFieldAttributes = fieldAttributes.AsEquatableArray(),
                });
            }

            token.ThrowIfCancellationRequested();

            using var cmdRefBuilder = ImmutableArrayBuilder<BindingCommandSpec>.Rent();

            foreach (var entry in tempCmdMethods.WrittenSpan)
            {
                token.ThrowIfCancellationRequested();

                var methodName = entry.method.Name;

                entry.method.GatherForwardedAttributes(
                      semanticModel
                    , token
                    , false
                    , out var fieldAttributes
                    , out _
                );

                cmdRefBuilder.Add(new BindingCommandSpec {
                    methodName = methodName,
                    paramFullTypeName = entry.paramFullTypeName,
                    paramRefKind = entry.paramRefKind,
                    paramName = entry.paramName,
                    skipBindingCommand = bindingCommandNames.Contains($"_bindingCommandFor{methodName}"),
                    forwardedFieldAttributes = fieldAttributes.AsEquatableArray(),
                });
            }

            using var nonVariantTypesBuilder = ImmutableArrayBuilder<NonVariantTypeSpec>.Rent();
            nonVariantTypesBuilder.AddRange(nonVariantTypeFilter.Values);

            var syntaxTree = classSyntax.SyntaxTree;
            var hintName = syntaxTree.GetHintName(classSyntax, symbol.ToFileName());

            return new BinderSpec {
                location = LocationInfo.From(classSyntax.GetLocation()),
                openingSource = openingSource,
                closingSource = closingSource,
                hintName = hintName,
                className = className,
                simpleTypeName = classSyntax.Identifier.Text,
                typeIdentifier = symbol.ToValidIdentifier(),
                hasBaseBinder = hasBaseBinder,
                hasOnBindPropertyFailedMethod = hasOnBindPropertyFailedMethod,
                hasOnBindCommandFailedMethod = hasOnBindCommandFailedMethod,
                isSealed = symbol.IsSealed,
                bindingPropertyRefs = propRefBuilder.ToImmutable().AsEquatableArray(),
                bindingCommandRefs = cmdRefBuilder.ToImmutable().AsEquatableArray(),
                nonVariantTypes = nonVariantTypesBuilder.ToImmutable().AsEquatableArray(),
            };
        }

        private static void PrintAdditionalUsings(ref Printer p)
        {
            p.PrintEndLine();
            p.Print("#pragma warning disable CS0105 // Using directive appeared previously in this namespace").PrintEndLine();
            p.PrintEndLine();
            p.PrintLine("using S = global::System;");
            p.PrintLine("using SCDC = global::System.CodeDom.Compiler;");
            p.PrintLine("using SCM = global::System.ComponentModel;");
            p.PrintLine("using SCG = global::System.Collections.Generic;");
            p.PrintLine("using SD = global::System.Diagnostics;");
            p.PrintLine("using SDCA = global::System.Diagnostics.CodeAnalysis;");
            p.PrintLine("using SRCS = global::System.Runtime.CompilerServices;");
            p.PrintLine("using ET = global::EncosyTower.Common;");
            p.PrintLine("using ETMCM = global::EncosyTower.Mvvm.ComponentModel;");
            p.PrintLine("using ETMCMSG = global::EncosyTower.Mvvm.ComponentModel.SourceGen;");
            p.PrintLine("using ETMI = global::EncosyTower.Mvvm.Input;");
            p.PrintLine("using ETMVB = global::EncosyTower.Mvvm.ViewBinding;");
            p.PrintLine("using ETMVBSG = global::EncosyTower.Mvvm.ViewBinding.SourceGen;");
            p.PrintLine("using ETV = global::EncosyTower.Variants;");
            p.PrintLine("using ETVC = global::EncosyTower.Variants.Converters;");
            p.PrintLine("using UE = global::UnityEngine;");
            p.PrintEndLine();
            p.Print("#pragma warning restore CS0105 // Using directive appeared previously in this namespace").PrintEndLine();
            p.PrintEndLine();
        }

        public readonly bool Equals(BinderSpec other)
            => string.Equals(className, other.className, StringComparison.Ordinal)
            && string.Equals(simpleTypeName, other.simpleTypeName, StringComparison.Ordinal)
            && string.Equals(typeIdentifier, other.typeIdentifier, StringComparison.Ordinal)
            && hasBaseBinder == other.hasBaseBinder
            && hasOnBindPropertyFailedMethod == other.hasOnBindPropertyFailedMethod
            && hasOnBindCommandFailedMethod == other.hasOnBindCommandFailedMethod
            && isSealed == other.isSealed
            && bindingPropertyRefs.Equals(other.bindingPropertyRefs)
            && bindingCommandRefs.Equals(other.bindingCommandRefs)
            && nonVariantTypes.Equals(other.nonVariantTypes)
            ;

        public readonly override bool Equals(object obj)
            => obj is BinderSpec other && Equals(other);

        public readonly override int GetHashCode()
            => HashValue.Combine(
                  className
                , simpleTypeName
                , typeIdentifier
            )
            .Add(hasBaseBinder)
            .Add(hasOnBindPropertyFailedMethod)
            .Add(hasOnBindCommandFailedMethod)
            .Add(isSealed)
            .Add(bindingPropertyRefs.GetHashCode())
            .Add(bindingCommandRefs.GetHashCode())
            .Add(nonVariantTypes.GetHashCode())
            ;


        public struct BindingPropertySpec : IEquatable<BindingPropertySpec>
        {
            public string methodName;
            public string paramFullTypeName;
            public string variantConverterPropertyName;
            public RefKind paramRefKind;
            public bool isParameterTypeVariant;
            public bool skipBindingProperty;
            public bool skipConverter;

            public EquatableArray<AttributeInfo> forwardedFieldAttributes;

            public readonly bool Equals(BindingPropertySpec other)
                => string.Equals(methodName, other.methodName, StringComparison.Ordinal)
                && string.Equals(paramFullTypeName, other.paramFullTypeName, StringComparison.Ordinal)
                && string.Equals(variantConverterPropertyName, other.variantConverterPropertyName, StringComparison.Ordinal)
                && paramRefKind == other.paramRefKind
                && isParameterTypeVariant == other.isParameterTypeVariant
                && skipBindingProperty == other.skipBindingProperty
                && skipConverter == other.skipConverter
                ;

            public readonly override bool Equals(object obj)
                => obj is BindingPropertySpec other && Equals(other);

            public readonly override int GetHashCode()
                => HashValue.Combine(methodName, paramFullTypeName, variantConverterPropertyName)
                .Add(paramRefKind)
                .Add(isParameterTypeVariant)
                .Add(skipBindingProperty)
                .Add(skipConverter)
                ;
        }

        public struct BindingCommandSpec : IEquatable<BindingCommandSpec>
        {
            public string methodName;
            public string paramFullTypeName;
            public string paramName;
            public RefKind paramRefKind;
            public bool skipBindingCommand;

            public EquatableArray<AttributeInfo> forwardedFieldAttributes;

            public readonly bool Equals(BindingCommandSpec other)
                => string.Equals(methodName, other.methodName, StringComparison.Ordinal)
                && string.Equals(paramFullTypeName, other.paramFullTypeName, StringComparison.Ordinal)
                && string.Equals(paramName, other.paramName, StringComparison.Ordinal)
                && paramRefKind == other.paramRefKind
                && skipBindingCommand == other.skipBindingCommand
                ;

            public readonly override bool Equals(object obj)
                => obj is BindingCommandSpec other && Equals(other);

            public readonly override int GetHashCode()
                => HashValue.Combine(methodName, paramFullTypeName, paramName)
                .Add(paramRefKind)
                .Add(skipBindingCommand)
                ;
        }

        public struct NonVariantTypeSpec : IEquatable<NonVariantTypeSpec>
        {
            public string fullTypeName;
            public string converterPropertyName;

            public readonly bool Equals(NonVariantTypeSpec other)
                => string.Equals(fullTypeName, other.fullTypeName, StringComparison.Ordinal)
                && string.Equals(converterPropertyName, other.converterPropertyName, StringComparison.Ordinal)
                ;

            public readonly override bool Equals(object obj)
                => obj is NonVariantTypeSpec other && Equals(other);

            public readonly override int GetHashCode()
                => HashValue.Combine(fullTypeName, converterPropertyName);
        }


        private struct BinderPropertyScanEntry
        {
            public IMethodSymbol method;
            public string paramFullTypeName;
            public string variantConverterPropertyName;
            public RefKind paramRefKind;
            public bool isParameterTypeVariant;
        }

        private struct BinderCommandScanEntry
        {
            public IMethodSymbol method;
            public string paramFullTypeName;
            public string paramName;
            public RefKind paramRefKind;
        }
    }
}

