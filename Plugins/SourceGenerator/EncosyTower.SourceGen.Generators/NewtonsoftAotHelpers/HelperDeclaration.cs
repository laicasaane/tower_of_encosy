using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace EncosyTower.SourceGen.Generators.NewtonsoftAotHelpers
{
    internal struct HelperDeclaration
    {
        private const string AOT_HELPER = "NSJU.AotHelper";

        public static string WriteCode(
              NewtonsoftAotHelperSpec helper
            , IEnumerable<AotTypeSpec> types
        )
        {
            var p = Printer.DefaultLarge;

            p.PrintEndLine();
            p.Print("#pragma warning disable").PrintEndLine();
            p.PrintEndLine();

            var staticKeyword = helper.isStatic ? "static " : "";
            var recordKeyword = helper.isRecord ? "record " : "";
            var typeKeyword = helper.typeKind == TypeKind.Class ? "class " : "struct ";

            p.PrintBeginLine(staticKeyword).Print("partial ").Print(recordKeyword).Print(typeKeyword)
                .PrintEndLine(helper.typeName);
            p.OpenScope();
            {
                p.PrintLine("[UES.Preserve]");
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

            return p.Result;
        }

        private static void WriteFieldInfo(ref Printer p, in AotFieldSpec field)
        {
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

        private static void WriteTypeArgEnsure(ref Printer p, in AotTypeArgSpec typeArg)
        {
            if (typeArg.canEnsure)
            {
                p.PrintBeginLine(AOT_HELPER).Print(".EnsureType<")
                    .Print(typeArg.fullName).PrintEndLine(">();");
            }
        }
    }
}

