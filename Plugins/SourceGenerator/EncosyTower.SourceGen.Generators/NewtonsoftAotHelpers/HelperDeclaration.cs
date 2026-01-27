using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

namespace EncosyTower.SourceGen.Generators.NewtonsoftJsonHelpers
{
    internal partial struct HelperDeclaration
    {
        private const string AOT_HELPER = "global::Newtonsoft.Json.Utilities.AotHelper";

        public string WriteCode(
              HelperCandidate helper
            , IEnumerable<INamedTypeSymbol> types
        )
        {
            var syntax = helper.syntax;
            var symbol = helper.symbol;

            var scopePrinter = new SyntaxNodeScopePrinter(Printer.DefaultLarge, syntax.Parent);
            var p = scopePrinter.printer;
            p = p.IncreasedIndent();

            var staticKeyword = symbol.IsStatic ? "static " : "";
            var recordKeyword = symbol.IsRecord ? "record " : "";
            var typeKeyword = symbol.TypeKind == TypeKind.Class ? "class " : "struct ";

            p.PrintEndLine();
            p.Print("#pragma warning disable").PrintEndLine();
            p.PrintEndLine();

            p.PrintBeginLine(staticKeyword).Print("partial ").Print(recordKeyword).Print(typeKeyword)
                .PrintEndLine(syntax.Identifier.Text);
            p.OpenScope();
            {
                p.PrintLine("[global::UnityEngine.Scripting.Preserve]");
                p.PrintBeginLine("public ").Print(staticKeyword).PrintEndLine("void EnsureNewtonsoftJson()");
                p.OpenScope();
                {
                    foreach (var type in types)
                    {
                        p.PrintBeginLine(AOT_HELPER).Print(".EnsureType<")
                            .Print(type.ToFullName()).PrintEndLine(">();");
                        p.OpenScope();
                        {
                            var baseType = type;

                            while (baseType != null)
                            {
                                var members = baseType.GetMembers();

                                foreach (var member in members)
                                {
                                    if (member is not IFieldSymbol field
                                        || field.Type is not INamedTypeSymbol fieldType
                                    )
                                    {
                                        continue;
                                    }

                                    WriteType(ref p, fieldType);

                                    if (fieldType.IsGenericType == false || fieldType.IsUnboundGenericType)
                                    {
                                        continue;
                                    }

                                    if (TryGetDictionaryTypeArgs(fieldType, out var keyTypeArg, out var valueTypeArg))
                                    {
                                        WriteType(ref p, keyTypeArg);
                                        WriteType(ref p, valueTypeArg);
                                        p.PrintBeginLine(AOT_HELPER).Print(".EnsureDictionary<")
                                            .Print(keyTypeArg.ToFullName())
                                            .Print(", ")
                                            .Print(valueTypeArg.ToFullName())
                                            .PrintEndLine(">();");
                                        continue;
                                    }

                                    if (TryGetListSetTypeArg(fieldType, out var typeArg))
                                    {
                                        WriteType(ref p, typeArg);
                                        p.PrintBeginLine(AOT_HELPER).Print(".EnsureList<")
                                            .Print(typeArg.ToFullName()).PrintEndLine(">();");
                                        continue;
                                    }

                                    WriteTypeArgs(ref p, fieldType.TypeArguments);
                                }

                                baseType = baseType.BaseType;
                            }
                        }
                        p.CloseScope();
                    }
                }
                p.CloseScope();
                p.PrintEndLine();
            }
            p.CloseScope();
            p.PrintEndLine();

            p = p.DecreasedIndent();
            return p.Result;
        }

        private static void WriteType(ref Printer p, INamedTypeSymbol type)
        {
            if (type.IsAbstract
                || type.SpecialType.IsSystemType()
                || type.TypeKind is not (TypeKind.Struct or TypeKind.Class)
            )
            {
                return;
            }

            if (type.TypeKind == TypeKind.Class)
            {
                var constructors = type.Constructors;
                var hasDefault = false;

                foreach (var constructor in constructors)
                {
                    if (constructor.Parameters.Length == 0)
                    {
                        hasDefault = true;
                        break;
                    }
                }

                if (hasDefault == false)
                {
                    return;
                }
            }

            p.PrintBeginLine(AOT_HELPER).Print(".EnsureType<")
                .Print(type.ToFullName()).PrintEndLine(">();");
        }

        private static void WriteTypeArgs(ref Printer p, ImmutableArray<ITypeSymbol> types)
        {
            if (types.IsDefaultOrEmpty)
            {
                return;
            }

            for (var i = 0; i < types.Length; i++)
            {
                if (types[i] is INamedTypeSymbol type)
                {
                    WriteType(ref p, type);
                }
            }
        }

        private static bool TryGetListSetTypeArg(
              INamedTypeSymbol type
            , out INamedTypeSymbol typeArg
        )
        {
            if (type.IsGenericType
                && type.TypeArguments.IsDefaultOrEmpty == false
                && type.TypeArguments.Length == 1
                && type.TypeArguments[0] is INamedTypeSymbol typeArgCandidate
            )
            {
                var name = type.ToFullName();

                if (name.StartsWith("global::System.Collections.Generic.List<")
                    || name.StartsWith("global::System.Collections.Generic.HashSet<")
                )
                {
                    typeArg = typeArgCandidate;
                    return true;
                }
            }

            typeArg = default;
            return false;
        }

        private static bool TryGetDictionaryTypeArgs(
              INamedTypeSymbol type
            , out INamedTypeSymbol keyTypeArg
            , out INamedTypeSymbol valueTypeArg
        )
        {
            if (type.IsGenericType
                && type.TypeArguments.IsDefaultOrEmpty == false
                && type.TypeArguments.Length == 2
                && type.TypeArguments[0] is INamedTypeSymbol keyTypeArgCandidate
                && type.TypeArguments[1] is INamedTypeSymbol valueTypeArgCandidate
            )
            {
                var name = type.ToFullName();

                if (name.StartsWith("global::System.Collections.Generic.Dictionary<"))
                {
                    keyTypeArg = keyTypeArgCandidate;
                    valueTypeArg = valueTypeArgCandidate;
                    return true;
                }
            }

            keyTypeArg = default;
            valueTypeArg = default;
            return false;
        }
    }
}
