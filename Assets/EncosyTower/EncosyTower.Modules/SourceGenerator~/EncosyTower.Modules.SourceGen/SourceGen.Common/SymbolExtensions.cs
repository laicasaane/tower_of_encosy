using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

// Everything in this file was copied from Unity's source generators.
namespace EncosyTower.Modules.SourceGen
{
    public static class SymbolExtensions
    {
        private static SymbolDisplayFormat SimpleFormat { get; }
            = new SymbolDisplayFormat(
                typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameOnly,
                globalNamespaceStyle: SymbolDisplayGlobalNamespaceStyle.Omitted,
                genericsOptions: SymbolDisplayGenericsOptions.IncludeTypeParameters,
                miscellaneousOptions:
                SymbolDisplayMiscellaneousOptions.EscapeKeywordIdentifiers |
                SymbolDisplayMiscellaneousOptions.UseSpecialTypes
            );

        public static SymbolDisplayFormat MemberNameFormat { get; }
            = new SymbolDisplayFormat(
                typeQualificationStyle:
                    SymbolDisplayTypeQualificationStyle.NameOnly,
                genericsOptions:
                    SymbolDisplayGenericsOptions.None,
                memberOptions:
                    SymbolDisplayMemberOptions.None,
                parameterOptions:
                    SymbolDisplayParameterOptions.None,
                miscellaneousOptions:
                    SymbolDisplayMiscellaneousOptions.None
            );

        private static SymbolDisplayFormat QualifiedFormat { get; }
            = new SymbolDisplayFormat(
                typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces,
                globalNamespaceStyle: SymbolDisplayGlobalNamespaceStyle.Included,
                genericsOptions: SymbolDisplayGenericsOptions.IncludeTypeParameters,
                miscellaneousOptions:
                SymbolDisplayMiscellaneousOptions.EscapeKeywordIdentifiers |
                SymbolDisplayMiscellaneousOptions.UseSpecialTypes
            );

        private static SymbolDisplayFormat QualifiedFormatWithoutGlobalPrefix { get; }
            = new SymbolDisplayFormat(
                typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces,
                genericsOptions: SymbolDisplayGenericsOptions.IncludeTypeParameters,
                miscellaneousOptions:
                SymbolDisplayMiscellaneousOptions.EscapeKeywordIdentifiers |
                SymbolDisplayMiscellaneousOptions.UseSpecialTypes
            );

        private static SymbolDisplayFormat QualifiedFormatWithoutSpecialTypeNames { get; }
            = new SymbolDisplayFormat(
                typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces,
                genericsOptions: SymbolDisplayGenericsOptions.IncludeTypeParameters,
                miscellaneousOptions:
                SymbolDisplayMiscellaneousOptions.EscapeKeywordIdentifiers
            );

        public static SymbolDisplayFormat QualifiedMemberFormatWithType { get; }
            = new SymbolDisplayFormat(
                typeQualificationStyle:
                    SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces,
                genericsOptions:
                    SymbolDisplayGenericsOptions.IncludeTypeParameters,
                memberOptions:
                    SymbolDisplayMemberOptions.IncludeType |
                    SymbolDisplayMemberOptions.IncludeParameters |
                    SymbolDisplayMemberOptions.IncludeExplicitInterface,
                parameterOptions:
                    SymbolDisplayParameterOptions.IncludeType |
                    SymbolDisplayParameterOptions.IncludeParamsRefOut,
                miscellaneousOptions:
                    SymbolDisplayMiscellaneousOptions.EscapeKeywordIdentifiers |
                    SymbolDisplayMiscellaneousOptions.UseSpecialTypes
            );

        public static SymbolDisplayFormat QualifiedMemberFormatWithGlobalPrefix { get; }
            = new SymbolDisplayFormat(
                typeQualificationStyle:
                    SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces,
                genericsOptions:
                    SymbolDisplayGenericsOptions.IncludeTypeParameters,
                memberOptions:
                    SymbolDisplayMemberOptions.IncludeParameters |
                    SymbolDisplayMemberOptions.IncludeExplicitInterface,
                parameterOptions:
                    SymbolDisplayParameterOptions.IncludeType |
                    SymbolDisplayParameterOptions.IncludeParamsRefOut,
                globalNamespaceStyle:
                    SymbolDisplayGlobalNamespaceStyle.Included,
                miscellaneousOptions:
                    SymbolDisplayMiscellaneousOptions.EscapeKeywordIdentifiers |
                    SymbolDisplayMiscellaneousOptions.UseSpecialTypes
            );

        public static SymbolDisplayFormat QualifiedMemberNameWithGlobalPrefix { get; }
            = new SymbolDisplayFormat(
                typeQualificationStyle:
                    SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces,
                genericsOptions:
                    SymbolDisplayGenericsOptions.IncludeTypeParameters,
                memberOptions:
                    SymbolDisplayMemberOptions.IncludeExplicitInterface,
                parameterOptions:
                    SymbolDisplayParameterOptions.None,
                globalNamespaceStyle:
                    SymbolDisplayGlobalNamespaceStyle.Included,
                miscellaneousOptions:
                    SymbolDisplayMiscellaneousOptions.EscapeKeywordIdentifiers |
                    SymbolDisplayMiscellaneousOptions.UseSpecialTypes
            );

        public static bool Is(this ITypeSymbol symbol, string fullyQualifiedName, bool checkBaseType = true)
        {
            fullyQualifiedName = PrependGlobalIfMissing(fullyQualifiedName);

            if (symbol is null)
                return false;

            if (symbol.ToDisplayString(QualifiedFormat) == fullyQualifiedName)
                return true;

            return checkBaseType && symbol.BaseType.Is(fullyQualifiedName);
        }

        public static IEnumerable<string> GetAllFullyQualifiedInterfaceAndBaseTypeNames(this ITypeSymbol symbol)
        {
            if (symbol.BaseType != null)
            {
                var baseTypeName = symbol.BaseType.ToDisplayString(QualifiedFormat);
                if (baseTypeName != "global::System.ValueType")
                    yield return baseTypeName;
            }

            foreach (var @interface in symbol.Interfaces)
                yield return @interface.ToDisplayString(QualifiedFormat);
        }

        public static bool IsInt(this ITypeSymbol symbol)
            => symbol.SpecialType == SpecialType.System_Int32;

        public static bool IsDynamicBuffer(this ITypeSymbol symbol)
            => symbol.Name == "DynamicBuffer"
            && symbol.ContainingNamespace.ToDisplayString(QualifiedFormat) == "global::Unity.Entities";

        public static bool IsSharedComponent(this ITypeSymbol symbol)
            => symbol.InheritsFromInterface("Unity.Entities.ISharedComponentData");

        public static bool IsComponent(this ITypeSymbol symbol)
            => symbol.InheritsFromInterface("Unity.Entities.IComponentData");

        public static bool IsZeroSizedComponent(this ITypeSymbol symbol, HashSet<ITypeSymbol> seenSymbols = null)
        {
            seenSymbols ??= new HashSet<ITypeSymbol>(SymbolEqualityComparer.Default) { symbol };

            foreach (var field in symbol.GetMembers().OfType<IFieldSymbol>())
            {
                switch (symbol.SpecialType)
                {
                    case SpecialType.System_Void:
                        continue;

                    case SpecialType.None:
                        if (field.IsStatic || field.IsConst)
                            continue;

                        if (field.Type.TypeKind == TypeKind.Struct)
                        {
                            // Handle cycles in type (otherwise we will stack overflow)
                            if (!seenSymbols.Add(field.Type))
                                continue;

                            if (IsZeroSizedComponent(field.Type))
                                continue;
                        }
                        return false;

                    default:
                        return false;
                }
            }
            return true;
        }

        public static void GetUnmanagedSize(this ITypeSymbol symbol, ref int size)
        {
            if (symbol == null)
            {
                return;
            }

            if (symbol.IsReferenceType)
            {
                size += sizeof(ulong);
                return;
            }

            switch (symbol.SpecialType)
            {
                case SpecialType.System_Char:
                {
                    size += sizeof(char);
                    return;
                }

                case SpecialType.System_Boolean:
                case SpecialType.System_SByte:
                case SpecialType.System_Byte:
                {
                    size += sizeof(byte);
                    return;
                }

                case SpecialType.System_Int16:
                case SpecialType.System_UInt16:
                {
                    size += sizeof(ushort);
                    return;
                }

                case SpecialType.System_Int32:
                case SpecialType.System_UInt32:
                case SpecialType.System_Single:
                {
                    size += sizeof(uint);
                    return;
                }

                case SpecialType.System_Int64:
                case SpecialType.System_UInt64:
                case SpecialType.System_Double:
                case SpecialType.System_DateTime:
                case SpecialType.System_IntPtr:
                case SpecialType.System_UIntPtr:
                {
                    size += sizeof(ulong);
                    return;
                }

                case SpecialType.System_Decimal:
                {
                    size += sizeof(decimal);
                    return;
                }

                default:
                {
                    if (symbol is INamedTypeSymbol namedTypeSymbol && namedTypeSymbol.EnumUnderlyingType != null)
                    {
                        GetUnmanagedSize(namedTypeSymbol.EnumUnderlyingType, ref size);
                    }
                    else if (symbol.IsUnmanagedType)
                    {
                        foreach (var field in symbol.GetMembers().OfType<IFieldSymbol>())
                        {
                            if (field.IsStatic == false && field.IsConst == false)
                            {
                                GetUnmanagedSize(field.Type, ref size);
                            }
                        }
                    }

                    return;
                }
            }
        }

        public static bool IsEnableableComponent(this ITypeSymbol symbol)
               => symbol.InheritsFromInterface("Unity.Entities.IEnableableComponent");

        public static string ToFullName(this ITypeSymbol symbol)
            => symbol.ToDisplayString(QualifiedFormat);

        public static string ToSimpleName(this ITypeSymbol symbol)
            => symbol.ToDisplayString(QualifiedFormatWithoutGlobalPrefix);

        public static string ToValidIdentifier(this ITypeSymbol symbol)
            => symbol.ToDisplayString(QualifiedFormatWithoutGlobalPrefix).ToValidIdentifier();

        public static string ToSimpleValidIdentifier(this ITypeSymbol symbol)
            => symbol.ToDisplayString(SimpleFormat).ToValidIdentifier();

        public static bool ImplementsInterface(this ISymbol symbol, string interfaceName)
        {
            interfaceName = PrependGlobalIfMissing(interfaceName);

            return symbol is ITypeSymbol typeSymbol
                && typeSymbol.AllInterfaces.Any(i => i.ToFullName() == interfaceName || i.InheritsFromInterface(interfaceName));
        }

        public static bool Is(this ITypeSymbol symbol, string nameSpace, string typeName, bool checkBaseType = true)
        {
            if (symbol is null)
                return false;

            if (symbol.Name == typeName && symbol.ContainingNamespace?.Name == nameSpace)
                return true;

            return checkBaseType && symbol.BaseType.Is(nameSpace, typeName);
        }

        public static ITypeSymbol GetSymbolType(this ISymbol symbol)
        {
            return symbol switch {
                ILocalSymbol localSymbol => localSymbol.Type,
                IParameterSymbol parameterSymbol => parameterSymbol.Type,
                INamedTypeSymbol namedTypeSymbol => namedTypeSymbol,
                IMethodSymbol methodSymbol => methodSymbol.ContainingType,
                IPropertySymbol propertySymbol => propertySymbol.ContainingType,
                _ => throw new InvalidOperationException($"Unknown typeSymbol type {symbol.GetType()}")
            };
        }

        public static bool InheritsFromInterface(this ITypeSymbol symbol, string interfaceName, bool checkBaseType = true)
        {
            if (symbol is null)
                return false;

            interfaceName = PrependGlobalIfMissing(interfaceName);

            foreach (var @interface in symbol.Interfaces)
            {
                if (@interface.ToDisplayString(QualifiedFormat) == interfaceName)
                    return true;

                if (checkBaseType)
                {
                    foreach (var baseInterface in @interface.AllInterfaces)
                    {
                        if (baseInterface.ToDisplayString(QualifiedFormat) == interfaceName)
                            return true;

                        if (baseInterface.InheritsFromInterface(interfaceName))
                            return true;
                    }
                }
            }

            if (checkBaseType && symbol.BaseType != null)
            {
                if (symbol.BaseType.InheritsFromInterface(interfaceName))
                    return true;
            }

            return false;
        }

        public static bool InheritsFromType(this ITypeSymbol symbol, string typeName, bool checkBaseType = true)
        {
            typeName = PrependGlobalIfMissing(typeName);

            if (symbol is null)
                return false;

            if (symbol.ToDisplayString(QualifiedFormat) == typeName)
                return true;

            if (checkBaseType && symbol.BaseType != null)
            {
                if (symbol.BaseType.InheritsFromType(typeName))
                    return true;
            }

            return false;
        }

        public static bool HasAttributeSimple(this ISymbol typeSymbol, string attributeName)
        {
            return typeSymbol.GetAttributes()
                .Any(attribute => attribute.AttributeClass.Name == attributeName);
        }

        public static bool HasAttribute(this ISymbol typeSymbol, string fullyQualifiedAttributeName)
        {
            fullyQualifiedAttributeName = PrependGlobalIfMissing(fullyQualifiedAttributeName);

            return typeSymbol.GetAttributes()
                .Any(attribute => attribute.AttributeClass.ToFullName() == fullyQualifiedAttributeName);
        }

        public static AttributeData GetAttribute(this ISymbol typeSymbol, string fullyQualifiedAttributeName)
        {
            fullyQualifiedAttributeName = PrependGlobalIfMissing(fullyQualifiedAttributeName);

            return typeSymbol.GetAttributes()
                .Where(attribute => attribute.AttributeClass.ToFullName() == fullyQualifiedAttributeName)
                .FirstOrDefault();
        }

        public static IEnumerable<AttributeData> GetAttributes(this ISymbol typeSymbol, string fullyQualifiedAttributeName)
        {
            fullyQualifiedAttributeName = PrependGlobalIfMissing(fullyQualifiedAttributeName);

            return typeSymbol.GetAttributes()
                .Where(attribute => attribute.AttributeClass.ToFullName() == fullyQualifiedAttributeName);
        }

        public static IEnumerable<AttributeData> GetAttributes(this ISymbol typeSymbol
            , string fullyQualifiedAttributeName1
            , string fullyQualifiedAttributeName2
        )
        {
            fullyQualifiedAttributeName1 = PrependGlobalIfMissing(fullyQualifiedAttributeName1);
            fullyQualifiedAttributeName2 = PrependGlobalIfMissing(fullyQualifiedAttributeName2);

            return typeSymbol.GetAttributes()
                .Where(attribute => {
                    var fullName = attribute.AttributeClass.ToFullName();
                    return fullName == fullyQualifiedAttributeName1 || fullName == fullyQualifiedAttributeName2;
                });
        }

        public static bool HasAttributeOrFieldWithAttribute(this ITypeSymbol typeSymbol, string fullyQualifiedAttributeName)
        {
            fullyQualifiedAttributeName = PrependGlobalIfMissing(fullyQualifiedAttributeName);

            return typeSymbol.HasAttribute(fullyQualifiedAttributeName)
                || typeSymbol.GetMembers().OfType<IFieldSymbol>()
                    .Any(f => !f.IsStatic && f.Type.HasAttributeOrFieldWithAttribute(fullyQualifiedAttributeName));
        }

        public static string GetMethodAndParamsAsString(this IMethodSymbol methodSymbol)
        {
            var strBuilder = new StringBuilder();
            strBuilder.Append(methodSymbol.Name);

            for (var typeIndex = 0; typeIndex < methodSymbol.TypeParameters.Length; typeIndex++)
                strBuilder.Append($"_T{typeIndex}");

            foreach (var param in methodSymbol.Parameters)
            {
                if (param.RefKind != RefKind.None)
                    strBuilder.Append($"_{param.RefKind.ToString().ToLower()}");
                strBuilder.Append($"_{param.Type.ToDisplayString(QualifiedFormatWithoutSpecialTypeNames).Replace(" ", string.Empty)}");
            }

            return strBuilder.ToString();
        }

        public static bool IsAspect(this ITypeSymbol typeSymbol)
            => typeSymbol.InheritsFromInterface("Unity.Entities.IAspect");

        public static TypedConstantKind GetTypedConstantKind(this ITypeSymbol type)
        {
            switch (type.SpecialType)
            {
                case SpecialType.System_Boolean:
                case SpecialType.System_SByte:
                case SpecialType.System_Int16:
                case SpecialType.System_Int32:
                case SpecialType.System_Int64:
                case SpecialType.System_Byte:
                case SpecialType.System_UInt16:
                case SpecialType.System_UInt32:
                case SpecialType.System_UInt64:
                case SpecialType.System_Single:
                case SpecialType.System_Double:
                case SpecialType.System_Char:
                case SpecialType.System_String:
                case SpecialType.System_Object:
                    return TypedConstantKind.Primitive;

                default:
                    switch (type.TypeKind)
                    {
                        case TypeKind.Array:
                            return TypedConstantKind.Array;
                        case TypeKind.Enum:
                            return TypedConstantKind.Enum;
                        case TypeKind.Error:
                            return TypedConstantKind.Error;
                    }
                    return TypedConstantKind.Type;
            }
        }

        private static string PrependGlobalIfMissing(this string typeOrNamespaceName)
            => typeOrNamespaceName.StartsWith("global::") == false
            ? $"global::{typeOrNamespaceName}"
            : typeOrNamespaceName;

        /// <summary>
        /// Checks whether or not a given symbol has an attribute with the specified fully qualified metadata name.
        /// </summary>
        /// <param name="symbol">The input <see cref="ISymbol"/> instance to check.</param>
        /// <param name="typeSymbol">The <see cref="ITypeSymbol"/> instance for the attribute type to look for.</param>
        /// <returns>Whether or not <paramref name="symbol"/> has an attribute with the specified type.</returns>
        public static bool HasAttributeWithType(this ISymbol symbol, ITypeSymbol typeSymbol)
        {
            foreach (AttributeData attribute in symbol.GetAttributes())
            {
                if (SymbolEqualityComparer.Default.Equals(attribute.AttributeClass, typeSymbol))
                {
                    return true;
                }
            }

            return false;
        }

        public static bool TryGetGenericType(
              this INamedTypeSymbol symbol
            , string startWith
            , int genericArgumentCount
            , out INamedTypeSymbol result
        )
        {
            var baseType = symbol;

            while (baseType != null)
            {
                if (baseType.ToFullName().StartsWith(startWith)
                    && baseType.TypeArguments.Length == genericArgumentCount
                )
                {
                    result = baseType;
                    return true;
                }

                baseType = baseType.BaseType;
            }

            result = null;
            return false;
        }

        public static bool TryGetGenericType(
              this INamedTypeSymbol symbol
            , string startWith
            , int genericArgumentCount1
            , int genericArgumentCount2
            , out INamedTypeSymbol result
        )
        {
            var baseType = symbol;

            while (baseType != null)
            {
                var typeArguments = baseType.TypeArguments;

                if (typeArguments.Length == genericArgumentCount1
                    || typeArguments.Length == genericArgumentCount2
                )
                {
                    if (baseType.ToFullName().StartsWith(startWith))
                    {
                        result = baseType;
                        return true;
                    }
                }

                baseType = baseType.BaseType;
            }

            result = null;
            return false;
        }

        /// <summary>
        /// Gathers all forwarded attributes for the generated field and property.
        /// </summary>
        /// <param name="methodSymbol">The input <see cref="IMethodSymbol"/> instance to process.</param>
        /// <param name="semanticModel">The <see cref="SemanticModel"/> instance for the current run.</param>
        /// <param name="token">The cancellation token for the current operation.</param>
        /// <param name="diagnostics">The current collection of gathered diagnostics.</param>
        /// <param name="fieldAttributes">The resulting field attributes to forward.</param>
        /// <param name="propertyAttributes">The resulting property attributes to forward.</param>
        public static void GatherForwardedAttributes(
              this IMethodSymbol methodSymbol
            , SemanticModel semanticModel
            , CancellationToken token
            , bool includePartialImplementationPart
            , in ImmutableArrayBuilder<DiagnosticInfo> diagnostics
            , out ImmutableArray<AttributeInfo> fieldAttributes
            , out ImmutableArray<AttributeInfo> propertyAttributes
            , DiagnosticDescriptor diagnostic
        )
        {
            using var fieldAttributesInfo = ImmutableArrayBuilder<AttributeInfo>.Rent();
            using var propertyAttributesInfo = ImmutableArrayBuilder<AttributeInfo>.Rent();

            if (methodSymbol is { IsPartialDefinition: true } or { PartialDefinitionPart: not null })
            {
                IMethodSymbol partialDefinition = methodSymbol.PartialDefinitionPart ?? methodSymbol;

                GatherForwardedAttributes(
                      partialDefinition
                    , semanticModel
                    , token
                    , in diagnostics
                    , in fieldAttributesInfo
                    , in propertyAttributesInfo
                    , diagnostic
                );

                if (includePartialImplementationPart)
                {
                    IMethodSymbol partialImplementation = methodSymbol.PartialImplementationPart ?? methodSymbol;

                    GatherForwardedAttributes(
                          partialImplementation
                        , semanticModel
                        , token
                        , in diagnostics
                        , in fieldAttributesInfo
                        , in propertyAttributesInfo
                        , diagnostic
                    );
                }
            }
            else
            {
                GatherForwardedAttributes(
                      methodSymbol
                    , semanticModel
                    , token
                    , in diagnostics
                    , in fieldAttributesInfo
                    , in propertyAttributesInfo
                    , diagnostic
                );
            }

            fieldAttributes = fieldAttributesInfo.ToImmutable();
            propertyAttributes = propertyAttributesInfo.ToImmutable();

            static void GatherForwardedAttributes(
                  IMethodSymbol symbol
                , SemanticModel semanticModel
                , CancellationToken token
                , in ImmutableArrayBuilder<DiagnosticInfo> diagnostics
                , in ImmutableArrayBuilder<AttributeInfo> fieldAttributesInfo
                , in ImmutableArrayBuilder<AttributeInfo> propertyAttributesInfo
                , DiagnosticDescriptor diagnostic
            )
            {
                if (symbol.DeclaringSyntaxReferences.Length != 1
                    || symbol.DeclaringSyntaxReferences[0] is not SyntaxReference syntaxReference
                )
                {
                    return;
                }

                if (syntaxReference.GetSyntax(token) is not MethodDeclarationSyntax methodDeclaration)
                {
                    return;
                }

                foreach (AttributeListSyntax attributeList in methodDeclaration.AttributeLists)
                {
                    if (attributeList.Target == null
                        || attributeList.Target.Identifier.Kind() is not (SyntaxKind.PropertyKeyword or SyntaxKind.FieldKeyword)
                    )
                    {
                        continue;
                    }

                    foreach (AttributeSyntax attribute in attributeList.Attributes)
                    {
                        if (!semanticModel.GetSymbolInfo(attribute, token)
                            .TryGetAttributeTypeSymbol(out INamedTypeSymbol attributeTypeSymbol)
                        )
                        {
                            diagnostics.Add(diagnostic, attribute, symbol, attribute.Name);
                            continue;
                        }

                        var attributeInfo = AttributeInfo.From(
                              attributeTypeSymbol
                            , semanticModel
                            , attribute.ArgumentList?.Arguments ?? Enumerable.Empty<AttributeArgumentSyntax>()
                            , token
                        );

                        if (attributeList.Target != null)
                        {
                            if (attributeList.Target.Identifier.IsKind(SyntaxKind.FieldKeyword))
                            {
                                fieldAttributesInfo.Add(attributeInfo);
                            }
                            else if (attributeList.Target.Identifier.IsKind(SyntaxKind.PropertyKeyword))
                            {
                                propertyAttributesInfo.Add(attributeInfo);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Gathers all forwarded attributes for the generated property.
        /// </summary>
        /// <param name="fieldSymbol">The input <see cref="IMethodSymbol"/> instance to process.</param>
        /// <param name="semanticModel">The <see cref="SemanticModel"/> instance for the current run.</param>
        /// <param name="token">The cancellation token for the current operation.</param>
        /// <param name="diagnostics">The current collection of gathered diagnostics.</param>
        /// <param name="fieldAttributes">The resulting field attributes to forward.</param>
        /// <param name="propertyAttributes">The resulting property attributes to forward.</param>
        public static void GatherForwardedAttributes(
              this IFieldSymbol fieldSymbol
            , SemanticModel semanticModel
            , CancellationToken token
            , in ImmutableArrayBuilder<DiagnosticInfo> diagnostics
            , out ImmutableArray<AttributeInfo> propertyAttributes
            , DiagnosticDescriptor diagnostic
        )
        {
            using var propertyAttributesInfo = ImmutableArrayBuilder<AttributeInfo>.Rent();

            GatherForwardedAttributes(
                  fieldSymbol
                , semanticModel
                , token
                , in diagnostics
                , in propertyAttributesInfo
                , diagnostic
            );

            propertyAttributes = propertyAttributesInfo.ToImmutable();

            static void GatherForwardedAttributes(
                  IFieldSymbol symbol
                , SemanticModel semanticModel
                , CancellationToken token
                , in ImmutableArrayBuilder<DiagnosticInfo> diagnostics
                , in ImmutableArrayBuilder<AttributeInfo> propertyAttributesInfo
                , DiagnosticDescriptor diagnostic
            )
            {
                if (symbol.DeclaringSyntaxReferences.Length != 1
                    || symbol.DeclaringSyntaxReferences[0] is not SyntaxReference syntaxReference
                )
                {
                    return;
                }

                var syntax = syntaxReference.GetSyntax(token);

                if (syntax.Parent?.Parent is not FieldDeclarationSyntax fieldDeclaration)
                {
                    return;
                }

                foreach (AttributeListSyntax attributeList in fieldDeclaration.AttributeLists)
                {
                    if (attributeList.Target == null
                        || attributeList.Target.Identifier.Kind() is not SyntaxKind.PropertyKeyword
                    )
                    {
                        continue;
                    }

                    foreach (AttributeSyntax attribute in attributeList.Attributes)
                    {
                        if (!semanticModel.GetSymbolInfo(attribute, token)
                                .TryGetAttributeTypeSymbol(out INamedTypeSymbol attributeTypeSymbol)
                        )
                        {
                            diagnostics.Add(diagnostic, attribute, symbol, attribute.Name);
                            continue;
                        }

                        var attributeInfo = AttributeInfo.From(
                              attributeTypeSymbol
                            , semanticModel
                            , attribute.ArgumentList?.Arguments ?? Enumerable.Empty<AttributeArgumentSyntax>()
                            , token
                        );

                        // Add the new attribute info to the right builder
                        if (attributeList.Target != null)
                        {
                            if (attributeList.Target.Identifier.IsKind(SyntaxKind.PropertyKeyword))
                            {
                                propertyAttributesInfo.Add(attributeInfo);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Gathers all forwarded attributes for the generated field.
        /// </summary>
        /// <param name="fieldSymbol">The input <see cref="IMethodSymbol"/> instance to process.</param>
        /// <param name="semanticModel">The <see cref="SemanticModel"/> instance for the current run.</param>
        /// <param name="token">The cancellation token for the current operation.</param>
        /// <param name="diagnostics">The current collection of gathered diagnostics.</param>
        /// <param name="fieldAttributes">The resulting field attributes to forward.</param>
        public static void GatherForwardedAttributes(
              this IPropertySymbol fieldSymbol
            , SemanticModel semanticModel
            , CancellationToken token
            , in ImmutableArrayBuilder<DiagnosticInfo> diagnostics
            , out ImmutableArray<AttributeInfo> fieldAttributes
            , DiagnosticDescriptor diagnostic
        )
        {
            using var fieldAttributesInfo = ImmutableArrayBuilder<AttributeInfo>.Rent();

            GatherForwardedAttributes(
                  fieldSymbol
                , semanticModel
                , token
                , in diagnostics
                , in fieldAttributesInfo
                , diagnostic
            );

            fieldAttributes = fieldAttributesInfo.ToImmutable();

            static void GatherForwardedAttributes(
                  IPropertySymbol symbol
                , SemanticModel semanticModel
                , CancellationToken token
                , in ImmutableArrayBuilder<DiagnosticInfo> diagnostics
                , in ImmutableArrayBuilder<AttributeInfo> fieldAttributesInfo
                , DiagnosticDescriptor diagnostic
            )
            {
                if (symbol.DeclaringSyntaxReferences.Length != 1
                    || symbol.DeclaringSyntaxReferences[0] is not SyntaxReference syntaxReference
                )
                {
                    return;
                }

                var syntax = syntaxReference.GetSyntax(token);

                if (syntax.Parent?.Parent is not PropertyDeclarationSyntax propDeclaration)
                {
                    return;
                }

                foreach (AttributeListSyntax attributeList in propDeclaration.AttributeLists)
                {
                    if (attributeList.Target == null
                        || attributeList.Target.Identifier.Kind() is not SyntaxKind.FieldKeyword
                    )
                    {
                        continue;
                    }

                    foreach (AttributeSyntax attribute in attributeList.Attributes)
                    {
                        if (!semanticModel.GetSymbolInfo(attribute, token)
                                .TryGetAttributeTypeSymbol(out INamedTypeSymbol attributeTypeSymbol)
                        )
                        {
                            diagnostics.Add(diagnostic, attribute, symbol, attribute.Name);
                            continue;
                        }

                        var attributeInfo = AttributeInfo.From(
                              attributeTypeSymbol
                            , semanticModel
                            , attribute.ArgumentList?.Arguments ?? Enumerable.Empty<AttributeArgumentSyntax>()
                            , token
                        );

                        // Add the new attribute info to the right builder
                        if (attributeList.Target != null)
                        {
                            if (attributeList.Target.Identifier.IsKind(SyntaxKind.FieldKeyword))
                            {
                                fieldAttributesInfo.Add(attributeInfo);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Gathers all forwarded attributes for the generated field.
        /// </summary>
        /// <param name="fieldSymbol">The input <see cref="IMethodSymbol"/> instance to process.</param>
        /// <param name="semanticModel">The <see cref="SemanticModel"/> instance for the current run.</param>
        /// <param name="token">The cancellation token for the current operation.</param>
        /// <param name="diagnostics">The current collection of gathered diagnostics.</param>
        /// <param name="fieldAttributes">The resulting field attributes to forward.</param>
        /// <param name="fieldAttributes">The resulting property attributes to forward.</param>
        public static void GatherForwardedAttributes(
              this IPropertySymbol fieldSymbol
            , SemanticModel semanticModel
            , CancellationToken token
            , in ImmutableArrayBuilder<DiagnosticInfo> diagnostics
            , out ImmutableArray<(string, AttributeInfo)> fieldAttributes
            , DiagnosticDescriptor diagnostic
        )
        {
            using var fieldAttributesInfo = ImmutableArrayBuilder<(string, AttributeInfo)>.Rent();

            GatherForwardedAttributes(
                  fieldSymbol
                , semanticModel
                , token
                , in diagnostics
                , in fieldAttributesInfo
                , diagnostic
            );

            fieldAttributes = fieldAttributesInfo.ToImmutable();

            static void GatherForwardedAttributes(
                  IPropertySymbol symbol
                , SemanticModel semanticModel
                , CancellationToken token
                , in ImmutableArrayBuilder<DiagnosticInfo> diagnostics
                , in ImmutableArrayBuilder<(string, AttributeInfo)> fieldAttributesInfo
                , DiagnosticDescriptor diagnostic
            )
            {
                if (symbol.DeclaringSyntaxReferences.Length != 1
                    || symbol.DeclaringSyntaxReferences[0] is not SyntaxReference syntaxReference
                )
                {
                    return;
                }

                var syntax = syntaxReference.GetSyntax(token);

                if (syntax is not PropertyDeclarationSyntax propDeclaration)
                {
                    return;
                }

                foreach (AttributeListSyntax attributeList in propDeclaration.AttributeLists)
                {
                    if (attributeList.Target == null
                        || attributeList.Target.Identifier.Kind() is not SyntaxKind.FieldKeyword
                    )
                    {
                        continue;
                    }

                    foreach (AttributeSyntax attribute in attributeList.Attributes)
                    {
                        if (!semanticModel.GetSymbolInfo(attribute, token)
                                .TryGetAttributeTypeSymbol(out INamedTypeSymbol attributeTypeSymbol)
                        )
                        {
                            diagnostics.Add(diagnostic, attribute, symbol, attribute.Name);
                            continue;
                        }

                        var attributeInfo = AttributeInfo.From(
                              attributeTypeSymbol
                            , semanticModel
                            , attribute.ArgumentList?.Arguments ?? Enumerable.Empty<AttributeArgumentSyntax>()
                            , token
                        );

                        // Add the new attribute info to the right builder
                        if (attributeList.Target != null)
                        {
                            if (attributeList.Target.Identifier.IsKind(SyntaxKind.FieldKeyword))
                            {
                                var typeName = attributeTypeSymbol.ToFullName();
                                fieldAttributesInfo.Add((typeName, attributeInfo));
                            }
                        }
                    }
                }
            }
        }

        public static bool DoesMatchInterface(in this ImmutableArray<INamedTypeSymbol> interfaces, string typeName)
        {
            foreach (var interfaceSymbol in interfaces)
            {
                if (interfaceSymbol.ToFullName().AsSpan().Equals(typeName.AsSpan(), StringComparison.Ordinal))
                {
                    return true;
                }
            }

            return false;
        }
    }
}

