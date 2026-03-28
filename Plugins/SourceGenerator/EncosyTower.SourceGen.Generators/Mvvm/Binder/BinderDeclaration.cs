using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using EncosyTower.SourceGen.Common.Mvvm.Common;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace EncosyTower.SourceGen.Generators.Mvvm.Binders
{
    public partial struct BinderDeclaration : IEquatable<BinderDeclaration>
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

        /// <summary>Excluded from <see cref="Equals"/> and <see cref="GetHashCode"/> —
        /// location is not stable across incremental runs.</summary>
        public LocationInfo location;

        public string openingSource;
        public string closingSource;
        public string hintName;
        public string sourceFilePath;

        /// <summary>Class name including generic type parameters, e.g. <c>MyBinder&lt;T&gt;</c>.</summary>
        public string className;

        /// <summary>Simple class identifier only (no generic params), e.g. <c>MyBinder</c>.
        /// Used for the generated constructor name.</summary>
        public string simpleTypeName;

        /// <summary>Pre-computed <c>symbol.ToValidIdentifier()</c> — used for the listening flag field name.</summary>
        public string typeIdentifier;

        public bool hasBaseBinder;
        public bool hasOnBindPropertyFailedMethod;
        public bool hasOnBindCommandFailedMethod;
        public bool isSealed;

        public EquatableArray<BindingPropertyInfo> bindingPropertyRefs;
        public EquatableArray<BindingCommandInfo> bindingCommandRefs;
        public EquatableArray<NonVariantTypeInfo> nonVariantTypes;

        public readonly bool IsValid
            => string.IsNullOrEmpty(hintName) == false
            && string.IsNullOrEmpty(simpleTypeName) == false;

        /// <summary>
        /// Extracts all binder metadata from the annotated class symbol and syntax into a
        /// fully populated, cache-friendly <see cref="BinderDeclaration"/>.
        /// Called once per class inside the <c>ForAttributeWithMetadataName</c> transform.
        /// </summary>
        public static BinderDeclaration Extract(
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

            using var diagnosticBuilder = ImmutableArrayBuilder<DiagnosticInfo>.Rent();

            // Collect existing field names to detect user-declared backing fields/converters/commands.
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

            // Build className (with generic type parameters if present).
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

            // Determine whether a base class already implements IBinder.
            var hasBaseBinder = false;

            if (symbol.BaseType != null && symbol.BaseType.TypeKind == TypeKind.Class)
            {
                if (symbol.BaseType.HasAttribute(BINDER_ATTRIBUTE)
                    || symbol.BaseType.ImplementsInterface(IBINDER_INTERFACE)
                )
                {
                    hasBaseBinder = true;
                }
            }

            // Walk the inheritance chain checking for user-defined OnBindPropertyFailed /
            // OnBindCommandFailed overrides.
            var hasOnBindPropertyFailedMethod = false;
            var hasOnBindCommandFailedMethod = false;
            var baseType = symbol;
            var isCurrentType = true;

            while (baseType != null)
            {
                foreach (var member in baseType.GetMembers("OnBindPropertyFailed"))
                {
                    if (member is IMethodSymbol checkMethod
                        && isCurrentType == false
                        && checkMethod.DeclaredAccessibility is (Accessibility.Public or Accessibility.Protected)
                        && checkMethod.Parameters.Length == 1
                        && checkMethod.Parameters[0].Type.HasFullName(BINDING_PROPERTY)
                    )
                    {
                        hasOnBindPropertyFailedMethod = true;
                        break;
                    }
                }

                foreach (var member in baseType.GetMembers("OnBindCommandFailed"))
                {
                    if (member is IMethodSymbol checkMethod
                        && isCurrentType == false
                        && checkMethod.DeclaredAccessibility is (Accessibility.Public or Accessibility.Protected)
                        && checkMethod.Parameters.Length == 1
                        && checkMethod.Parameters[0].Type.HasFullName(BINDING_COMMAND)
                    )
                    {
                        hasOnBindCommandFailedMethod = true;
                        break;
                    }
                }

                isCurrentType = false;
                baseType = baseType.BaseType;
            }

            // First pass: scan all members to build field-name sets and collect method symbols.
            var members = symbol.GetMembers();

            // Temporary lists that retain the IMethodSymbol so that GatherForwardedAttributes
            // can be called (it requires the symbol for syntax-tree lookup).
            using var tempPropMethods = ImmutableArrayBuilder<BinderPropertyScanEntry>.Rent();
            using var tempCmdMethods = ImmutableArrayBuilder<BinderCommandScanEntry>.Rent();
            var nonVariantTypeFilter = new Dictionary<string, NonVariantTypeInfo>(members.Length, StringComparer.Ordinal);

            foreach (var member in members)
            {
                if (member is IMethodSymbol method)
                {
                    var bindingPropAttribute = method.GetAttribute(BINDING_PROPERTY_ATTRIBUTE);
                    var bindingCommandAttribute = method.GetAttribute(BINDING_COMMAND_ATTRIBUTE);

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
                            nonVariantTypeFilter[paramFullTypeName] = new NonVariantTypeInfo {
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

            // Second pass: compute skip flags and gather forwarded attributes now that field
            // name sets are complete and we still hold the IMethodSymbol references.
            using var propRefBuilder = ImmutableArrayBuilder<BindingPropertyInfo>.Rent();

            foreach (var entry in tempPropMethods.WrittenSpan)
            {
                var methodName = entry.method.Name;

                entry.method.GatherForwardedAttributes(
                      semanticModel
                    , token
                    , true
                    , diagnosticBuilder
                    , out var fieldAttributes
                    , out _
                    , DiagnosticDescriptors.InvalidFieldTargetedAttributeOnBindingPropertyMethod
                );

                propRefBuilder.Add(new BindingPropertyInfo {
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

            using var cmdRefBuilder = ImmutableArrayBuilder<BindingCommandInfo>.Rent();

            foreach (var entry in tempCmdMethods.WrittenSpan)
            {
                var methodName = entry.method.Name;

                entry.method.GatherForwardedAttributes(
                      semanticModel
                    , token
                    , false
                    , diagnosticBuilder
                    , out var fieldAttributes
                    , out _
                    , DiagnosticDescriptors.InvalidFieldTargetedAttributeOnBindingCommandMethod
                );

                cmdRefBuilder.Add(new BindingCommandInfo {
                    methodName = methodName,
                    paramFullTypeName = entry.paramFullTypeName,
                    paramRefKind = entry.paramRefKind,
                    paramName = entry.paramName,
                    skipBindingCommand = bindingCommandNames.Contains($"_bindingCommandFor{methodName}"),
                    forwardedFieldAttributes = fieldAttributes.AsEquatableArray(),
                });
            }

            using var nonVariantTypesBuilder = ImmutableArrayBuilder<NonVariantTypeInfo>.Rent();
            nonVariantTypesBuilder.AddRange(nonVariantTypeFilter.Values);

            // Pre-compute the hint name and source file path while we have the compilation.
            var syntaxTree = classSyntax.SyntaxTree;
            var fileTypeName = symbol.ToFileName();
            var hintName = syntaxTree.GetGeneratedSourceFileName(
                  BinderGenerator.GENERATOR_NAME
                , classSyntax
                , fileTypeName
            );
            var sourceFilePath = syntaxTree.GetGeneratedSourceFilePath(
                  semanticModel.Compilation.Assembly.Name
                , BinderGenerator.GENERATOR_NAME
                , fileTypeName
            );

            return new BinderDeclaration {
                location = LocationInfo.From(classSyntax.GetLocation()),
                openingSource = openingSource,
                closingSource = closingSource,
                hintName = hintName,
                sourceFilePath = sourceFilePath,
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
            p.PrintLine("using System;");
            p.PrintLine("using System.CodeDom.Compiler;");
            p.PrintLine("using System.Diagnostics;");
            p.PrintLine("using System.Diagnostics.CodeAnalysis;");
            p.PrintLine("using System.Runtime.CompilerServices;");
            p.PrintLine("using System.Runtime.InteropServices;");
            p.PrintLine("using EncosyTower.Common;");
            p.PrintLine("using EncosyTower.Mvvm.ComponentModel;");
            p.PrintLine("using EncosyTower.Mvvm.Input;");
            p.PrintLine("using EncosyTower.Mvvm.ViewBinding;");
            p.PrintLine("using EncosyTower.Mvvm.ViewBinding.SourceGen;");
            p.PrintLine("using EncosyTower.Variants.Converters;");
            p.PrintEndLine();
            p.PrintLine("using BindingProperty = global::EncosyTower.Mvvm.ViewBinding.BindingProperty;");
            p.PrintLine("using Converter = global::EncosyTower.Mvvm.ViewBinding.Converter;");
            p.PrintLine("using EditorBrowsableAttribute = global::System.ComponentModel.EditorBrowsableAttribute;");
            p.PrintLine("using EditorBrowsableState = global::System.ComponentModel.EditorBrowsableState;");
            p.PrintLine("using IBinder = global::EncosyTower.Mvvm.ViewBinding.IBinder;");
            p.PrintLine("using INotifyPropertyChanged = global::EncosyTower.Mvvm.ComponentModel.INotifyPropertyChanged;");
            p.PrintEndLine();
            p.Print("#pragma warning restore CS0105 // Using directive appeared previously in this namespace").PrintEndLine();
            p.PrintEndLine();
        }

        // ── IEquatable<BinderDeclaration> ─────────────────────────────────────────
        // NOTE: `location` is intentionally excluded — not stable across incremental passes.
        // NOTE: `forwardedFieldAttributes` within each ref is intentionally excluded to
        //       avoid potential recursion issues with AttributeInfo.Equals.

        public readonly bool Equals(BinderDeclaration other)
            => string.Equals(openingSource, other.openingSource, StringComparison.Ordinal)
            && string.Equals(closingSource, other.closingSource, StringComparison.Ordinal)
            && string.Equals(hintName, other.hintName, StringComparison.Ordinal)
            && string.Equals(sourceFilePath, other.sourceFilePath, StringComparison.Ordinal)
            && string.Equals(className, other.className, StringComparison.Ordinal)
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
            => obj is BinderDeclaration other && Equals(other);

        public readonly override int GetHashCode()
            => HashValue.Combine(
                  openingSource
                , closingSource
                , hintName
                , sourceFilePath
                , className
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

        // ── Nested cache-friendly data ────────────────────────────────────────────

        /// <summary>
        /// Cache-friendly representation of a method decorated with
        /// <c>[BindingProperty]</c>.
        /// </summary>
        public struct BindingPropertyInfo : IEquatable<BindingPropertyInfo>
        {
            public string methodName;

            /// <summary>Fully-qualified parameter type name, or empty string when no parameter.</summary>
            public string paramFullTypeName;

            /// <summary>
            /// Pre-computed <c>paramType.ToValidIdentifier().MakeFirstCharUpperCase()</c>
            /// used for the <c>_variantConverter{X}</c> field name.
            /// Empty when the parameter is a <c>Variant</c> type or when there is no parameter.
            /// </summary>
            public string variantConverterPropertyName;

            public RefKind paramRefKind;
            public bool isParameterTypeVariant;
            public bool skipBindingProperty;
            public bool skipConverter;

            /// <summary>
            /// Forwarded <c>[field:]</c>-targeted attribute data.
            /// Intentionally excluded from <see cref="Equals"/> and <see cref="GetHashCode"/>
            /// to avoid potential recursion through <see cref="AttributeInfo.Equals"/>.
            /// </summary>
            public EquatableArray<AttributeInfo> forwardedFieldAttributes;

            public readonly bool Equals(BindingPropertyInfo other)
                => string.Equals(methodName, other.methodName, StringComparison.Ordinal)
                && string.Equals(paramFullTypeName, other.paramFullTypeName, StringComparison.Ordinal)
                && string.Equals(variantConverterPropertyName, other.variantConverterPropertyName, StringComparison.Ordinal)
                && paramRefKind == other.paramRefKind
                && isParameterTypeVariant == other.isParameterTypeVariant
                && skipBindingProperty == other.skipBindingProperty
                && skipConverter == other.skipConverter
                ;

            public readonly override bool Equals(object obj)
                => obj is BindingPropertyInfo other && Equals(other);

            public readonly override int GetHashCode()
                => HashValue.Combine(methodName, paramFullTypeName, variantConverterPropertyName)
                .Add(paramRefKind)
                .Add(isParameterTypeVariant)
                .Add(skipBindingProperty)
                .Add(skipConverter)
                ;
        }

        /// <summary>
        /// Cache-friendly representation of a method decorated with
        /// <c>[BindingCommand]</c>.
        /// </summary>
        public struct BindingCommandInfo : IEquatable<BindingCommandInfo>
        {
            public string methodName;

            /// <summary>Fully-qualified parameter type name, or empty string when no parameter.</summary>
            public string paramFullTypeName;

            /// <summary>Parameter variable name, or empty string when no parameter.</summary>
            public string paramName;

            public RefKind paramRefKind;
            public bool skipBindingCommand;

            /// <summary>
            /// Forwarded <c>[field:]</c>-targeted attribute data.
            /// Intentionally excluded from <see cref="Equals"/> and <see cref="GetHashCode"/>
            /// to avoid potential recursion through <see cref="AttributeInfo.Equals"/>.
            /// </summary>
            public EquatableArray<AttributeInfo> forwardedFieldAttributes;

            public readonly bool Equals(BindingCommandInfo other)
                => string.Equals(methodName, other.methodName, StringComparison.Ordinal)
                && string.Equals(paramFullTypeName, other.paramFullTypeName, StringComparison.Ordinal)
                && string.Equals(paramName, other.paramName, StringComparison.Ordinal)
                && paramRefKind == other.paramRefKind
                && skipBindingCommand == other.skipBindingCommand
                ;

            public readonly override bool Equals(object obj)
                => obj is BindingCommandInfo other && Equals(other);

            public readonly override int GetHashCode()
                => HashValue.Combine(methodName, paramFullTypeName, paramName)
                .Add(paramRefKind)
                .Add(skipBindingCommand)
                ;
        }

        /// <summary>
        /// Cache-friendly representation of a non-<c>Variant</c> parameter type that
        /// requires a <c>CachedVariantConverter</c> field to be generated.
        /// </summary>
        public struct NonVariantTypeInfo : IEquatable<NonVariantTypeInfo>
        {
            public string fullTypeName;

            /// <summary>Pre-computed <c>type.ToValidIdentifier().MakeFirstCharUpperCase()</c>
            /// used for the converter field name suffix.</summary>
            public string converterPropertyName;

            public readonly bool Equals(NonVariantTypeInfo other)
                => string.Equals(fullTypeName, other.fullTypeName, StringComparison.Ordinal)
                && string.Equals(converterPropertyName, other.converterPropertyName, StringComparison.Ordinal)
                ;

            public readonly override bool Equals(object obj)
                => obj is NonVariantTypeInfo other && Equals(other);

            public readonly override int GetHashCode()
                => HashValue.Combine(fullTypeName, converterPropertyName);
        }

        // ── Private scan-entry helpers (used only during Extract) ─────────────────

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