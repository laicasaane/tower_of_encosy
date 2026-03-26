using Microsoft.CodeAnalysis;

namespace EncosyTower.SourceGen.Generators.Mvvm.Binders
{
    partial struct BinderDeclaration
    {
        private const string AGGRESSIVE_INLINING = "[MethodImpl(MethodImplOptions.AggressiveInlining)]";
        private const string GENERATED_CODE = $"[GeneratedCode(\"EncosyTower.SourceGen.Generators.Mvvm.Binders.BinderGenerator\", \"{SourceGenVersion.VALUE}\")]";
        private const string EXCLUDE_COVERAGE = "[ExcludeFromCodeCoverage]";
        private const string OBSOLETE_METHOD = "[Obsolete(\"This method is not intended to be used directly by user code.\")]";
        private const string EDITOR_BROWSABLE_NEVER = "[EditorBrowsable(EditorBrowsableState.Never)]";
        private const string GENERATED_BINDING_PROPERTY = $"[GeneratedBindingProperty({{0}}, typeof({{1}}))]";
        private const string GENERATED_BINDING_COMMAND = $"[GeneratedBindingCommand(";
        private const string GENERATED_CONVERTER = $"[GeneratedConverter({{0}}, typeof({{1}}))]";
        private const string IADAPTER = "IAdapter";
        private const string CACHED_VARIANT_CONVERTER = "CachedVariantConverter";

        public readonly string WriteCode()
        {
            var p = Printer.DefaultLarge;

            p.PrintEndLine();
            p.Print("#pragma warning disable").PrintEndLine();
            p.PrintEndLine();

            p = p.IncreasedIndent();

            WriteBindingPropertyMethodInfoAttributes(ref p);
            WriteBindingCommandMethodInfoAttributes(ref p);

            p.PrintBeginLine("partial class ").Print(className).PrintEndLine(" : IBinder");
            p.OpenScope();
            {
                WriteConstantBindingProperties(ref p);
                WriteConstantBindingCommands(ref p);
                WriteBindingProperties(ref p);
                WriteConverters(ref p);
                WriteBindingCommands(ref p);

                if (nonVariantTypes.Count > 0)
                {
                    WriteVariantConverters(ref p);
                }

                WriteListeners(ref p);
                WriteRelayCommands(ref p);
                WriteFlags(ref p);
                WriteConstructor(ref p);
                WriteContextProperty(ref p);
                WriteStartListeningMethod(ref p);
                WritePartialOnBindFailedMethods(ref p);
                WriteStopListeningMethod(ref p);
                WriteSetTargetPropertyNameMethod(ref p);
                WriteSetAdapterMethod(ref p);

                if (nonVariantTypes.Count > 0)
                {
                    WriteVariantOverloads(ref p);
                }

                WritePartialBindingCommandMethods(ref p);
                WriteSetTargetCommandNameMethod(ref p);
                WriteRefreshContextMethod(ref p);
            }
            p.CloseScope();

            p = p.DecreasedIndent();
            return p.Result;
        }

        private readonly void WriteBindingPropertyMethodInfoAttributes(ref Printer p)
        {
            foreach (var member in bindingPropertyRefs)
            {
                var paramType = string.IsNullOrEmpty(member.paramFullTypeName)
                    ? "void"
                    : member.paramFullTypeName;

                p.PrintBeginLine()
                    .Print($"[BindingPropertyMethodInfo(")
                    .Print($"\"{member.methodName}\", typeof(")
                    .Print(paramType)
                    .PrintEndLine("))]");
            }
        }

        private readonly void WriteBindingCommandMethodInfoAttributes(ref Printer p)
        {
            foreach (var member in bindingCommandRefs)
            {
                p.PrintBeginLine()
                    .Print($"[BindingCommandMethodInfo(")
                    .Print($"\"{member.methodName}\", ");

                if (string.IsNullOrEmpty(member.paramFullTypeName))
                {
                    p.Print("typeof(void)");
                }
                else
                {
                    p.Print("typeof(").Print(member.paramFullTypeName).Print(")");
                }

                p.Print(")]").PrintEndLine();
            }
        }

        private readonly void WriteConstantBindingProperties(ref Printer p)
        {
            if (bindingPropertyRefs.Count < 1)
            {
                return;
            }

            foreach (var member in bindingPropertyRefs)
            {
                var name = member.methodName;

                p.PrintLine($"/// <summary>The name of <see cref=\"{name}\"/></summary>");
                p.PrintLine(GENERATED_CODE);
                p.PrintLine($"public const string {ConstName(member)} = nameof({className}.{name});");
                p.PrintEndLine();
            }

            p.PrintEndLine();
        }

        private readonly void WriteConstantBindingCommands(ref Printer p)
        {
            if (bindingCommandRefs.Count < 1)
            {
                return;
            }

            foreach (var member in bindingCommandRefs)
            {
                var name = member.methodName;

                p.PrintLine($"/// <summary>The name of <see cref=\"{name}\"/></summary>");
                p.PrintLine(GENERATED_CODE);
                p.PrintLine($"public const string {ConstName(member)} = nameof({className}.{name});");
                p.PrintEndLine();
            }

            p.PrintEndLine();
        }

        private readonly void WriteBindingProperties(ref Printer p)
        {
            if (bindingPropertyRefs.Count < 1)
            {
                return;
            }

            foreach (var member in bindingPropertyRefs)
            {
                if (member.skipBindingProperty)
                {
                    continue;
                }

                p.PrintLine($"/// <summary>The binding property for <see cref=\"{member.methodName}\"/></summary>");
                p.PrintLine("[global::UnityEngine.SerializeField]");

                foreach (var attribute in member.forwardedFieldAttributes)
                {
                    p.PrintLine($"[{attribute.GetSyntax().ToFullString()}]");
                }

                p.PrintLine(GENERATED_CODE);

                var paramType = string.IsNullOrEmpty(member.paramFullTypeName)
                    ? "void"
                    : member.paramFullTypeName;

                p.PrintLine(string.Format(GENERATED_BINDING_PROPERTY, ConstName(member), paramType));
                p.PrintLine($"private BindingProperty {BindingPropertyName(member)} =  new BindingProperty();");
                p.PrintEndLine();
            }

            p.PrintEndLine();
        }

        private readonly void WriteConverters(ref Printer p)
        {
            if (bindingPropertyRefs.Count < 1)
            {
                return;
            }

            foreach (var member in bindingPropertyRefs)
            {
                if (member.skipConverter || string.IsNullOrEmpty(member.paramFullTypeName))
                {
                    continue;
                }

                var typeName = member.paramFullTypeName;

                p.PrintLine($"/// <summary>The converter for the parameter of <see cref=\"{member.methodName}\"/></summary>");
                p.PrintLine("[global::UnityEngine.SerializeField]");

                foreach (var attribute in member.forwardedFieldAttributes)
                {
                    p.PrintLine($"[{attribute.GetSyntax().ToFullString()}]");
                }

                p.PrintLine(GENERATED_CODE);
                p.PrintLine(string.Format(GENERATED_CONVERTER, ConstName(member), typeName));
                p.PrintLine($"private Converter {ConverterName(member)} = new Converter();");
                p.PrintEndLine();
            }

            p.PrintEndLine();
        }

        private readonly void WriteBindingCommands(ref Printer p)
        {
            if (bindingCommandRefs.Count < 1)
            {
                return;
            }

            foreach (var member in bindingCommandRefs)
            {
                if (member.skipBindingCommand)
                {
                    continue;
                }

                p.PrintLine($"/// <summary>The binding command for <see cref=\"{member.methodName}\"/></summary>");
                p.PrintLine("[global::UnityEngine.SerializeField]");

                foreach (var attribute in member.forwardedFieldAttributes)
                {
                    p.PrintLine($"[{attribute.GetSyntax().ToFullString()}]");
                }

                p.PrintLine(GENERATED_CODE);
                p.PrintBeginLine()
                    .Print(GENERATED_BINDING_COMMAND)
                    .Print(ConstName(member));

                if (string.IsNullOrEmpty(member.paramFullTypeName) == false)
                {
                    p.Print($", typeof({member.paramFullTypeName})");
                }
                else
                {
                    p.Print($", typeof(void)");
                }

                p.Print(")]").PrintEndLine();

                p.PrintLine($"private BindingCommand {BindingCommandName(member)} =  new BindingCommand();");
                p.PrintEndLine();
            }

            p.PrintEndLine();
        }

        private readonly void WriteVariantConverters(ref Printer p)
        {
            if (nonVariantTypes.Count < 1)
            {
                return;
            }

            foreach (var type in nonVariantTypes)
            {
                p.PrintLine(GENERATED_CODE);
                p.PrintLine($"private readonly {CACHED_VARIANT_CONVERTER}<{type.fullTypeName}> _variantConverter{type.converterPropertyName} = {CACHED_VARIANT_CONVERTER}<{type.fullTypeName}>.Default;");
                p.PrintEndLine();
            }

            p.PrintEndLine();
        }

        private readonly void WriteListeners(ref Printer p)
        {
            if (bindingPropertyRefs.Count < 1)
            {
                return;
            }

            foreach (var member in bindingPropertyRefs)
            {
                p.PrintLine($"/// <summary>");
                p.PrintLine($"/// The listener that binds <see cref=\"{member.methodName}\"/>");
                p.PrintLine($"/// to the property chosen by <see cref=\"{BindingPropertyName(member)}\"/>.");
                p.PrintLine($"/// </summary>");
                p.PrintLine(GENERATED_CODE).PrintLine(EDITOR_BROWSABLE_NEVER);
                p.PrintLine($"private readonly PropertyChangeEventListener<{className}> {ListenerName(member)};");
                p.PrintEndLine();
            }

            p.PrintEndLine();
        }

        private readonly void WriteRelayCommands(ref Printer p)
        {
            if (bindingCommandRefs.Count < 1)
            {
                return;
            }

            foreach (var member in bindingCommandRefs)
            {
                p.PrintLine($"/// <summary>");
                p.PrintLine($"/// The relay command that binds <see cref=\"{member.methodName}\"/>");
                p.PrintLine($"/// to the command chosen by <see cref=\"{BindingCommandName(member)}\"/>.");
                p.PrintLine($"/// </summary>");
                p.PrintLine(GENERATED_CODE).PrintLine(EDITOR_BROWSABLE_NEVER);

                if (string.IsNullOrEmpty(member.paramFullTypeName))
                {
                    p.PrintLine($"private IRelayCommand {RelayCommandName(member)};");
                }
                else
                {
                    p.PrintLine($"private IRelayCommand<{member.paramFullTypeName}> {RelayCommandName(member)};");
                }

                p.PrintEndLine();
            }

            p.PrintEndLine();
        }

        private readonly void WriteFlags(ref Printer p)
        {
            if (bindingPropertyRefs.Count < 1 && bindingCommandRefs.Count < 1)
            {
                return;
            }

            p.PrintLine($"/// <summary>A flag indicates whether this binder is listening to events from <see cref=\"Context\"/>.</summary>");
            p.PrintLine(GENERATED_CODE);
            p.PrintLine($"private bool {IsListeningName(this)};");
            p.PrintEndLine();
        }

        private readonly void WriteConstructor(ref Printer p)
        {
            if (bindingPropertyRefs.Count < 1)
            {
                return;
            }

            p.PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
            p.PrintBeginLine().Print($"public {simpleTypeName}()");

            if (hasBaseBinder)
            {
                p.Print(" : base()");
            }

            p.PrintEndLine();

            p.OpenScope();
            {
                p.PrintLine($"OnBeforeConstructor();").PrintEndLine();

                foreach (var member in bindingPropertyRefs)
                {
                    var listenerMethodName = MethodName(member);

                    p.PrintLine($"this.{ListenerName(member)} = new PropertyChangeEventListener<{className}>(this)");
                    p.OpenScope();

                    if (string.IsNullOrEmpty(member.paramFullTypeName))
                    {
                        p.PrintLine($"OnEventAction = static (instance, args) => instance.{listenerMethodName}()");
                    }
                    else
                    {
                        p.PrintLine($"OnEventAction = static (instance, args) => instance.{listenerMethodName}(instance.{ConverterName(member)}.Convert(args.NewValue))");
                    }

                    p.CloseScope("};");
                    p.PrintEndLine();
                }

                p.PrintLine($"OnAfterConstructor();");
            }
            p.CloseScope();
            p.PrintEndLine();

            p.PrintLine($"/// <summary>Executes the logic at the beginning of the default constructor.</summary>");
            p.PrintLine($"/// <remarks>This method is invoked at the beginning of the default constructor.</remarks>");
            p.PrintLine(GENERATED_CODE);
            p.PrintLine($"partial void OnBeforeConstructor();");
            p.PrintEndLine();

            p.PrintLine($"/// <summary>Executes the logic at the end of the default constructor.</summary>");
            p.PrintLine($"/// <remarks>This method is invoked at the end of the default constructor.</remarks>");
            p.PrintLine(GENERATED_CODE);
            p.PrintLine($"partial void OnAfterConstructor();");
            p.PrintEndLine();
        }

        private readonly void WriteContextProperty(ref Printer p)
        {
            if (hasBaseBinder)
            {
                return;
            }

            var keyword = isSealed ? "" : "virtual ";

            p.PrintLine("/// <inheritdoc/>");
            p.PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
            p.PrintBeginLine("public ").Print(keyword)
                .Print($"IObservableObject Context")
                .PrintEndLine(" { get; private set; }");
            p.PrintEndLine();
        }

        private readonly void WriteStartListeningMethod(ref Printer p)
        {
            var keyword = hasBaseBinder ? "override " : (isSealed ? "" : "virtual ");

            if (bindingPropertyRefs.Count < 1 && bindingCommandRefs.Count < 1)
            {
                if (hasBaseBinder == false)
                {
                    p.PrintLine("/// <inheritdoc/>");
                    p.PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
                    p.PrintLine($"public {keyword}void StartListening() {{ }}");
                    p.PrintEndLine();
                }

                return;
            }

            p.PrintLine("/// <inheritdoc/>");
            p.PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
            p.PrintLine($"public {keyword}void StartListening()");
            p.OpenScope();
            {
                if (hasBaseBinder)
                {
                    p.PrintLine("base.StartListening();");
                    p.PrintEndLine();
                }

                p.PrintLine($"if (this.{IsListeningName(this)}) return;");
                p.PrintEndLine();

                p.PrintLine($"this.{IsListeningName(this)} = true;");
                p.PrintEndLine();

                if (bindingPropertyRefs.Count > 0)
                {
                    p.PrintLine($"if (this.Context is INotifyPropertyChanged inpc)");
                    p.OpenScope();
                    {
                        foreach (var member in bindingPropertyRefs)
                        {
                            p.PrintLine($"if (inpc.AttachPropertyChangedListener(this.{BindingPropertyName(member)}.TargetPropertyName, this.{ListenerName(member)}) == false)");
                            p.OpenScope();
                            {
                                p.PrintLine($"OnBindPropertyFailed(this.{BindingPropertyName(member)});");
                            }
                            p.CloseScope();
                            p.PrintEndLine();
                        }
                    }
                    p.CloseScope();
                    p.PrintEndLine();
                }

                if (bindingCommandRefs.Count > 0)
                {
                    p.PrintLine($"if (this.Context is ICommandListener cl)");
                    p.OpenScope();
                    {
                        foreach (var member in bindingCommandRefs)
                        {
                            p.PrintLine($"if (cl.TryGetCommand(this.{BindingCommandName(member)}.TargetCommandName, out this.{RelayCommandName(member)}) == false)");
                            p.OpenScope();
                            {
                                p.PrintLine($"OnBindCommandFailed(this.{BindingCommandName(member)});");
                            }
                            p.CloseScope();
                            p.PrintEndLine();
                        }
                    }
                    p.CloseScope();
                }
            }
            p.CloseScope();

            p.PrintEndLine();
        }

        private readonly void WritePartialOnBindFailedMethods(ref Printer p)
        {
            if (bindingPropertyRefs.Count > 0 && hasOnBindPropertyFailedMethod == false)
            {
                p.PrintLine($"partial void OnBindPropertyFailed(BindingProperty bindingProperty);");
                p.PrintEndLine();
            }

            if (bindingCommandRefs.Count > 0 && hasOnBindCommandFailedMethod == false)
            {
                p.PrintLine($"partial void OnBindCommandFailed(BindingCommand bindingCommand);");
                p.PrintEndLine();
            }
        }

        private readonly void WriteStopListeningMethod(ref Printer p)
        {
            var keyword = hasBaseBinder ? "override " : (isSealed ? "" : "virtual ");

            if (bindingPropertyRefs.Count < 1 && bindingCommandRefs.Count < 1)
            {
                if (hasBaseBinder == false)
                {
                    p.PrintLine("/// <inheritdoc/>");
                    p.PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
                    p.PrintLine($"public {keyword}void StopListening() {{ }}");
                    p.PrintEndLine();
                }

                return;
            }

            p.PrintLine("/// <inheritdoc/>");
            p.PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
            p.PrintLine($"public {keyword}void StopListening()");
            p.OpenScope();
            {
                if (hasBaseBinder)
                {
                    p.PrintLine("base.StopListening();");
                    p.PrintEndLine();
                }

                p.PrintLine($"if (this.{IsListeningName(this)} == false) return;");
                p.PrintEndLine();

                p.PrintLine($"this.{IsListeningName(this)} = false;");
                p.PrintEndLine();

                if (bindingPropertyRefs.Count > 0)
                {
                    foreach (var member in bindingPropertyRefs)
                    {
                        p.PrintLine($"this.{ListenerName(member)}.Detach();");
                    }

                    p.PrintEndLine();
                }

                if (bindingCommandRefs.Count > 0)
                {
                    foreach (var member in bindingCommandRefs)
                    {
                        p.PrintLine($"this.{RelayCommandName(member)} = null;");
                    }
                }
            }
            p.CloseScope();

            p.PrintEndLine();
        }

        private readonly void WriteSetTargetPropertyNameMethod(ref Printer p)
        {
            var keyword = hasBaseBinder ? "override " : (isSealed ? "" : "virtual ");

            if (bindingPropertyRefs.Count < 1)
            {
                if (hasBaseBinder == false)
                {
                    p.PrintLine("/// <inheritdoc/>");
                    p.PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
                    p.PrintLine($"public {keyword}bool SetTargetPropertyName(string bindingPropertyName, string targetPropertyName)");
                    p.OpenScope();
                    {
                        p.PrintLine("return false;");
                    }
                    p.CloseScope();
                    p.PrintEndLine();
                }

                return;
            }

            p.PrintLine("/// <inheritdoc/>");
            p.PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
            p.PrintLine($"public {keyword}bool SetTargetPropertyName(string bindingPropertyName, string targetPropertyName)");
            p.OpenScope();
            {
                if (hasBaseBinder)
                {
                    p.PrintLine("if (base.SetTargetPropertyName(bindingPropertyName, targetPropertyName)) return true;");
                    p.PrintEndLine();
                }

                p.PrintLine("switch (bindingPropertyName)");
                p.OpenScope();
                {
                    foreach (var member in bindingPropertyRefs)
                    {
                        p.PrintLine($"case {ConstName(member)}:");
                        p.OpenScope();
                        {
                            p.PrintLine($"this.{BindingPropertyName(member)}.TargetPropertyName = targetPropertyName;");
                            p.PrintLine("return true;");
                        }
                        p.CloseScope();
                        p.PrintEndLine();
                    }
                }
                p.CloseScope();
                p.PrintEndLine();

                p.PrintLine("return false;");
            }
            p.CloseScope();

            p.PrintEndLine();
        }

        private readonly void WriteSetAdapterMethod(ref Printer p)
        {
            var keyword = hasBaseBinder ? "override " : (isSealed ? "" : "virtual ");

            if (bindingPropertyRefs.Count < 1)
            {
                if (hasBaseBinder == false)
                {
                    p.PrintLine("/// <inheritdoc/>");
                    p.PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
                    p.PrintLine($"public {keyword}bool SetAdapter(string bindingPropertyName, {IADAPTER} adapter)");
                    p.OpenScope();
                    {
                        p.PrintLine("return false;");
                    }
                    p.CloseScope();
                    p.PrintEndLine();
                }

                return;
            }

            p.PrintLine("/// <inheritdoc/>");
            p.PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
            p.PrintLine($"public {keyword}bool SetAdapter(string bindingPropertyName, {IADAPTER} adapter)");
            p.OpenScope();
            {
                if (hasBaseBinder)
                {
                    p.PrintLine("if (base.SetAdapter(bindingPropertyName, adapter)) return true;");
                    p.PrintEndLine();
                }

                p.PrintLine("switch (bindingPropertyName)");
                p.OpenScope();
                {
                    foreach (var member in bindingPropertyRefs)
                    {
                        if (string.IsNullOrEmpty(member.paramFullTypeName))
                        {
                            continue;
                        }

                        p.PrintLine($"case {ConstName(member)}:");
                        p.OpenScope();
                        {
                            p.PrintLine($"this.{ConverterName(member)}.Adapter = adapter;");
                            p.PrintLine("return true;");
                        }
                        p.CloseScope();
                        p.PrintEndLine();
                    }
                }
                p.CloseScope();
                p.PrintEndLine();

                p.PrintLine("return false;");
            }
            p.CloseScope();

            p.PrintEndLine();
        }

        private readonly void WriteVariantOverloads(ref Printer p)
        {
            if (bindingPropertyRefs.Count < 1)
            {
                return;
            }

            foreach (var member in bindingPropertyRefs)
            {
                if (member.isParameterTypeVariant || string.IsNullOrEmpty(member.paramFullTypeName))
                {
                    continue;
                }

                var originalMethodName = member.methodName;
                var paramTypeName = member.paramFullTypeName;
                var variantMethodName = $"{member.methodName}__Variant";

                p.PrintLine($"/// <summary>");
                p.PrintLine($"/// This overload will try to get the value of type <see cref=\"{paramTypeName}\"/>");
                p.PrintLine($"/// from <see cref=\"global::EncosyTower.Variants.Variant\"/>");
                p.PrintLine($"/// to pass into <see cref=\"{originalMethodName}\"/>.");
                p.PrintLine($"/// </summary>");
                p.PrintLine($"/// <remarks>This method is not intended to be used directly by user code.</remarks>");
                p.PrintLine(EDITOR_BROWSABLE_NEVER).PrintLine(OBSOLETE_METHOD).PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
                p.PrintLine($"private void {variantMethodName}(in global::EncosyTower.Variants.Variant variant)");
                p.OpenScope();
                {
                    p.PrintLine($"if (this._variantConverter{member.variantConverterPropertyName}.TryGetValue(variant, out {paramTypeName} value))");
                    p.OpenScope();
                    {
                        p.PrintBeginLine().Print($"{originalMethodName}(");

                        if (member.paramRefKind == RefKind.Ref)
                        {
                            p.Print("ref ");
                        }

                        p.Print("value);").PrintEndLine();
                    }
                    p.CloseScope();
                }
                p.CloseScope();
                p.PrintEndLine();
            }

            p.PrintEndLine();
        }

        private readonly void WritePartialBindingCommandMethods(ref Printer p)
        {
            if (bindingCommandRefs.Count < 1)
            {
                return;
            }

            foreach (var member in bindingCommandRefs)
            {
                p.PrintLine(AGGRESSIVE_INLINING).PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);

                if (string.IsNullOrEmpty(member.paramFullTypeName))
                {
                    p.PrintLine($"partial void {member.methodName}()");
                    p.OpenScope();
                    {
                        p.PrintLine($"this.{RelayCommandName(member)}?.Execute();");
                    }
                    p.CloseScope();
                }
                else
                {
                    p.PrintBeginLine().Print($"partial void {member.methodName}(");

                    if (member.paramRefKind == RefKind.Ref)
                    {
                        p.Print("ref ");
                    }

                    p.Print($"{member.paramFullTypeName} {member.paramName})").PrintEndLine();
                    p.OpenScope();
                    {
                        p.PrintLine($"this.{RelayCommandName(member)}?.Execute({member.paramName});");
                    }
                    p.CloseScope();
                }

                p.PrintEndLine();
            }

            p.PrintEndLine();
        }

        private readonly void WriteSetTargetCommandNameMethod(ref Printer p)
        {
            var keyword = hasBaseBinder ? "override " : (isSealed ? "" : "virtual ");

            if (bindingCommandRefs.Count < 1)
            {
                if (hasBaseBinder == false)
                {
                    p.PrintLine("/// <inheritdoc/>");
                    p.PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
                    p.PrintLine($"public {keyword}bool SetTargetCommandName(string bindingCommandName, string targetCommandName)");
                    p.OpenScope();
                    {
                        p.PrintLine("return false;");
                    }
                    p.CloseScope();
                    p.PrintEndLine();
                }

                return;
            }

            p.PrintLine("/// <inheritdoc/>");
            p.PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
            p.PrintLine($"public {keyword}bool SetTargetCommandName(string bindingCommandName, string targetCommandName)");
            p.OpenScope();
            {
                if (hasBaseBinder)
                {
                    p.PrintLine("if (base.SetTargetCommandName(bindingCommandName, targetCommandName)) return true;");
                    p.PrintEndLine();
                }

                p.PrintLine("switch (bindingCommandName)");
                p.OpenScope();
                {
                    foreach (var member in bindingCommandRefs)
                    {
                        p.PrintLine($"case {ConstName(member)}:");
                        p.OpenScope();
                        {
                            p.PrintLine($"this.{BindingCommandName(member)}.TargetCommandName = targetCommandName;");
                            p.PrintLine("return true;");
                        }
                        p.CloseScope();
                        p.PrintEndLine();
                    }
                }
                p.CloseScope();
                p.PrintEndLine();

                p.PrintLine("return false;");
            }
            p.CloseScope();

            p.PrintEndLine();
        }

        private readonly void WriteRefreshContextMethod(ref Printer p)
        {
            var keyword = hasBaseBinder ? "override " : (isSealed ? "" : "virtual ");

            if (bindingPropertyRefs.Count < 1 && bindingCommandRefs.Count < 1)
            {
                if (hasBaseBinder == false)
                {
                    p.PrintLine("/// <inheritdoc/>");
                    p.PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
                    p.PrintLine($"public {keyword}void RefreshContext() {{ }}");
                    p.PrintEndLine();
                }

                return;
            }

            p.PrintLine("/// <inheritdoc/>");
            p.PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
            p.PrintLine($"public {keyword}void RefreshContext()");
            p.OpenScope();
            {
                if (hasBaseBinder)
                {
                    p.PrintLine("base.RefreshContext();");
                    p.PrintEndLine();
                }

                if (bindingPropertyRefs.Count > 0)
                {
                    p.PrintLine($"if (this.Context is INotifyPropertyChanged inpc)");
                    p.OpenScope();
                    {
                        foreach (var member in bindingPropertyRefs)
                        {
                            p.PrintLine($"if (inpc.NotifyPropertyChanged(this.{BindingPropertyName(member)}.TargetPropertyName, this.{ListenerName(member)}) == false)");
                            p.OpenScope();
                            {
                                p.PrintLine($"OnBindPropertyFailed(this.{BindingPropertyName(member)});");
                            }
                            p.CloseScope();
                            p.PrintEndLine();
                        }
                    }
                    p.CloseScope();
                    p.PrintEndLine();
                }
            }
            p.CloseScope();

            p.PrintEndLine();
        }

        // ── Private naming helpers ────────────────────────────────────────────────

        private static string IsListeningName(in BinderDeclaration dec)
            => $"_isListening_{dec.typeIdentifier}";

        private static string ConstName(in BindingPropertyInfo member)
            => $"BindingProperty_{member.methodName}";

        private static string ConstName(in BindingCommandInfo member)
            => $"BindingCommand_{member.methodName}";

        private static string BindingPropertyName(in BindingPropertyInfo member)
            => $"_bindingFieldFor{member.methodName}";

        private static string ConverterName(in BindingPropertyInfo member)
            => $"_converterFor{member.methodName}";

        private static string ListenerName(in BindingPropertyInfo member)
            => $"_listenerFor{member.methodName}";

        /// <summary>
        /// Returns the method name used in the constructor listener action.
        /// For non-Variant parameter types, a private <c>__Variant</c> proxy is called instead
        /// of the user's original method.
        /// </summary>
        private static string MethodName(in BindingPropertyInfo member)
            => member.isParameterTypeVariant ? member.methodName : $"{member.methodName}__Variant";

        private static string BindingCommandName(in BindingCommandInfo member)
            => $"_bindingCommandFor{member.methodName}";

        private static string RelayCommandName(in BindingCommandInfo member)
            => $"_relayCommandFor{member.methodName}";
    }
}
