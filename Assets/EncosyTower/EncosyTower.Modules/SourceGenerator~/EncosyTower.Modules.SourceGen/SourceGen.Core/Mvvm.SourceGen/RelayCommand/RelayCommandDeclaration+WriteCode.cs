using EncosyTower.Modules.SourceGen;
using Microsoft.CodeAnalysis;

namespace EncosyTower.Modules.Mvvm.RelayCommandSourceGen
{
    partial class RelayCommandDeclaration
    {
        private const string GENERATED_CODE = "[global::System.CodeDom.Compiler.GeneratedCode(\"EncosyTower.Modules.Mvvm.RelayCommandGenerator\", \"1.0.0\")]";
        private const string EXCLUDE_COVERAGE = "[global::System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]";
        private const string GENERATED_RELAY_COMMAND = "[global::EncosyTower.Modules.Mvvm.Input.SourceGen.GeneratedRelayCommand({0})]";

        public string WriteCode()
        {
            var scopePrinter = new SyntaxNodeScopePrinter(Printer.DefaultLarge, Syntax.Parent);
            var p = scopePrinter.printer;

            p.PrintEndLine();
            p.Print("#pragma warning disable").PrintEndLine();
            p.PrintEndLine();

            p = p.IncreasedIndent();

            WriteRelayCommandInfoAttributes(ref p);

            p.PrintBeginLine("partial class ").Print(ClassName)
                .PrintEndLine(" : global::EncosyTower.Modules.Mvvm.Input.ICommandListener");

            p.OpenScope();
            {
                WriteConstantFields(ref p);
                WriteFields(ref p);
                WriteProperties(ref p);
                WriteTryGetCommand(ref p);
            }
            p.CloseScope();

            p = p.DecreasedIndent();
            return p.Result;
        }

        private void WriteRelayCommandInfoAttributes(ref Printer p)
        {
            foreach (var member in MemberRefs)
            {
                var propName = CommandPropertyName(member);

                p.PrintBeginLine()
                    .Print($"[global::EncosyTower.Modules.Mvvm.Input.RelayCommandInfo(\"{propName}\", ");

                if (member.Member.Parameters.Length > 0)
                {
                    var param = member.Member.Parameters[0];
                    var paramType = param.Type.ToFullName();

                    p.Print($"typeof({paramType})");
                }
                else
                {
                    p.Print($"typeof(void)");
                }

                p.Print(")]").PrintEndLine();
            }
        }

        private void WriteConstantFields(ref Printer p)
        {
            foreach (var member in MemberRefs)
            {
                var name = CommandPropertyName(member);

                p.PrintLine($"/// <summary>The name of <see cref=\"{name}\"/></summary>");
                p.PrintLine(GENERATED_CODE);
                p.PrintLine($"public const string {ConstName(member)} = nameof({ClassName}.{name});");
                p.PrintEndLine();
            }

            p.PrintEndLine();
        }

        private void WriteFields(ref Printer p)
        {
            foreach (var member in MemberRefs)
            {
                var propertyName = CommandPropertyName(member);
                var fieldName = CommandFieldName(member);
                var typeName = CommandTypeName(member);

                p.PrintLine($"/// <summary>The backing field for <see cref=\"{propertyName}\"/>.</summary>");

                foreach (var attribute in member.ForwardedFieldAttributes)
                {
                    p.PrintLine($"[{attribute.GetSyntax().ToFullString()}]");
                }

                p.PrintLine(GENERATED_CODE);
                p.PrintLine($"private {typeName} {fieldName};");
                p.PrintEndLine();
            }

            p.PrintEndLine();
        }

        private void WriteProperties(ref Printer p)
        {
            foreach (var member in MemberRefs)
            {
                var method = member.Member;
                var propertyName = CommandPropertyName(member);
                var fieldName = CommandFieldName(member);
                var typeName = CommandTypeName(member);
                var interfaceName = CommandInterfaceName(member);
                var interfaceNameComment = CommandInterfaceNameComment(member);
                var canExecuteArg = CanExecuteMethodArg(member.CanExecuteMethod);

                p.PrintLine($"/// <summary>Gets an <see cref=\"{interfaceNameComment}\"/> instance wrapping <see cref=\"{method.Name}\"/>.</summary>");

                foreach (var attribute in member.ForwardedPropertyAttributes)
                {
                    p.PrintLine($"[{attribute.GetSyntax().ToFullString()}]");
                }

                p.PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
                p.PrintLine(string.Format(GENERATED_RELAY_COMMAND, ConstName(member)));
                p.PrintLine($"public {interfaceName} {propertyName}");
                p.OpenScope();
                {
                    p.PrintLine("get");
                    p.OpenScope();
                    {
                        p.PrintLine($"if (this.{fieldName} == null)");
                        p = p.IncreasedIndent();
                        p.PrintLine($"this.{fieldName} = new {typeName}({method.Name}{canExecuteArg});");
                        p = p.DecreasedIndent();
                        p.PrintEndLine();

                        p.PrintLine($"return this.{fieldName};");
                    }
                    p.CloseScope();
                }
                p.CloseScope();
                p.PrintEndLine();
            }

            p.PrintEndLine();
        }

        private void WriteTryGetCommand(ref Printer p)
        {
            p.PrintLine("/// <inheritdoc/>");
            p.PrintLine("public bool TryGetCommand<TCommand>(string commandName, out TCommand command) where TCommand : global::EncosyTower.Modules.Mvvm.Input.ICommand");
            p.OpenScope();
            {
                p.PrintLine("switch (commandName)");
                p.OpenScope();
                {
                    foreach (var member in MemberRefs)
                    {
                        var constName = ConstName(member);
                        var propertyName = CommandPropertyName(member);

                        p.PrintLine($"case {constName}:");
                        p.OpenScope();
                        {
                            p.PrintLine($"if (this.{propertyName} is TCommand commandT)");
                            p.OpenScope();
                            {
                                p.PrintLine("command = commandT;");
                                p.PrintLine("return true;");
                            }
                            p.CloseScope();
                            p.PrintEndLine();

                            p.PrintLine("command = default;");
                            p.PrintLine("return false;");
                        }
                        p.CloseScope();
                        p.PrintEndLine();
                    }

                    p.PrintLine($"default:");
                    p.OpenScope();
                    {
                        p.PrintLine("command = default;");
                        p.PrintLine("return false;");
                    }
                    p.CloseScope();
                }
                p.CloseScope();
            }
            p.CloseScope();
            p.PrintEndLine();
        }

        private static string ConstName(MemberRef member)
            => $"CommandName_{CommandPropertyName(member)}";

        private static string CommandFieldName(MemberRef method)
            => $"_command{method.Member.Name}";

        private static string CommandPropertyName(MemberRef method)
            => $"{method.Member.Name}Command";

        private static string CommandTypeName(MemberRef method)
        {
            if (method.Member.Parameters.Length < 1)
            {
                return "global::EncosyTower.Modules.Mvvm.Input.RelayCommand";
            }

            var param = method.Member.Parameters[0];
            return $"global::EncosyTower.Modules.Mvvm.Input.RelayCommand<{param.Type.ToFullName()}>";
        }

        private static string CommandInterfaceName(MemberRef method)
        {
            if (method.Member.Parameters.Length < 1)
            {
                return "global::EncosyTower.Modules.Mvvm.Input.IRelayCommand";
            }

            var param = method.Member.Parameters[0];
            return $"global::EncosyTower.Modules.Mvvm.Input.IRelayCommand<{param.Type.ToFullName()}>";
        }

        private static string CommandInterfaceNameComment(MemberRef method)
        {
            if (method.Member.Parameters.Length < 1)
            {
                return "global::EncosyTower.Modules.Mvvm.Input.IRelayCommand";
            }

            var param = method.Member.Parameters[0];
            return $"global::EncosyTower.Modules.Mvvm.Input.IRelayCommand{{{param.Type.ToFullName()}}}";
        }

        private static string CanExecuteMethodArg(IMethodSymbol method)
        {
            if (method == null)
                return string.Empty;

            if (method.Parameters.Length < 1)
                return $", _ => {method.Name}()";

            return $", {method.Name}";
        }
    }
}
