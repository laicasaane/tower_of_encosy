using System;
using System.Collections.Immutable;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace EncosyTower.SourceGen
{
    public static class GeneratorHelpers
    {
        private const string GENERATED_CODE = $"[global::System.CodeDom.Compiler.GeneratedCode(\"EncosyTower.SourceGen.Generators\", \"{SourceGenVersion.VALUE}\")]";
        private const string EXCLUDE_COVERAGE = "[global::System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]";
        private const string SKIP_ATTRIBUTE = "global::EncosyTower.CodeGen.SkipSourceGeneratorsForAssemblyAttribute";
        private const string ALLOW_ATTRIBUTE = "global::EncosyTower.CodeGen.AllowSourceGeneratorsForAssemblyAttribute";

        public const string FIELD_PREFIX_UNDERSCORE = "_";
        public const string FIELD_PREFIX_M_UNDERSCORE = "m_";

        public readonly static string[] FullyQualifiedFixedStringTypeNames = new string[] {
            "global::Unity.Collections.FixedString32Bytes",
            "global::Unity.Collections.FixedString64Bytes",
            "global::Unity.Collections.FixedString128Bytes",
            "global::Unity.Collections.FixedString512Bytes",
            "global::Unity.Collections.FixedString4096Bytes",
        };

        public static bool IsValidCompilation(
              this Compilation compilation
            , string generatorNamespace
            , string skipAttribute
        )
        {
            var assembly = compilation.Assembly;
            var skipAllSourceGen = assembly.HasAttribute(SKIP_ATTRIBUTE);
            var skipThisSourceGen = CanSkipThisSourceGen(assembly, skipAttribute);
            var isAllowed = IsThisSourceGenAllowed(assembly, generatorNamespace);

            return (skipAllSourceGen && isAllowed)
                || (skipAllSourceGen == false && skipThisSourceGen == false);

            static bool CanSkipThisSourceGen(IAssemblySymbol assembly, string skipAttribute)
            {
                return string.IsNullOrWhiteSpace(skipAttribute) == false
                    && assembly.HasAttribute(skipAttribute);
            }

            static bool IsThisSourceGenAllowed(IAssemblySymbol assembly, string generatorNamespace)
            {
                if (string.IsNullOrWhiteSpace(generatorNamespace)
                    || assembly.TryGetAttribute(ALLOW_ATTRIBUTE, out var allowAttrib) == false
                )
                {
                    return false;
                }

                var args = allowAttrib.ConstructorArguments;

                if (args.Length < 1 || args[0].Kind != TypedConstantKind.Array)
                {
                    return false;
                }

                var values = args[0].Values;

                foreach (var value in values)
                {
                    if (value.Value is string ns
                        && string.Equals(ns, generatorNamespace, StringComparison.Ordinal)
                    )
                    {
                        return true;
                    }
                }

                return false;
            }
        }

        public static bool IsClassSyntaxMatch(SyntaxNode syntaxNode, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            return syntaxNode is ClassDeclarationSyntax classSyntax
                && classSyntax.BaseList != null
                && classSyntax.BaseList.Types.Count > 0;
        }

        public static bool IsStructSyntaxMatch(SyntaxNode syntaxNode, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            return syntaxNode is StructDeclarationSyntax structSyntax
                && structSyntax.BaseList != null
                && structSyntax.BaseList.Types.Count > 0;
        }

        public static bool IsClassSyntaxMatchByAttribute(
              SyntaxNode syntaxNode
            , CancellationToken token
            , SyntaxKind syntaxKind
            , string attributeNamespace
            , string attributeName
        )
        {
            token.ThrowIfCancellationRequested();

            if (syntaxNode is not ClassDeclarationSyntax classSyntax
                || classSyntax.BaseList == null
                || classSyntax.BaseList.Types.Count < 1
            )
            {
                return false;
            }

            var members = classSyntax.Members;

            foreach (var member in members)
            {
                if (member.Kind() == syntaxKind
                    && member.HasAttributeCandidate(attributeNamespace, attributeName)
                )
                {
                    return true;
                }
            }

            return false;
        }

        public static ClassDeclarationSyntax GetClassSemanticMatch(
              GeneratorSyntaxContext context
            , CancellationToken token
            , string interfaceName
        )
        {
            token.ThrowIfCancellationRequested();

            if (context.Node is not ClassDeclarationSyntax classSyntax
                || classSyntax.BaseList == null
                || classSyntax.DoesSemanticMatch(interfaceName, context.SemanticModel, token) == false
            )
            {
                return null;
            }

            return classSyntax;
        }

        public static StructDeclarationSyntax GetStructSemanticMatch(
              GeneratorSyntaxContext context
            , CancellationToken token
            , string interfaceName
        )
        {
            token.ThrowIfCancellationRequested();

            if (context.Node is not StructDeclarationSyntax structSyntax
                || structSyntax.BaseList == null
                || structSyntax.DoesSemanticMatch(interfaceName, context.SemanticModel, token) == false
            )
            {
                return null;
            }

            return structSyntax;
        }

        public static bool DoesSemanticMatch(
              this ClassDeclarationSyntax classSyntax
            , string interfaceName
            , SemanticModel semanticModel
            , CancellationToken token
        )
        {
            if (classSyntax.BaseList != null)
            {
                foreach (var baseType in classSyntax.BaseList.Types)
                {
                    var typeInfo = semanticModel.GetTypeInfo(baseType.Type, token);

                    if (typeInfo.Type.ToFullName() == interfaceName)
                    {
                        return true;
                    }

                    if (typeInfo.Type.ImplementsInterface(interfaceName))
                    {
                        return true;
                    }

                    if (IsMatch(typeInfo.Type.Interfaces, interfaceName)
                        || IsMatch(typeInfo.Type.AllInterfaces, interfaceName)
                    )
                    {
                        return true;
                    }
                }
            }

            return false;

            static bool IsMatch(ImmutableArray<INamedTypeSymbol> interfaces, string interfaceName)
            {
                foreach (var interfaceSymbol in interfaces)
                {
                    if (interfaceSymbol.ToFullName() == interfaceName)
                    {
                        return true;
                    }
                }

                return false;
            }
        }

        public static bool DoesSemanticMatch(
              this StructDeclarationSyntax structSyntax
            , string interfaceName
            , SemanticModel semanticModel
            , CancellationToken token
        )
        {
            if (structSyntax.BaseList != null)
            {
                foreach (var baseType in structSyntax.BaseList.Types)
                {
                    var typeInfo = semanticModel.GetTypeInfo(baseType.Type, token);

                    if (typeInfo.Type.ToFullName() == interfaceName)
                    {
                        return true;
                    }

                    if (typeInfo.Type.ImplementsInterface(interfaceName))
                    {
                        return true;
                    }

                    if (IsMatch(typeInfo.Type.Interfaces, interfaceName)
                        || IsMatch(typeInfo.Type.AllInterfaces, interfaceName)
                    )
                    {
                        return true;
                    }
                }
            }

            return false;

            static bool IsMatch(ImmutableArray<INamedTypeSymbol> interfaces, string interfaceName)
            {
                foreach (var interfaceSymbol in interfaces)
                {
                    if (interfaceSymbol.ToFullName() == interfaceName)
                    {
                        return true;
                    }
                }

                return false;
            }
        }

        public static string ToPropertyName(this IFieldSymbol field)
        {
            return ToPropertyName(field.Name);
        }

        public static string ToPropertyName(this string fieldName)
        {
            var nameSpan = fieldName.AsSpan();
            var prefix = FIELD_PREFIX_UNDERSCORE.AsSpan();

            if (nameSpan.StartsWith(prefix))
            {
                return MakeFirstCharUpperCase(nameSpan.Slice(1));
            }

            prefix = FIELD_PREFIX_M_UNDERSCORE.AsSpan();

            if (nameSpan.StartsWith(prefix))
            {
                return MakeFirstCharUpperCase(nameSpan.Slice(2));
            }

            return MakeFirstCharUpperCase(nameSpan);
        }

        public static string ToPublicFieldName(this IPropertySymbol property)
        {
            return MakeFirstCharLowerCase(property.Name.AsSpan());
        }

        public static string ToPublicFieldName(this string propertyName)
        {
            return MakeFirstCharLowerCase(propertyName.AsSpan());
        }

        public static string ToPrivateFieldName(this IPropertySymbol property)
        {
            return $"{FIELD_PREFIX_UNDERSCORE}{MakeFirstCharLowerCase(property.Name.AsSpan())}";
        }

        public static string ToPrivateFieldName(this string propertyName)
        {
            return $"{FIELD_PREFIX_UNDERSCORE}{MakeFirstCharLowerCase(propertyName.AsSpan())}";
        }

        public static string ToArgumentName(this string propertyName)
        {
            return $"{MakeFirstCharLowerCase(propertyName.AsSpan())}";
        }

        public static string MakeFirstCharUpperCase(this ReadOnlySpan<char> value)
        {
            return $"{char.ToUpper(value[0])}{value.Slice(1).ToString()}";
        }

        public static string MakeFirstCharLowerCase(this ReadOnlySpan<char> value)
        {
            return $"{char.ToLower(value[0])}{value.Slice(1).ToString()}";
        }

        public static string GetKeyword(this Accessibility accessibility)
            => accessibility switch {
                Accessibility.Public => "public",
                Accessibility.Private => "private",
                Accessibility.Protected => "protected",
                Accessibility.Internal => "internal",
                Accessibility.ProtectedOrInternal => "protected internal",
                Accessibility.ProtectedAndInternal => "private protected",
                _ => ""
            };

        public static string GetFixedStringTypeName(int maxByteCount)
            => (maxByteCount + 3) switch {
                <= 32 - 3 => "FixedString32Bytes",
                <= 64 - 3 => "FixedString64Bytes",
                <= 128 - 3 => "FixedString128Bytes",
                <= 512 - 3 => "FixedString512Bytes",
                _ => "FixedString4096Bytes",
            };

        public static string GetFixedStringFullyQualifiedTypeName(int maxByteCount)
            => (maxByteCount + 3) switch {
                <= 32 - 3 => "global::Unity.Collections.FixedString32Bytes",
                <= 64 - 3 => "global::Unity.Collections.FixedString64Bytes",
                <= 128 - 3 => "global::Unity.Collections.FixedString128Bytes",
                <= 512 - 3 => "global::Unity.Collections.FixedString512Bytes",
                _ => "global::Unity.Collections.FixedString4096Bytes",
            };

        public static Printer WritePreserveAttributeClass(this ref Printer p, string generatedCode = null)
        {
            p.PrintLine(generatedCode ?? GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
            p.PrintLine("[global::System.AttributeUsage(");
            p = p.IncreasedIndent();
            {
                p.PrintLine("  global::System.AttributeTargets.Assembly");
                p.PrintLine("| global::System.AttributeTargets.Class");
                p.PrintLine("| global::System.AttributeTargets.Struct");
                p.PrintLine("| global::System.AttributeTargets.Enum");
                p.PrintLine("| global::System.AttributeTargets.Constructor");
                p.PrintLine("| global::System.AttributeTargets.Method");
                p.PrintLine("| global::System.AttributeTargets.Property");
                p.PrintLine("| global::System.AttributeTargets.Field");
                p.PrintLine("| global::System.AttributeTargets.Event");
                p.PrintLine("| global::System.AttributeTargets.Interface");
                p.PrintLine("| global::System.AttributeTargets.Delegate");
                p.PrintLine(", Inherited = false");
            }
            p = p.DecreasedIndent();
            p.PrintLine(")]");
            p.PrintLine("public sealed class PreserveAttribute : global::System.Attribute { }");
            return p;
        }

        public static string GetEnumUnderlyingTypeFromMemberCount(long memberCount)
            => GetEnumUnderlyingTypeFromSize(DetermineEnumSizeFromMemberCount(memberCount));

        public static string GetEnumUnderlyingTypeFromMemberCount(ulong memberCount)
            => GetEnumUnderlyingTypeFromSize(DetermineEnumSizeFromMemberCount(memberCount));

        public static string GetEnumUnderlyingTypeFromSize(int size)
            => size switch {
                <= 1 => "byte",
                <= 2 => "ushort",
                <= 4 => "uint",
                _ => "ulong",
            };

        public static int DetermineEnumSizeFromMemberCount(long memberCount)
            => memberCount switch {
                <= byte.MaxValue => 1,
                <= ushort.MaxValue => 2,
                <= uint.MaxValue => 4,
                _ => 8,
            };

        public static int DetermineEnumSizeFromMemberCount(ulong memberCount)
            => memberCount switch {
                <= byte.MaxValue => 1,
                <= ushort.MaxValue => 2,
                <= uint.MaxValue => 4,
                _ => 8,
            };
    }
}
