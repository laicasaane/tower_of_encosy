namespace EncosyTower.SourceGen.Generators.Mvvm.RelayCommands
{
    partial struct RelayCommandDeclaration
    {
        private const string GENERATED_CODE = $"[GeneratedCode(\"EncosyTower.SourceGen.Generators.Mvvm.RelayCommands.RelayCommandGenerator\", \"{SourceGenVersion.VALUE}\")]";
        private const string EXCLUDE_COVERAGE = "[ExcludeFromCodeCoverage]";
        private const string GENERATED_RELAY_COMMAND = "[GeneratedRelayCommand({0})]";
        private const string EDITOR_BROWSABLE_NEVER = "[EditorBrowsable(EditorBrowsableState.Never)]";

        public readonly string WriteCode()
        {
            var p = Printer.DefaultLarge;

            p.PrintEndLine();
            p.Print("#pragma warning disable").PrintEndLine();
            p.PrintEndLine();

            p = p.IncreasedIndent();

            WriteRelayCommandInfoAttributes(ref p);

            p.PrintBeginLine("partial class ").Print(className).PrintEndLine(" : ICommandListener");
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

        private readonly void WriteRelayCommandInfoAttributes(ref Printer p)
        {
            foreach (var member in memberRefs)
            {
                var propName = CommandPropertyName(member);

                p.PrintBeginLine()
                    .Print($"[RelayCommandInfo(\"{propName}\", ");

                if (string.IsNullOrEmpty(member.paramTypeName) == false)
                {
                    p.Print($"typeof({member.paramTypeName})");
                }
                else
                {
                    p.Print($"typeof(void)");
                }

                p.Print(")]").PrintEndLine();
            }
        }

        private readonly void WriteConstantFields(ref Printer p)
        {
            foreach (var member in memberRefs)
            {
                var name = CommandPropertyName(member);

                p.PrintLine($"/// <summary>The name of <see cref=\"{name}\"/></summary>");
                p.PrintLine(GENERATED_CODE);
                p.PrintLine($"public const string {ConstName(member)} = nameof({className}.{name});");
                p.PrintEndLine();
            }

            p.PrintEndLine();
        }

        private readonly void WriteFields(ref Printer p)
        {
            foreach (var member in memberRefs)
            {
                var propertyName = CommandPropertyName(member);
                var fieldName = CommandFieldName(member);
                var typeName = CommandTypeName(member);

                p.PrintLine($"/// <summary>The backing field for <see cref=\"{propertyName}\"/>.</summary>");

                foreach (var attribute in member.forwardedFieldAttributes)
                {
                    p.PrintLine($"[{attribute.GetSyntax().ToFullString()}]");
                }

                p.PrintLine(GENERATED_CODE).PrintLine(EDITOR_BROWSABLE_NEVER);
                p.PrintLine($"private {typeName} {fieldName};");
                p.PrintEndLine();
            }

            p.PrintEndLine();
        }

        private readonly void WriteProperties(ref Printer p)
        {
            foreach (var member in memberRefs)
            {
                var propertyName = CommandPropertyName(member);
                var fieldName = CommandFieldName(member);
                var typeName = CommandTypeName(member);
                var interfaceName = CommandInterfaceName(member);
                var interfaceNameComment = CommandInterfaceNameComment(member);
                var canExecuteArg = CanExecuteMethodArg(member);

                p.PrintLine($"/// <summary>Gets an <see cref=\"{interfaceNameComment}\"/> instance wrapping <see cref=\"{member.methodName}\"/>.</summary>");

                foreach (var attribute in member.forwardedPropertyAttributes)
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
                        p.PrintLine($"this.{fieldName} = new {typeName}({member.methodName}{canExecuteArg});");
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

        private readonly void WriteTryGetCommand(ref Printer p)
        {
            p.PrintLine("/// <inheritdoc/>");
            p.PrintLine("public bool TryGetCommand<TCommand>(string commandName, out TCommand command) where TCommand : ICommand");
            p.OpenScope();
            {
                p.PrintLine("switch (commandName)");
                p.OpenScope();
                {
                    foreach (var member in memberRefs)
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

        private static string ConstName(MemberDeclaration member)
            => $"CommandName_{CommandPropertyName(member)}";

        private static string CommandFieldName(MemberDeclaration member)
            => $"_command{member.methodName}";

        private static string CommandPropertyName(MemberDeclaration member)
            => $"{member.methodName}Command";

        private static string CommandTypeName(MemberDeclaration member)
        {
            if (string.IsNullOrEmpty(member.paramTypeName))
            {
                return "RelayCommand";
            }

            return $"RelayCommand<{member.paramTypeName}>";
        }

        private static string CommandInterfaceName(MemberDeclaration member)
        {
            if (string.IsNullOrEmpty(member.paramTypeName))
            {
                return "IRelayCommand";
            }

            return $"IRelayCommand<{member.paramTypeName}>";
        }

        private static string CommandInterfaceNameComment(MemberDeclaration member)
        {
            if (string.IsNullOrEmpty(member.paramTypeName))
            {
                return "IRelayCommand";
            }

            return $"IRelayCommand{{{member.paramTypeName}}}";
        }

        private static string CanExecuteMethodArg(MemberDeclaration member)
        {
            if (string.IsNullOrEmpty(member.canExecuteMethodName))
                return string.Empty;

            if (member.canExecuteHasParam == false)
                return $", _ => {member.canExecuteMethodName}()";

            return $", {member.canExecuteMethodName}";
        }
    }
}

