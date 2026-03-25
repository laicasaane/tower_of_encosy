using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace EncosyTower.SourceGen.Generators.NewtonsoftJsonHelpers
{
    internal struct HelperDeclaration
    {
        private const string AOT_HELPER = "global::Newtonsoft.Json.Utilities.AotHelper";

        /// <summary>
        /// Generates the complete source text for the partial class/struct annotated
        /// with <c>[NewtonsoftJsonAotHelper]</c>, including the namespace and any
        /// containing-type wrappers derived from <paramref name="helper"/>.
        /// No Roslyn symbols are accessed; all data is pre-extracted in the pipeline.
        /// </summary>
        public static string WriteCode(
              NewtonsoftAotHelperInfo helper
            , IEnumerable<AotTypeCandidate> types
        )
        {
            var p = Printer.DefaultLarge;

            p.PrintEndLine();
            p.Print("#pragma warning disable").PrintEndLine();
            p.PrintEndLine();

            var hasNamespace = string.IsNullOrEmpty(helper.namespaceName) == false;
            var numContainingTypes = helper.containingTypes.Count;

            if (hasNamespace)
            {
                p.PrintLine($"namespace {helper.namespaceName}");
                p.OpenScope();
            }

            for (var i = 0; i < numContainingTypes; i++)
            {
                p.PrintLine(helper.containingTypes[i]);
                p.OpenScope();
            }

            var staticKeyword = helper.isStatic ? "static " : "";
            var recordKeyword = helper.isRecord ? "record " : "";
            var typeKeyword = helper.typeKind == TypeKind.Class ? "class " : "struct ";

            p.PrintBeginLine(staticKeyword).Print("partial ").Print(recordKeyword).Print(typeKeyword)
                .PrintEndLine(helper.typeName);
            p.OpenScope();
            {
                p.PrintLine("[global::UnityEngine.Scripting.Preserve]");
                p.PrintBeginLine("public ").Print(staticKeyword).PrintEndLine("void EnsureNewtonsoftJson()");
                p.OpenScope();
                {
                    foreach (var type in types)
                    {
                        p.PrintBeginLine(AOT_HELPER).Print(".EnsureType<")
                            .Print(type.fullName).PrintEndLine(">();");
                        p.OpenScope();
                        {
                            var fields = type.fields;
                            var count = fields.Count;

                            for (var i = 0; i < count; i++)
                            {
                                WriteFieldInfo(ref p, fields[i]);
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

            for (var i = 0; i < numContainingTypes; i++)
            {
                p.CloseScope();
            }

            if (hasNamespace)
            {
                p.CloseScope();
            }

            return p.Result;
        }

        /// <summary>
        /// Emits the AOT registration calls for a single pre-extracted field entry.
        /// Mirrors the logic that was previously spread across
        /// <c>WriteType</c>, <c>WriteTypeArgs</c>, <c>TryGetDictionaryTypeArgs</c>,
        /// and <c>TryGetListSetTypeArg</c>, but operates entirely on primitive data.
        /// </summary>
        private static void WriteFieldInfo(ref Printer p, in AotFieldInfo field)
        {
            // Always try to emit EnsureType for the field type itself.
            if (field.fieldTypeCanEnsure)
            {
                p.PrintBeginLine(AOT_HELPER).Print(".EnsureType<")
                    .Print(field.fieldTypeFullName).PrintEndLine(">();");
            }

            switch (field.collectionKind)
            {
                case AotCollectionKind.Dictionary:
                {
                    WriteTypeArgEnsure(ref p, field.elementOrKey);
                    WriteTypeArgEnsure(ref p, field.dictionaryValue);
                    p.PrintBeginLine(AOT_HELPER).Print(".EnsureDictionary<")
                        .Print(field.elementOrKey.fullName)
                        .Print(", ")
                        .Print(field.dictionaryValue.fullName)
                        .PrintEndLine(">();");
                    break;
                }

                case AotCollectionKind.List:
                case AotCollectionKind.HashSet:
                {
                    WriteTypeArgEnsure(ref p, field.elementOrKey);
                    p.PrintBeginLine(AOT_HELPER).Print(".EnsureList<")
                        .Print(field.elementOrKey.fullName).PrintEndLine(">();");
                    break;
                }

                case AotCollectionKind.None:
                {
                    // Emit EnsureType for each named type argument of other generics.
                    var otherTypeArgs = field.otherTypeArgs;
                    var count = otherTypeArgs.Count;

                    for (var i = 0; i < count; i++)
                    {
                        WriteTypeArgEnsure(ref p, otherTypeArgs[i]);
                    }

                    break;
                }
            }
        }

        private static void WriteTypeArgEnsure(ref Printer p, in AotTypeArgInfo typeArg)
        {
            if (typeArg.canEnsure)
            {
                p.PrintBeginLine(AOT_HELPER).Print(".EnsureType<")
                    .Print(typeArg.fullName).PrintEndLine(">();");
            }
        }
    }
}

