using System.Collections.Generic;
using System.Collections.Immutable;
using EncosyTower.Modules.Data.SourceGen;
using EncosyTower.Modules.SourceGen;
using Microsoft.CodeAnalysis;

namespace EncosyTower.Modules.DataAuthoring.SourceGen
{
    public static class Helpers
    {
        public const string GENERATOR_NAME = nameof(DatabaseGenerator);

        public const string IDATA = "global::EncosyTower.Modules.Data.IData";
        public const string DATA_TABLE_ASSET = "global::EncosyTower.Modules.Data.DataTableAsset";
        public const string DATABASE_ATTRIBUTE = "global::EncosyTower.Modules.Data.Authoring.DatabaseAttribute";
        public const string TABLE_ATTRIBUTE = "global::EncosyTower.Modules.Data.Authoring.TableAttribute";
        public const string HORIZONTAL_LIST_ATTRIBUTE = "global::EncosyTower.Modules.Data.Authoring.HorizontalAttribute";

        public const string DATA_PROPERTY_ATTRIBUTE = "global::EncosyTower.Modules.Data.DataPropertyAttribute";
        public const string DATA_CONVERTER_ATTRIBUTE = "global::EncosyTower.Modules.Data.DataConverterAttribute";
        public const string SERIALIZE_FIELD_ATTRIBUTE = "global::UnityEngine.SerializeField";
        public const string JSON_INCLUDE_ATTRIBUTE = "global::System.Text.Json.Serialization.JsonIncludeAttribute";
        public const string JSON_PROPERTY_ATTRIBUTE = "global::Newtonsoft.Json.JsonPropertyAttribute";
        public const string SKIP_ATTRIBUTE = "global::EncosyTower.Modules.Data.Authoring.SkipSourceGenForAssemblyAttribute";

        public const string LIST_TYPE_T = "global::System.Collections.Generic.List<";
        public const string DICTIONARY_TYPE_T = "global::System.Collections.Generic.Dictionary<";
        public const string HASH_SET_TYPE_T = "global::System.Collections.Generic.HashSet<";
        public const string QUEUE_TYPE_T = "global::System.Collections.Generic.Queue<";
        public const string STACK_TYPE_T = "global::System.Collections.Generic.Stack<";
        public const string VERTICAL_LIST_TYPE = "global::Cathei.BakingSheet.VerticalList<";

        public const string IREADONLY_LIST_TYPE_T = "global::System.Collections.Generic.IReadOnlyList<";
        public const string IREADONLY_DICTIONARY_TYPE_T = "global::System.Collections.Generic.IReadOnlyDictionary<";
        public const string READONLY_MEMORY_TYPE_T = "global::System.ReadOnlyMemory<";
        public const string READONLY_SPAN_TYPE_T = "global::System.ReadOnlySpan<";
        public const string MEMORY_TYPE_T = "global::System.Memory<";
        public const string SPAN_TYPE_T = "global::System.Span<";

        public const string GENERATED_PROPERTY_FROM_FIELD = "global::EncosyTower.Modules.Data.SourceGen.GeneratedPropertyFromFieldAttribute";
        public const string GENERATED_FIELD_FROM_PROPERTY = "global::EncosyTower.Modules.Data.SourceGen.GeneratedFieldFromPropertyAttribute";
        public const string AGGRESSIVE_INLINING = "[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]";
        public const string GENERATED_CODE = "[global::System.CodeDom.Compiler.GeneratedCode(\"EncosyTower.Modules.Data.DatabaseGenerator\", \"1.8.2\")]";
        public const string EXCLUDE_COVERAGE = "[global::System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]";

        public static void Process(this MemberRef memberRef)
        {
            var typeRef = memberRef.TypeRef;
            MakeCollectionTypeRef(typeRef);

            var typeMembers = typeRef.Type.GetMembers();
            bool? fieldTypeHasParameterlessConstructor = null;
            var fieldTypeParameterConstructorCount = 0;

            foreach (var typeMember in typeMembers)
            {
                if (fieldTypeHasParameterlessConstructor.HasValue == false
                    && typeMember is IMethodSymbol method
                    && method.MethodKind == MethodKind.Constructor
                )
                {
                    if (method.Parameters.Length == 0)
                    {
                        fieldTypeHasParameterlessConstructor = true;
                    }
                    else
                    {
                        fieldTypeParameterConstructorCount += 1;
                    }
                }
            }

            if (fieldTypeHasParameterlessConstructor.HasValue)
            {
                memberRef.TypeHasParameterlessConstructor = fieldTypeHasParameterlessConstructor.Value;
            }
            else
            {
                memberRef.TypeHasParameterlessConstructor = fieldTypeParameterConstructorCount == 0;
            }
        }

        public static void MakeCollectionTypeRef(this TypeRef typeRef)
        {
            var collectionTypeRef = typeRef.CollectionTypeRef;

            if (typeRef.Type is IArrayTypeSymbol arrayType)
            {
                collectionTypeRef.Kind = CollectionKind.Array;
                collectionTypeRef.ElementType = arrayType.ElementType;
            }
            else if (typeRef.Type is INamedTypeSymbol namedType)
            {
                if (namedType.TryGetGenericType(LIST_TYPE_T, 1, out var listType))
                {
                    collectionTypeRef.Kind = CollectionKind.List;
                    collectionTypeRef.ElementType = listType.TypeArguments[0];
                }
                else if (namedType.TryGetGenericType(DICTIONARY_TYPE_T, 2, out var dictType))
                {
                    collectionTypeRef.Kind = CollectionKind.Dictionary;
                    collectionTypeRef.KeyType = dictType.TypeArguments[0];
                    collectionTypeRef.ElementType = dictType.TypeArguments[1];
                }
                else if (namedType.TryGetGenericType(HASH_SET_TYPE_T, 1, out var hashSetType))
                {
                    collectionTypeRef.Kind = CollectionKind.HashSet;
                    collectionTypeRef.ElementType = hashSetType.TypeArguments[0];
                }
                else if (namedType.TryGetGenericType(QUEUE_TYPE_T, 1, out var queueType))
                {
                    collectionTypeRef.Kind = CollectionKind.Queue;
                    collectionTypeRef.ElementType = queueType.TypeArguments[0];
                }
                else if (namedType.TryGetGenericType(STACK_TYPE_T, 1, out var stackType))
                {
                    collectionTypeRef.Kind = CollectionKind.Stack;
                    collectionTypeRef.ElementType = stackType.TypeArguments[0];
                }
                else if (namedType.TryGetGenericType(READONLY_MEMORY_TYPE_T, 1, out var readMemoryType))
                {
                    collectionTypeRef.Kind = CollectionKind.Array;
                    collectionTypeRef.ElementType = readMemoryType.TypeArguments[0];
                }
                else if (namedType.TryGetGenericType(MEMORY_TYPE_T, 1, out var memoryType))
                {
                    collectionTypeRef.Kind = CollectionKind.Array;
                    collectionTypeRef.ElementType = memoryType.TypeArguments[0];
                }
                else if (namedType.TryGetGenericType(READONLY_SPAN_TYPE_T, 1, out var readSpanType))
                {
                    collectionTypeRef.Kind = CollectionKind.Array;
                    collectionTypeRef.ElementType = readSpanType.TypeArguments[0];
                }
                else if (namedType.TryGetGenericType(SPAN_TYPE_T, 1, out var spanType))
                {
                    collectionTypeRef.Kind = CollectionKind.Array;
                    collectionTypeRef.ElementType = spanType.TypeArguments[0];
                }
                else if (namedType.TryGetGenericType(IREADONLY_LIST_TYPE_T, 1, out var readListType))
                {
                    collectionTypeRef.Kind = CollectionKind.List;
                    collectionTypeRef.ElementType = readListType.TypeArguments[0];
                }
                else if (namedType.TryGetGenericType(IREADONLY_DICTIONARY_TYPE_T, 2, out var readDictType))
                {
                    collectionTypeRef.Kind = CollectionKind.Dictionary;
                    collectionTypeRef.KeyType = readDictType.TypeArguments[0];
                    collectionTypeRef.ElementType = readDictType.TypeArguments[1];
                }
            }
        }

        public static bool TryMakeConverterRef(
              this MemberRef targetRef
            , SourceProductionContext context
            , SyntaxNode outerNode
            , ISymbol targetSymbol
        )
        {
            if (targetSymbol.GetAttribute(DATA_CONVERTER_ATTRIBUTE) is not AttributeData attrib
                || attrib.ConstructorArguments.Length != 1
            )
            {
                return false;
            }

            if (attrib.ConstructorArguments[0].Value is not INamedTypeSymbol converterType)
            {
                context.ReportDiagnostic(
                      ConverterDiagnosticDescriptors.NotTypeOfExpression
                    , attrib.ApplicationSyntaxReference.GetSyntax(context.CancellationToken)
                );
                return false;
            }
            else
            {
                var targetType = targetRef.TypeRef.Type;
                var syntaxRef = attrib.ApplicationSyntaxReference;

                if (TryGetConvertMethod(outerNode, converterType, out var convertMethod, targetType, context, syntaxRef) == false)
                {
                    return false;
                }

                MakeConverterRef(targetRef.ConverterRef, converterType, convertMethod, targetType);

                return true;
            }
        }

        private static bool TryGetConvertMethod(
              SyntaxNode outerNode
            , INamedTypeSymbol converterType
            , out IMethodSymbol convertMethod
            , ITypeSymbol returnType = null
            , SourceProductionContext? context = null
            , SyntaxReference syntaxRef = null
        )
        {
            if (converterType.IsAbstract)
            {
                context?.ReportDiagnostic(
                      ConverterDiagnosticDescriptors.AbstractTypeNotSupported
                    , syntaxRef?.GetSyntax(context?.CancellationToken ?? default) ?? outerNode
                    , converterType.Name
                );

                convertMethod = null;
                return false;
            }

            if (converterType.IsUnboundGenericType)
            {
                context?.ReportDiagnostic(
                      ConverterDiagnosticDescriptors.OpenGenericTypeNotSupported
                    , syntaxRef?.GetSyntax(context?.CancellationToken ?? default) ?? outerNode
                    , converterType.Name
                );

                convertMethod = null;
                return false;
            }

            if (converterType.IsValueType == false)
            {
                var ctors = converterType.GetMembers(".ctor");
                IMethodSymbol ctorMethod = null;

                foreach (var ctor in ctors)
                {
                    if (ctor is not IMethodSymbol method
                        || method.DeclaredAccessibility != Accessibility.Public
                    )
                    {
                        continue;
                    }

                    if (method.Parameters.Length == 0)
                    {
                        ctorMethod = method;
                        break;
                    }
                }

                if (ctorMethod == null)
                {
                    context?.ReportDiagnostic(
                          ConverterDiagnosticDescriptors.MissingDefaultConstructor
                        , syntaxRef?.GetSyntax(context?.CancellationToken ?? default) ?? outerNode
                        , converterType.Name
                    );

                    convertMethod = null;
                    return false;
                }
            }

            var members = converterType.GetMembers("Convert");
            IMethodSymbol staticConvertMethod = null;
            IMethodSymbol instanceConvertMethod = null;
            var multipleStaticConvertMethods = false;
            var multipleInstanceConvertMethods = false;

            foreach (var member in members)
            {
                if (member is not IMethodSymbol method
                    || method.DeclaredAccessibility != Accessibility.Public
                )
                {
                    continue;
                }

                if (method.IsStatic)
                {
                    if (multipleStaticConvertMethods == false)
                    {
                        if (staticConvertMethod != null)
                        {
                            staticConvertMethod = null;
                            multipleStaticConvertMethods = true;
                        }
                        else
                        {
                            staticConvertMethod = method;
                        }
                    }

                    continue;
                }

                if (multipleInstanceConvertMethods == false)
                {
                    if (instanceConvertMethod != null)
                    {
                        instanceConvertMethod = null;
                        multipleInstanceConvertMethods = true;
                    }
                    else
                    {
                        instanceConvertMethod = method;
                    }
                }
            }

            var multipleConvertMethods = multipleStaticConvertMethods
                || (multipleStaticConvertMethods == false && multipleInstanceConvertMethods)
                ;

            if (multipleConvertMethods)
            {
                var diagnostic = multipleStaticConvertMethods
                    ? ConverterDiagnosticDescriptors.StaticConvertMethodAmbiguity
                    : ConverterDiagnosticDescriptors.InstancedConvertMethodAmbiguity
                    ;

                context?.ReportDiagnostic(
                      diagnostic
                    , syntaxRef?.GetSyntax(context?.CancellationToken ?? default) ?? outerNode
                    , converterType.Name
                );

                convertMethod = null;
                return false;
            }

            convertMethod = staticConvertMethod ?? instanceConvertMethod;

            if (convertMethod == null)
            {
                var diagnostic = returnType != null
                    ? ConverterDiagnosticDescriptors.MissingConvertMethodReturnType
                    : ConverterDiagnosticDescriptors.MissingConvertMethod
                    ;

                context?.ReportDiagnostic(
                      diagnostic
                    , syntaxRef?.GetSyntax(context?.CancellationToken ?? default) ?? outerNode
                    , converterType.Name
                    , returnType?.ToFullName() ?? string.Empty
                );

                convertMethod = null;
                return false;
            }

            if (convertMethod.Parameters.Length != 1
                || convertMethod.ReturnsVoid
                || (returnType != null && SymbolEqualityComparer.Default.Equals(convertMethod.ReturnType, returnType) == false)
            )
            {
                DiagnosticDescriptor diagnostic;

                if (convertMethod.IsStatic)
                {
                    diagnostic = returnType != null
                        ? ConverterDiagnosticDescriptors.InvalidStaticConvertMethodReturnType
                        : ConverterDiagnosticDescriptors.InvalidStaticConvertMethod
                        ;
                }
                else
                {
                    diagnostic = returnType != null
                        ? ConverterDiagnosticDescriptors.InvalidInstancedConvertMethodReturnType
                        : ConverterDiagnosticDescriptors.InvalidInstancedConvertMethod
                        ;
                }

                context?.ReportDiagnostic(
                      diagnostic
                    , syntaxRef?.GetSyntax(context?.CancellationToken ?? default) ?? outerNode
                    , converterType.Name
                    , returnType?.ToFullName() ?? string.Empty
                );

                convertMethod = null;
                return false;
            }

            return true;
        }

        private static void MakeConverterRef(
              ConverterRef converterRef
            , INamedTypeSymbol converterType
            , IMethodSymbol convertMethod
            , ITypeSymbol targetType
        )
        {
            converterRef.ConverterType = converterType;
            converterRef.TargetType = targetType;
            converterRef.Kind = convertMethod.IsStatic ? ConverterKind.Static : ConverterKind.Instance;

            var sourceTypeRef = converterRef.SourceTypeRef;
            sourceTypeRef.Type = convertMethod.Parameters[0].Type;

            MakeCollectionTypeRef(sourceTypeRef);
        }

        public static bool TryGetCommonConverterRef(
              this MemberRef targetRef
            , DatabaseDeclaration database
            , TableRef tableRef
        )
        {
            if (tableRef.ConverterMap.TryGetValue(targetRef.TypeRef.Type, out var converterRef))
            {
                targetRef.ConverterRef.CopyFrom(converterRef);
                return true;
            }

            if (database.DatabaseRef.ConverterMap.TryGetValue(targetRef.TypeRef.Type, out converterRef))
            {
                targetRef.ConverterRef.CopyFrom(converterRef);
                return true;
            }

            return false;
        }

        public static void TryFallbackConverterRef(this MemberRef targetRef, SyntaxNode outerNode)
        {
            if (targetRef.TypeRef.Type is not INamedTypeSymbol returnType)
            {
                return;
            }

            if (TryGetConvertMethod(outerNode, returnType, out var convertMethod, returnType))
            {
                MakeConverterRef(targetRef.ConverterRef, returnType, convertMethod, returnType);
            }
        }

        public static bool TryMakeConverterRef(
              this TypedConstant typedConstant
            , SourceProductionContext context
            , SyntaxNode outerNode
            , AttributeData attrib
            , int position
            , out ConverterRef result
        )
        {
            if (typedConstant.Value is not INamedTypeSymbol type)
            {
                context.ReportDiagnostic(
                      ConverterDiagnosticDescriptors.NotTypeOfExpressionAt
                    , attrib.ApplicationSyntaxReference.GetSyntax(context.CancellationToken)
                    , position
                );

                result = default;
                return false;
            }

            var syntaxRef = attrib.ApplicationSyntaxReference;

            if (TryGetConvertMethod(outerNode, type, out var convertMethod, context: context, syntaxRef: syntaxRef) == false)
            {
                result = default;
                return false;
            }

            result = new ConverterRef();
            MakeConverterRef(result, type, convertMethod, convertMethod.ReturnType);
            return true;
        }

        public static void MakeConverterMap(
              this ImmutableArray<TypedConstant> values
            , SourceProductionContext context
            , SyntaxNode outerNode
            , AttributeData attrib
            , Dictionary<ITypeSymbol, ConverterRef> converterMap
            , int offset
        )
        {
            if (values.IsDefaultOrEmpty)
            {
                return;
            }

            for (var i = 0; i < values.Length; i++)
            {
                if (values[i].TryMakeConverterRef(context, outerNode, attrib, i, out var converterRef) == false)
                {
                    continue;
                }

                if (converterMap.TryGetValue(converterRef.TargetType, out var anotherConverter))
                {
                    context.ReportDiagnostic(
                          ConverterDiagnosticDescriptors.ConverterAmbiguity
                        , attrib.ApplicationSyntaxReference?.GetSyntax(context.CancellationToken) ?? outerNode
                        , converterRef.ConverterType.Name
                        , anotherConverter.ConverterType.Name
                        , anotherConverter.TargetType.ToFullName()
                        , offset + i
                    );
                }
                else
                {
                    converterMap[converterRef.TargetType] = converterRef;
                }
            }
        }
    }
}
