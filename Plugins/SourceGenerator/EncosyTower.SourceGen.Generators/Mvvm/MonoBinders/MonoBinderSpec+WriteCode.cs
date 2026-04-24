using System.Collections.Immutable;
using EncosyTower.SourceGen.Generators.Mvvm.Binders;
using Microsoft.CodeAnalysis;

namespace EncosyTower.SourceGen.Generators.Mvvm.MonoBinders
{
    partial struct MonoBinderSpec
    {
        private const string BINDER_ATTR = "[ETMVB.Binder]";
        private const string SERIALIZABLE_ATTR = "[S.Serializable]";
        private const string BINDING_PROP_ATTR = "[ETMVB.BindingProperty]";
        private const string BINDING_CMD_ATTR = "[ETMVB.BindingCommand]";
        private const string HIDE_IN_INSPECTOR = "[field: UE.HideInInspector]";
        private const string MONO_BINDER_BASE = "ETMVBC.MonoBinder";
        private const string MONO_BINDING_PROP = "ETMVBC.MonoBindingProperty";
        private const string MONO_BINDING_CMD = "ETMVBC.MonoBindingCommand";
        private const string UNITY_ACTION = "UEE.UnityAction";
        private const string LABEL_ATTR = "ETA.Label";

        private const string AGGRESSIVE_INLINING = "[SRCS.MethodImpl(SRCS.MethodImplOptions.AggressiveInlining)]";
        private const string EXCLUDE_COVERAGE = "[SDCA.ExcludeFromCodeCoverage]";
        private const string GENERATED_CODE = $"[SCDC.GeneratedCode(\"EncosyTower.SourceGen.Generators.Mvvm.MonoBinders.MonoBinderGenerator\", \"{SourceGenVersion.VALUE}\")]";
        private const string OBSOLETE_METHOD = "[S.Obsolete(\"This method is not intended to be used directly by user code.\")]";
        private const string EDITOR_BROWSABLE_NEVER = "[SCM.EditorBrowsable(SCM.EditorBrowsableState.Never)]";
        private const string GENERATED_BINDING_PROPERTY = "[ETMVBSG.GeneratedBindingProperty({0}, typeof({1}))]";
        private const string GENERATED_BINDING_COMMAND = "[ETMVBSG.GeneratedBindingCommand(";
        private const string GENERATED_CONVERTER = "[ETMVBSG.GeneratedConverter({0}, typeof({1}))]";
        private const string IADAPTER = "ETMVB.IAdapter";
        private const string CACHED_VARIANT_CONVERTER = "ETVC.CachedVariantConverter";

        public readonly string WriteCode()
        {
            var p = Printer.DefaultLarge;

            p.PrintEndLine();
            p.Print("#pragma warning disable").PrintEndLine();
            p.PrintEndLine();

            if (string.IsNullOrEmpty(preprocessorGuard) == false)
            {
                p.Print("#if ").PrintEndLine(preprocessorGuard);
                p.PrintEndLine();
            }

            p = p.IncreasedIndent();

            WriteMonoBinderContainer(ref p);
            WriteOuterClassIBinder(ref p);

            p = p.DecreasedIndent();

            if (string.IsNullOrEmpty(preprocessorGuard) == false)
            {
                p.PrintEndLine();
                p.Print("#endif // ").PrintEndLine(preprocessorGuard);
            }

            return p.Result;
        }

        private readonly void WriteMonoBinderContainer(ref Printer p)
        {
            p.PrintLine(SERIALIZABLE_ATTR);
            p.PrintLine(BINDER_ATTR);
            p.PrintBeginLine("[").Print(LABEL_ATTR).Print("(\"").Print(componentLabelName).PrintEndLine("\")]");
            p.PrintBeginLine("partial class ").Print(userClassName)
                .Print(" : ").Print(MONO_BINDER_BASE).Print("<").Print(componentFullTypeName).PrintEndLine(">");
            p.OpenScope();
            {
                WriteCustomSetterPartials(ref p);
                WriteWrapperPartials(ref p);
                WritePropertyBindings(ref p);
                WriteCommandBindings(ref p);
            }
            p.CloseScope();
            p.PrintEndLine();
        }

        private readonly void WritePropertyBindings(ref Printer p)
        {
            foreach (var binding in propertyBindings)
            {
                if (binding.skipGeneration)
                    continue;

                WritePropertyBinding(ref p, binding);
            }
        }

        private readonly void WritePropertyBinding(ref Printer p, in PropertyBindingSpec b)
        {
            p.PrintLine(SERIALIZABLE_ATTR);
            p.PrintLine(BINDER_ATTR);

            if (b.isObsolete)
            {
                p.PrintBeginLine("[S.Obsolete(\"")
                    .Print(EscapeStringLiteral(b.obsoleteMessage))
                    .PrintEndLine("\")]");
            }
            else
            {
                p.PrintBeginLine("[").Print(LABEL_ATTR).Print("(\"").Print(b.label)
                    .Print("\", \"").Print(componentLabelName).PrintEndLine("\")]");
            }

            p.PrintBeginLine(EXCLUDE_COVERAGE).PrintEndLine(GENERATED_CODE);
            p.PrintBeginLine("public sealed partial class ").Print(b.generatedClassName)
                .Print(" : ").Print(MONO_BINDING_PROP).Print("<").Print(componentFullTypeName).PrintEndLine(">");
            p.OpenScope();
            {
                p.PrintLine(BINDING_PROP_ATTR);
                p.PrintLine(HIDE_IN_INSPECTOR);

                var inModifier = b.needsInModifier ? "in " : string.Empty;
                p.PrintBeginLine("private void ").Print(b.setterMethodName)
                    .Print("(").Print(inModifier).Print(b.propFullTypeName).PrintEndLine(" value)");
                p.OpenScope();
                {
                    p.PrintLine("var targets = Targets;");
                    p.PrintLine("var length = targets.Length;");
                    p.PrintEndLine();
                    p.PrintLine("for (var i = 0; i < length; i++)");
                    p.OpenScope();
                    {
                        if (b.useCustomSetter)
                        {
                            p.PrintBeginLine(userClassName).Print(".")
                                .Print(b.customSetterPartialMethodName).PrintEndLine("(targets[i], value);");
                        }
                        else if (string.IsNullOrEmpty(b.setterMethod))
                        {
                            p.PrintBeginLine("targets[i].").Print(b.memberName).PrintEndLine(" = value;");
                        }
                        else
                        {
                            p.PrintBeginLine("targets[i].").Print(b.setterMethod).PrintEndLine("(value);");
                        }
                    }
                    p.CloseScope();
                }
                p.CloseScope();
            }
            p.CloseScope();
            p.PrintEndLine();
        }

        private readonly void WriteCommandBindings(ref Printer p)
        {
            foreach (var binding in commandBindings)
            {
                if (binding.skipGeneration)
                    continue;

                WriteCommandBinding(ref p, binding);
            }
        }

        private readonly void WriteCustomSetterPartials(ref Printer p)
        {
            foreach (var binding in propertyBindings)
            {
                if (binding.useCustomSetter == false || binding.skipGeneration)
                    continue;

                var inModifier = binding.needsInModifier ? "in " : string.Empty;

                p.PrintBeginLine("private static partial void ")
                    .Print(binding.customSetterPartialMethodName)
                    .Print("(")
                    .Print(componentFullTypeName)
                    .Print(" target, ")
                    .Print(inModifier)
                    .Print(binding.propFullTypeName)
                    .PrintEndLine(" value);");
            }
        }

        private readonly void WriteCommandBinding(ref Printer p, in CommandBindingSpec b)
        {
            var hasWrapper = !string.IsNullOrEmpty(b.wrapperTypeName);
            var argCount = b.actionTypeArgs.Count;

            p.PrintLine(SERIALIZABLE_ATTR);
            p.PrintLine(BINDER_ATTR);

            if (b.isObsolete)
            {
                p.PrintBeginLine("[S.Obsolete(\"")
                    .Print(EscapeStringLiteral(b.obsoleteMessage))
                    .PrintEndLine("\")]");
            }
            else
            {
                p.PrintBeginLine("[").Print(LABEL_ATTR).Print("(\"").Print(b.label)
                    .Print("\", \"").Print(componentLabelName).PrintEndLine("\")]");
            }

            p.PrintBeginLine(EXCLUDE_COVERAGE).PrintEndLine(GENERATED_CODE);
            p.PrintBeginLine("public sealed partial class ").Print(b.generatedClassName)
                .Print(" : ").Print(MONO_BINDING_CMD).Print("<").Print(componentFullTypeName).PrintEndLine(">");
            p.OpenScope();
            {
                p.PrintBeginLine("private readonly ");
                PrintCommandFieldType(ref p, b);
                p.PrintEndLine(" _command;");
                p.PrintEndLine();

                p.PrintBeginLine("public ").Print(b.generatedClassName).PrintEndLine("()");
                p.OpenScope();
                {
                    if (hasWrapper && argCount > 1)
                    {
                        p.PrintBeginLine("_command = (");

                        for (var i = 0; i < argCount; i++)
                        {
                            p.PrintIf(i > 0, ", ").Print("p").Print(i);
                        }

                        p.Print(") => ").Print(b.callbackMethodName)
                            .Print("(").Print(userClassName).Print(".Wrap_")
                            .Print(b.memberPascalName).Print("(");

                        for (var i = 0; i < argCount; i++)
                        {
                            p.PrintIf(i > 0, ", ").Print("p").Print(i);
                        }

                        p.PrintEndLine("));");
                    }
                    else
                    {
                        p.PrintBeginLine("_command = ").Print(b.callbackMethodName).PrintEndLine(";");
                    }
                }
                p.CloseScope();
                p.PrintEndLine();

                p.PrintLine("protected override void OnBeforeSetTargets()");
                p.OpenScope();
                {
                    p.PrintLine("var targets = Targets;");
                    p.PrintLine("var length = targets.Length;");
                    p.PrintLine("var command = _command;");
                    p.PrintEndLine();
                    p.PrintLine("for (var i = 0; i < length; i++)");
                    p.OpenScope();
                    {
                        if (b.isUnityEvent)
                        {
                            p.PrintBeginLine("targets[i].").Print(b.memberName)
                                .PrintEndLine(".RemoveListener(command);");
                        }
                        else
                        {
                            p.PrintBeginLine("targets[i].").Print(b.memberName)
                                .PrintEndLine(" -= command;");
                        }
                    }
                    p.CloseScope();
                }
                p.CloseScope();
                p.PrintEndLine();

                p.PrintLine("protected override void OnAfterSetTargets()");
                p.OpenScope();
                {
                    p.PrintLine("var targets = Targets;");
                    p.PrintLine("var length = targets.Length;");
                    p.PrintLine("var command = _command;");
                    p.PrintEndLine();
                    p.PrintLine("for (var i = 0; i < length; i++)");
                    p.OpenScope();
                    {
                        if (b.isUnityEvent)
                        {
                            p.PrintBeginLine("targets[i].").Print(b.memberName)
                                .PrintEndLine(".AddListener(command);");
                        }
                        else
                        {
                            p.PrintBeginLine("targets[i].").Print(b.memberName)
                                .PrintEndLine(" += command;");
                        }
                    }
                    p.CloseScope();
                }
                p.CloseScope();
                p.PrintEndLine();

                p.PrintBeginLine(HIDE_IN_INSPECTOR).PrintEndLine(BINDING_CMD_ATTR);

                if (hasWrapper && argCount > 1)
                {
                    p.PrintBeginLine("partial void ").Print(b.callbackMethodName)
                        .Print("(").Print(b.wrapperTypeName).PrintEndLine(" value);");
                }
                else if (argCount == 0)
                {
                    p.PrintBeginLine("partial void ").Print(b.callbackMethodName).PrintEndLine("();");
                }
                else if (argCount == 1)
                {
                    p.PrintBeginLine("partial void ").Print(b.callbackMethodName)
                        .Print("(").Print(b.actionTypeArgs[0]).PrintEndLine(" value);");
                }
                else
                {
                    p.PrintBeginLine("partial void ").Print(b.callbackMethodName).Print("(");
                    PrintMethodParamList(ref p, b.actionTypeArgs);
                    p.PrintEndLine(");");
                }
            }
            p.CloseScope();
            p.PrintEndLine();
        }

        private readonly void WriteWrapperPartials(ref Printer p)
        {
            foreach (var b in commandBindings)
            {
                var hasWrapper = !string.IsNullOrEmpty(b.wrapperTypeName);
                var argCount = b.actionTypeArgs.Count;

                if (b.skipGeneration || hasWrapper == false || argCount <= 1)
                    continue;

                p.PrintBeginLine("private static partial ")
                    .Print(b.wrapperTypeName)
                    .Print(" Wrap_")
                    .Print(b.memberPascalName)
                    .Print("(");
                PrintMethodParamList(ref p, b.actionTypeArgs);
                p.PrintEndLine(");");
                p.PrintEndLine();

                p.PrintBeginLine("private static partial void Unwrap_")
                    .Print(b.memberPascalName)
                    .Print("(")
                    .Print(b.wrapperTypeName)
                    .Print(" value");

                for (var i = 0; i < argCount; i++)
                {
                    p.Print(", out ").Print(b.actionTypeArgs[i]).Print(" p").Print(i);
                }

                p.PrintEndLine(");");
                p.PrintEndLine();
            }
        }

        private static void PrintCommandFieldType(ref Printer p, in CommandBindingSpec b)
        {
            if (b.isUnityEvent == false)
            {
                p.Print(b.delegateFullTypeName);
                return;
            }

            var typeArgs = b.actionTypeArgs;

            if (typeArgs.Count == 0)
            {
                p.Print(UNITY_ACTION);
                return;
            }

            p.Print(UNITY_ACTION).Print("<");

            for (var i = 0; i < typeArgs.Count; i++)
            {
                p.PrintIf(i > 0, ", ").Print(typeArgs[i]);
            }

            p.Print(">");
        }

        private static void PrintMethodParamList(ref Printer p, EquatableArray<string> typeArgs)
        {
            for (var i = 0; i < typeArgs.Count; i++)
            {
                p.PrintIf(i > 0, ", ").Print(typeArgs[i]).Print(" p").Print(i);
            }
        }

        private static string EscapeStringLiteral(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return string.Empty;
            }

            return value
                .Replace("\\", "\\\\")
                .Replace("\"", "\\\"")
                .Replace("\r", "\\r")
                .Replace("\n", "\\n");
        }

        private static string IsListeningName(string typeIdentifier)
            => $"_isListening_{typeIdentifier}";

        private static string PropConstName(string methodName)
            => $"BindingProperty_{methodName}";

        private static string CmdConstName(string methodName)
            => $"BindingCommand_{methodName}";

        private static string PropFieldName(string methodName)
            => $"_bindingFieldFor{methodName}";

        private static string ConverterFieldName(string methodName)
            => $"_converterFor{methodName}";

        private static string ListenerFieldName(string methodName)
            => $"_listenerFor{methodName}";

        private static string CmdFieldName(string methodName)
            => $"_bindingCommandFor{methodName}";

        private static string RelayCommandFieldName(string methodName)
            => $"_relayCommandFor{methodName}";

        private readonly void WriteOuterClassIBinder(ref Printer p)
        {
            WriteBpMethodInfoAttributes(ref p, userBindingPropertyRefs);
            WriteBcMethodInfoAttributes(ref p, userBindingCommandRefs);

            p.PrintBeginLine("partial class ").Print(userClassName).PrintEndLine(" : ETMVB.IBinder");
            p.OpenScope();
            {
                WriteBinderBody(
                      ref p
                    , userClassName
                    , userClassName
                    , outerTypeIdentifier
                    , hasOnBindPropertyFailedMethod
                    , hasOnBindCommandFailedMethod
                    , userBindingPropertyRefs
                    , userBindingCommandRefs
                    , outerNonVariantTypes
                );
                WritePropertySubclassIBinders(ref p);
                WriteCommandSubclassIBinders(ref p);
            }
            p.CloseScope();
            p.PrintEndLine();
        }

        private readonly void WritePropertySubclassIBinders(ref Printer p)
        {
            foreach (var b in propertyBindings)
            {
                if (b.skipGeneration)
                    continue;

                WritePropertySubclassIBinder(ref p, in b);
            }
        }

        private readonly void WritePropertySubclassIBinder(ref Printer p, in PropertyBindingSpec b)
        {
            var methodName = b.setterMethodName;
            var paramType = b.propFullTypeName;
            var isVariant = paramType == BinderSpec.VARIANT_TYPE;
            var vcPropName = b.variantConverterPropertyName;

            var propInfo = new BinderSpec.BindingPropertySpec {
                methodName = methodName,
                paramFullTypeName = paramType,
                paramRefKind = b.needsInModifier ? RefKind.In : RefKind.None,
                variantConverterPropertyName = vcPropName,
                isParameterTypeVariant = isVariant,
                skipBindingProperty = false,
                skipConverter = false,
                forwardedFieldAttributes = default,
            };

            var propRefs = ImmutableArray.Create(propInfo).AsEquatableArray();
            var cmdRefs = ImmutableArray<BinderSpec.BindingCommandSpec>.Empty.AsEquatableArray();

            EquatableArray<BinderSpec.NonVariantTypeSpec> nvRefs;

            if (isVariant == false && string.IsNullOrEmpty(paramType) == false)
            {
                var nvInfo = new BinderSpec.NonVariantTypeSpec {
                    fullTypeName = paramType,
                    converterPropertyName = vcPropName,
                };
                nvRefs = ImmutableArray.Create(nvInfo).AsEquatableArray();
            }
            else
            {
                nvRefs = ImmutableArray<BinderSpec.NonVariantTypeSpec>.Empty.AsEquatableArray();
            }

            p.PrintBeginLine()
                .Print("[ETMVBSG.BindingPropertyMethodInfo(")
                .Print("\"").Print(methodName).Print("\", typeof(")
                .Print(paramType)
                .PrintEndLine("))]");

            p.PrintBeginLine("partial class ").Print(b.generatedClassName).PrintEndLine(" : ETMVB.IBinder");
            p.OpenScope();
            {
                WriteBinderBody(
                      ref p
                    , b.generatedClassName
                    , b.generatedClassName
                    , b.generatedClassName
                    , false
                    , false
                    , propRefs
                    , cmdRefs
                    , nvRefs
                );
            }
            p.CloseScope();
            p.PrintEndLine();
        }

        private readonly void WriteCommandSubclassIBinders(ref Printer p)
        {
            foreach (var b in commandBindings)
            {
                if (b.skipGeneration)
                    continue;

                WriteCommandSubclassIBinder(ref p, in b);
            }
        }

        private readonly void WriteCommandSubclassIBinder(ref Printer p, in CommandBindingSpec b)
        {
            var methodName = b.callbackMethodName;
            var argCount = b.actionTypeArgs.Count;
            var hasWrapper = string.IsNullOrEmpty(b.wrapperTypeName) == false;

            string effectiveParamType;

            if (argCount == 0)
                effectiveParamType = string.Empty;
            else if (argCount == 1)
                effectiveParamType = b.actionTypeArgs[0];
            else if (hasWrapper)
                effectiveParamType = b.wrapperTypeName;
            else
                effectiveParamType = string.Empty;

            var cmdInfo = new BinderSpec.BindingCommandSpec {
                methodName = methodName,
                paramFullTypeName = effectiveParamType,
                paramRefKind = RefKind.None,
                paramName = string.IsNullOrEmpty(effectiveParamType) ? string.Empty : "value",
                skipBindingCommand = false,
                forwardedFieldAttributes = default,
            };

            var propRefs = ImmutableArray<BinderSpec.BindingPropertySpec>.Empty.AsEquatableArray();
            var cmdRefs = ImmutableArray.Create(cmdInfo).AsEquatableArray();
            var nvRefs = ImmutableArray<BinderSpec.NonVariantTypeSpec>.Empty.AsEquatableArray();

            p.PrintBeginLine()
                .Print("[ETMVBSG.BindingCommandMethodInfo(")
                .Print("\"").Print(methodName).Print("\", ");

            if (string.IsNullOrEmpty(effectiveParamType))
                p.Print("typeof(void)");
            else
                p.Print("typeof(").Print(effectiveParamType).Print(")");

            p.Print(")]").PrintEndLine();

            p.PrintBeginLine("partial class ").Print(b.generatedClassName).PrintEndLine(" : ETMVB.IBinder");
            p.OpenScope();
            {
                WriteBinderBody(
                      ref p
                    , b.generatedClassName
                    , b.generatedClassName
                    , b.generatedClassName
                    , false
                    , false
                    , propRefs
                    , cmdRefs
                    , nvRefs
                );
            }
            p.CloseScope();
            p.PrintEndLine();
        }

        private static void WriteBinderBody(
              ref Printer p
            , string className
            , string simpleTypeName
            , string typeIdentifier
            , bool hasOnBindPropertyFailedMethod
            , bool hasOnBindCommandFailedMethod
            , in EquatableArray<BinderSpec.BindingPropertySpec> propRefs
            , in EquatableArray<BinderSpec.BindingCommandSpec> cmdRefs
            , in EquatableArray<BinderSpec.NonVariantTypeSpec> nvTypes
        )
        {
            var keyword = "override ";
            var isListeningField = IsListeningName(typeIdentifier);

            WriteBinderConstantProperties(ref p, className, propRefs);
            WriteBinderConstantCommands(ref p, className, cmdRefs);
            WriteBinderPropertyFields(ref p, propRefs);
            WriteBinderConverterFields(ref p, propRefs);
            WriteBinderCommandFields(ref p, cmdRefs);
            WriteBinderVariantConverterFields(ref p, nvTypes);
            WriteBinderListenerFields(ref p, className, propRefs);
            WriteBinderRelayCommandFields(ref p, cmdRefs);
            WriteBinderListeningFlag(ref p, isListeningField, propRefs, cmdRefs);
            WriteBinderConstructor(ref p, className, simpleTypeName, propRefs);
            WriteBinderStartListening(ref p, keyword, isListeningField, propRefs, cmdRefs);
            WriteBinderOnBindFailedPartials(ref p, hasOnBindPropertyFailedMethod, hasOnBindCommandFailedMethod, propRefs, cmdRefs);
            WriteBinderStopListening(ref p, keyword, isListeningField, propRefs, cmdRefs);
            WriteBinderSetTargetPropertyName(ref p, keyword, propRefs);
            WriteBinderSetAdapter(ref p, keyword, propRefs);

            if (nvTypes.Count > 0)
            {
                WriteBinderVariantOverloads(ref p, propRefs);
            }

            WriteBinderPartialCommandMethods(ref p, cmdRefs);
            WriteBinderSetTargetCommandName(ref p, keyword, cmdRefs);
            WriteBinderRefreshContext(ref p, keyword, propRefs);
        }

        private static void WriteBpMethodInfoAttributes(
              ref Printer p
            , in EquatableArray<BinderSpec.BindingPropertySpec> propRefs
        )
        {
            foreach (var m in propRefs)
            {
                var paramType = string.IsNullOrEmpty(m.paramFullTypeName) ? "void" : m.paramFullTypeName;

                p.PrintBeginLine()
                    .Print("[ETMVBSG.BindingPropertyMethodInfo(")
                    .Print("\"").Print(m.methodName).Print("\", typeof(")
                    .Print(paramType)
                    .PrintEndLine("))]");
            }
        }

        private static void WriteBcMethodInfoAttributes(
              ref Printer p
            , in EquatableArray<BinderSpec.BindingCommandSpec> cmdRefs
        )
        {
            foreach (var m in cmdRefs)
            {
                p.PrintBeginLine()
                    .Print("[ETMVBSG.BindingCommandMethodInfo(")
                    .Print("\"").Print(m.methodName).Print("\", ");

                if (string.IsNullOrEmpty(m.paramFullTypeName))
                    p.Print("typeof(void)");
                else
                    p.Print("typeof(").Print(m.paramFullTypeName).Print(")");

                p.Print(")]").PrintEndLine();
            }
        }

        private static void WriteBinderConstantProperties(
              ref Printer p
            , string className
            , in EquatableArray<BinderSpec.BindingPropertySpec> propRefs
        )
        {
            foreach (var m in propRefs)
            {
                p.PrintBeginLine("/// <summary>The name of <see cref=\"").Print(m.methodName).PrintEndLine("m.methodName");
                p.PrintBeginLine("public const string ").Print(PropConstName(m.methodName))
                    .Print(" = nameof(").Print(className).Print(".").Print(m.methodName).PrintEndLine(");");
                p.PrintEndLine();
            }

            if (propRefs.Count > 0) p.PrintEndLine();
        }

        private static void WriteBinderConstantCommands(
              ref Printer p
            , string className
            , in EquatableArray<BinderSpec.BindingCommandSpec> cmdRefs
        )
        {
            foreach (var m in cmdRefs)
            {
                p.PrintBeginLine("/// <summary>The name of <see cref=\"").Print(m.methodName).PrintEndLine("\"/></summary>");
                p.PrintBeginLine("public const string ").Print(CmdConstName(m.methodName))
                    .Print(" = nameof(").Print(className).Print(".").Print(m.methodName).PrintEndLine(");");
                p.PrintEndLine();
            }

            if (cmdRefs.Count > 0) p.PrintEndLine();
        }

        private static void WriteBinderPropertyFields(
              ref Printer p
            , in EquatableArray<BinderSpec.BindingPropertySpec> propRefs
        )
        {
            foreach (var m in propRefs)
            {
                if (m.skipBindingProperty)
                    continue;

                p.PrintLine("[UE.SerializeField]");

                foreach (var attr in m.forwardedFieldAttributes)
                {
                    p.PrintBeginLine("[").Print(attr.GetSyntax().ToFullString()).PrintEndLine("]");
                }

                var paramType = string.IsNullOrEmpty(m.paramFullTypeName) ? "void" : m.paramFullTypeName;
                p.PrintLine(string.Format(GENERATED_BINDING_PROPERTY, PropConstName(m.methodName), paramType));
                p.PrintBeginLine("private ETMVB.BindingProperty ")
                    .Print(PropFieldName(m.methodName)).PrintEndLine(" = new ETMVB.BindingProperty();");
                p.PrintEndLine();
            }

            if (propRefs.Count > 0) p.PrintEndLine();
        }

        private static void WriteBinderConverterFields(
              ref Printer p
            , in EquatableArray<BinderSpec.BindingPropertySpec> propRefs
        )
        {
            foreach (var m in propRefs)
            {
                if (m.skipConverter || string.IsNullOrEmpty(m.paramFullTypeName))
                    continue;

                p.PrintLine("[UE.SerializeField]");

                foreach (var attr in m.forwardedFieldAttributes)
                {
                    p.PrintBeginLine("[").Print(attr.GetSyntax().ToFullString()).PrintEndLine("]");
                }

                p.PrintLine(string.Format(GENERATED_CONVERTER, PropConstName(m.methodName), m.paramFullTypeName));
                p.PrintBeginLine("private ETMVB.Converter ")
                    .Print(ConverterFieldName(m.methodName)).PrintEndLine(" = new ETMVB.Converter();");
                p.PrintEndLine();
            }

            if (propRefs.Count > 0) p.PrintEndLine();
        }

        private static void WriteBinderCommandFields(
              ref Printer p
            , in EquatableArray<BinderSpec.BindingCommandSpec> cmdRefs
        )
        {
            foreach (var m in cmdRefs)
            {
                if (m.skipBindingCommand)
                    continue;

                p.PrintLine("[UE.SerializeField]");

                foreach (var attr in m.forwardedFieldAttributes)
                {
                    p.PrintBeginLine("[").Print(attr.GetSyntax().ToFullString()).PrintEndLine("]");
                }

                p.PrintBeginLine()
                    .Print(GENERATED_BINDING_COMMAND)
                    .Print(CmdConstName(m.methodName));

                if (string.IsNullOrEmpty(m.paramFullTypeName))
                    p.Print(", typeof(void)");
                else
                    p.Print(", typeof(").Print(m.paramFullTypeName).Print(")");

                p.Print(")]").PrintEndLine();
                p.PrintBeginLine("private ETMVB.BindingCommand ")
                    .Print(CmdFieldName(m.methodName)).PrintEndLine(" = new ETMVB.BindingCommand();");
                p.PrintEndLine();
            }

            if (cmdRefs.Count > 0) p.PrintEndLine();
        }

        private static void WriteBinderVariantConverterFields(
              ref Printer p
            , in EquatableArray<BinderSpec.NonVariantTypeSpec> nvTypes
        )
        {
            foreach (var t in nvTypes)
            {
                p.PrintBeginLine("private readonly ").Print(CACHED_VARIANT_CONVERTER)
                    .Print("<").Print(t.fullTypeName).Print("> _variantConverter")
                    .Print(t.converterPropertyName).Print(" = ")
                    .Print(CACHED_VARIANT_CONVERTER).Print("<")
                    .Print(t.fullTypeName).PrintEndLine(">.Default;");
                p.PrintEndLine();
            }

            if (nvTypes.Count > 0) p.PrintEndLine();
        }

        private static void WriteBinderListenerFields(
              ref Printer p
            , string className
            , in EquatableArray<BinderSpec.BindingPropertySpec> propRefs
        )
        {
            foreach (var m in propRefs)
            {
                p.PrintLine(EDITOR_BROWSABLE_NEVER);
                p.PrintBeginLine("private readonly ETMCM.PropertyChangeEventListener<")
                    .Print(className).Print("> ").Print(ListenerFieldName(m.methodName)).PrintEndLine(";");
                p.PrintEndLine();
            }

            if (propRefs.Count > 0) p.PrintEndLine();
        }

        private static void WriteBinderRelayCommandFields(
              ref Printer p
            , in EquatableArray<BinderSpec.BindingCommandSpec> cmdRefs
        )
        {
            foreach (var m in cmdRefs)
            {
                p.PrintLine(EDITOR_BROWSABLE_NEVER);

                if (string.IsNullOrEmpty(m.paramFullTypeName))
                {
                    p.PrintBeginLine("private ETMI.IRelayCommand ")
                        .Print(RelayCommandFieldName(m.methodName)).PrintEndLine(";");
                }
                else
                {
                    p.PrintBeginLine("private ETMI.IRelayCommand<")
                        .Print(m.paramFullTypeName).Print("> ")
                        .Print(RelayCommandFieldName(m.methodName)).PrintEndLine(";");
                }

                p.PrintEndLine();
            }

            if (cmdRefs.Count > 0) p.PrintEndLine();
        }

        private static void WriteBinderListeningFlag(
              ref Printer p
            , string isListeningField
            , in EquatableArray<BinderSpec.BindingPropertySpec> propRefs
            , in EquatableArray<BinderSpec.BindingCommandSpec> cmdRefs
        )
        {
            if (propRefs.Count < 1 && cmdRefs.Count < 1)
                return;

            p.PrintBeginLine("private bool ").Print(isListeningField).PrintEndLine(";");
            p.PrintEndLine();
        }

        private static void WriteBinderConstructor(
              ref Printer p
            , string className
            , string simpleTypeName
            , in EquatableArray<BinderSpec.BindingPropertySpec> propRefs
        )
        {
            if (propRefs.Count < 1)
                return;

            p.PrintBeginLine("public ").Print(simpleTypeName).Print("()");
            p.Print(" : base()");
            p.PrintEndLine();
            p.OpenScope();
            {
                p.PrintLine("OnBeforeConstructor();").PrintEndLine();

                foreach (var m in propRefs)
                {
                    p.PrintBeginLine("this.").Print(ListenerFieldName(m.methodName))
                        .Print(" = new ETMCM.PropertyChangeEventListener<").Print(className).PrintEndLine(">(this)");
                    p.OpenScope();

                    if (string.IsNullOrEmpty(m.paramFullTypeName))
                    {
                        p.PrintBeginLine("OnEventAction = static (instance, args) => instance.")
                            .Print(m.methodName).PrintIf(m.isParameterTypeVariant == false, "__Variant")
                            .PrintEndLine("()");
                    }
                    else
                    {
                        p.PrintBeginLine("OnEventAction = static (instance, args) => instance.")
                            .Print(m.methodName).PrintIf(m.isParameterTypeVariant == false, "__Variant")
                            .Print("(instance.").Print(ConverterFieldName(m.methodName))
                            .PrintEndLine(".Convert(args.NewValue))");
                    }

                    p.CloseScope("};");
                    p.PrintEndLine();
                }

                p.PrintLine("OnAfterConstructor();");
            }
            p.CloseScope();
            p.PrintEndLine();

            p.PrintLine("partial void OnBeforeConstructor();");
            p.PrintEndLine();

            p.PrintLine("partial void OnAfterConstructor();");
            p.PrintEndLine();
        }

        private static void WriteBinderStartListening(
              ref Printer p
            , string keyword
            , string isListeningField
            , in EquatableArray<BinderSpec.BindingPropertySpec> propRefs
            , in EquatableArray<BinderSpec.BindingCommandSpec> cmdRefs
        )
        {
            if (propRefs.Count < 1 && cmdRefs.Count < 1)
            {
                return;
            }

            p.PrintLine("/// <inheritdoc/>");
            p.PrintBeginLine("public ").Print(keyword).PrintEndLine("void StartListening()");
            p.OpenScope();
            {
                p.PrintLine("base.StartListening();");
                p.PrintEndLine();

                p.PrintBeginLine("if (this.").Print(isListeningField).PrintEndLine(") return;");
                p.PrintEndLine();
                p.PrintBeginLine("this.").Print(isListeningField).PrintEndLine(" = true;");
                p.PrintEndLine();

                if (propRefs.Count > 0)
                {
                    p.PrintLine("if (this.Context is ETMCM.INotifyPropertyChanged inpc)");
                    p.OpenScope();
                    {
                        foreach (var m in propRefs)
                        {
                            p.PrintBeginLine("if (inpc.AttachPropertyChangedListener(this.")
                                .Print(PropFieldName(m.methodName)).Print(".TargetPropertyName, this.")
                                .Print(ListenerFieldName(m.methodName)).PrintEndLine(") == false)");
                            p.OpenScope();
                            {
                                p.PrintBeginLine("OnBindPropertyFailed(this.")
                                    .Print(PropFieldName(m.methodName)).PrintEndLine(");");
                            }
                            p.CloseScope();
                            p.PrintEndLine();
                        }
                    }
                    p.CloseScope();
                    p.PrintEndLine();
                }

                if (cmdRefs.Count > 0)
                {
                    p.PrintLine("if (this.Context is ETMI.ICommandListener cl)");
                    p.OpenScope();
                    {
                        foreach (var m in cmdRefs)
                        {
                            p.PrintBeginLine("if (cl.TryGetCommand(this.")
                                .Print(CmdFieldName(m.methodName)).Print(".TargetCommandName, out this.")
                                .Print(RelayCommandFieldName(m.methodName)).PrintEndLine(") == false)");
                            p.OpenScope();
                            {
                                p.PrintBeginLine("OnBindCommandFailed(this.")
                                    .Print(CmdFieldName(m.methodName)).PrintEndLine(");");
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

        private static void WriteBinderOnBindFailedPartials(
              ref Printer p
            , bool hasOnBindPropertyFailedMethod
            , bool hasOnBindCommandFailedMethod
            , in EquatableArray<BinderSpec.BindingPropertySpec> propRefs
            , in EquatableArray<BinderSpec.BindingCommandSpec> cmdRefs
        )
        {
            if (propRefs.Count > 0 && !hasOnBindPropertyFailedMethod)
            {
                p.PrintLine("partial void OnBindPropertyFailed(ETMVB.BindingProperty bindingProperty);");
                p.PrintEndLine();
            }

            if (cmdRefs.Count > 0 && !hasOnBindCommandFailedMethod)
            {
                p.PrintLine("partial void OnBindCommandFailed(ETMVB.BindingCommand bindingCommand);");
                p.PrintEndLine();
            }
        }

        private static void WriteBinderStopListening(
              ref Printer p
            , string keyword
            , string isListeningField
            , in EquatableArray<BinderSpec.BindingPropertySpec> propRefs
            , in EquatableArray<BinderSpec.BindingCommandSpec> cmdRefs
        )
        {
            if (propRefs.Count < 1 && cmdRefs.Count < 1)
            {
                return;
            }

            p.PrintLine("/// <inheritdoc/>");
            p.PrintBeginLine("public ").Print(keyword).PrintEndLine("void StopListening()");
            p.OpenScope();
            {
                p.PrintLine("base.StopListening();");
                p.PrintEndLine();

                p.PrintBeginLine("if (this.").Print(isListeningField).PrintEndLine(" == false) return;");
                p.PrintEndLine();
                p.PrintBeginLine("this.").Print(isListeningField).PrintEndLine(" = false;");
                p.PrintEndLine();

                if (propRefs.Count > 0)
                {
                    foreach (var m in propRefs)
                    {
                        p.PrintBeginLine("this.").Print(ListenerFieldName(m.methodName)).PrintEndLine(".Detach();");
                    }

                    p.PrintEndLine();
                }

                if (cmdRefs.Count > 0)
                {
                    foreach (var m in cmdRefs)
                    {
                        p.PrintBeginLine("this.").Print(RelayCommandFieldName(m.methodName)).PrintEndLine(" = null;");
                    }
                }
            }
            p.CloseScope();
            p.PrintEndLine();
        }

        private static void WriteBinderSetTargetPropertyName(
              ref Printer p
            , string keyword
            , in EquatableArray<BinderSpec.BindingPropertySpec> propRefs
        )
        {
            if (propRefs.Count < 1)
            {
                return;
            }

            p.PrintLine("/// <inheritdoc/>");
            p.PrintBeginLine("public ").Print(keyword)
                .PrintEndLine("bool SetTargetPropertyName(string bindingPropertyName, string targetPropertyName)");
            p.OpenScope();
            {
                p.PrintLine("if (base.SetTargetPropertyName(bindingPropertyName, targetPropertyName)) return true;");
                p.PrintEndLine();

                p.PrintLine("switch (bindingPropertyName)");
                p.OpenScope();
                {
                    foreach (var m in propRefs)
                    {
                        p.PrintBeginLine("case ").Print(PropConstName(m.methodName)).PrintEndLine(":");
                        p.OpenScope();
                        {
                            p.PrintBeginLine("this.").Print(PropFieldName(m.methodName))
                                .PrintEndLine(".TargetPropertyName = targetPropertyName;");
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

        private static void WriteBinderSetAdapter(
              ref Printer p
            , string keyword
            , in EquatableArray<BinderSpec.BindingPropertySpec> propRefs
        )
        {
            if (propRefs.Count < 1)
            {
                return;
            }

            p.PrintLine("/// <inheritdoc/>");
            p.PrintBeginLine("public ").Print(keyword)
                .Print("bool SetAdapter(string bindingPropertyName, ")
                .Print(IADAPTER)
                .PrintEndLine(" adapter)");
            p.OpenScope();
            {
                p.PrintLine("if (base.SetAdapter(bindingPropertyName, adapter)) return true;");
                p.PrintEndLine();

                p.PrintLine("switch (bindingPropertyName)");
                p.OpenScope();
                {
                    foreach (var m in propRefs)
                    {
                        if (string.IsNullOrEmpty(m.paramFullTypeName))
                            continue;

                        p.PrintBeginLine("case ").Print(PropConstName(m.methodName)).PrintEndLine(":");
                        p.OpenScope();
                        {
                            p.PrintBeginLine("this.").Print(ConverterFieldName(m.methodName))
                                .PrintEndLine(".Adapter = adapter;");
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

        private static void WriteBinderVariantOverloads(
              ref Printer p
            , in EquatableArray<BinderSpec.BindingPropertySpec> propRefs
        )
        {
            foreach (var m in propRefs)
            {
                if (m.isParameterTypeVariant || string.IsNullOrEmpty(m.paramFullTypeName))
                    continue;

                p.PrintLine(EDITOR_BROWSABLE_NEVER).PrintLine(OBSOLETE_METHOD);
                p.PrintBeginLine("private void ").Print(m.methodName).Print("__Variant")
                    .PrintEndLine("(in global::EncosyTower.Variants.Variant variant)");
                p.OpenScope();
                {
                    p.PrintBeginLine("if (this._variantConverter")
                        .Print(m.variantConverterPropertyName)
                        .Print(".TryGetValue(variant, out ")
                        .Print(m.paramFullTypeName)
                        .PrintEndLine(" value))");
                    p.OpenScope();
                    {
                        p.PrintBeginLine().Print(m.methodName).Print("(");
                        p.PrintIf(m.paramRefKind == RefKind.Ref, "ref ");
                        p.Print("value);").PrintEndLine();
                    }
                    p.CloseScope();
                }
                p.CloseScope();
                p.PrintEndLine();
            }

            p.PrintEndLine();
        }

        private static void WriteBinderPartialCommandMethods(
              ref Printer p
            , in EquatableArray<BinderSpec.BindingCommandSpec> cmdRefs
        )
        {
            foreach (var m in cmdRefs)
            {
                p.PrintLine(AGGRESSIVE_INLINING);

                if (string.IsNullOrEmpty(m.paramFullTypeName))
                {
                    p.PrintBeginLine("partial void ").Print(m.methodName).PrintEndLine("()");
                    p.OpenScope();
                    {
                        p.PrintBeginLine("this.").Print(RelayCommandFieldName(m.methodName))
                            .PrintEndLine("?.Execute();");
                    }
                    p.CloseScope();
                }
                else
                {
                    p.PrintBeginLine().Print("partial void ").Print(m.methodName).Print("(");
                    p.PrintIf(m.paramRefKind == RefKind.Ref, "ref ");
                    p.Print(m.paramFullTypeName).Print(" ").Print(m.paramName).Print(")").PrintEndLine();
                    p.OpenScope();
                    {
                        p.PrintBeginLine("this.").Print(RelayCommandFieldName(m.methodName))
                            .Print("?.Execute(").Print(m.paramName).PrintEndLine(");");
                    }
                    p.CloseScope();
                }

                p.PrintEndLine();
            }

            if (cmdRefs.Count > 0) p.PrintEndLine();
        }

        private static void WriteBinderSetTargetCommandName(
              ref Printer p
            , string keyword
            , in EquatableArray<BinderSpec.BindingCommandSpec> cmdRefs
        )
        {
            if (cmdRefs.Count < 1)
            {
                return;
            }

            p.PrintLine("/// <inheritdoc/>");
            p.PrintBeginLine("public ").Print(keyword)
                .PrintEndLine("bool SetTargetCommandName(string bindingCommandName, string targetCommandName)");
            p.OpenScope();
            {
                p.PrintLine("if (base.SetTargetCommandName(bindingCommandName, targetCommandName)) return true;");
                p.PrintEndLine();

                p.PrintLine("switch (bindingCommandName)");
                p.OpenScope();
                {
                    foreach (var m in cmdRefs)
                    {
                        p.PrintBeginLine("case ").Print(CmdConstName(m.methodName)).PrintEndLine(":");
                        p.OpenScope();
                        {
                            p.PrintBeginLine("this.").Print(CmdFieldName(m.methodName))
                                .PrintEndLine(".TargetCommandName = targetCommandName;");
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

        private static void WriteBinderRefreshContext(
              ref Printer p
            , string keyword
            , in EquatableArray<BinderSpec.BindingPropertySpec> propRefs
        )
        {
            if (propRefs.Count < 1)
            {
                return;
            }

            p.PrintLine("/// <inheritdoc/>");
            p.PrintBeginLine("public ").Print(keyword).PrintEndLine("void RefreshContext()");
            p.OpenScope();
            {
                p.PrintLine("base.RefreshContext();");
                p.PrintEndLine();

                p.PrintLine("if (this.Context is ETMCM.INotifyPropertyChanged inpc)");
                p.OpenScope();
                {
                    foreach (var m in propRefs)
                    {
                        p.PrintBeginLine("if (inpc.NotifyPropertyChanged(this.")
                            .Print(PropFieldName(m.methodName))
                            .Print(".TargetPropertyName, this.")
                            .Print(ListenerFieldName(m.methodName))
                            .PrintEndLine(") == false)");
                        p.OpenScope();
                        {
                            p.PrintBeginLine("OnBindPropertyFailed(this.")
                                .Print(PropFieldName(m.methodName))
                                .PrintEndLine(");");
                        }
                        p.CloseScope();
                        p.PrintEndLine();
                    }
                }
                p.CloseScope();
            }
            p.CloseScope();
            p.PrintEndLine();
        }
    }
}
