using System;
using System.Collections.Generic;

namespace EncosyTower.SourceGen.Generators.Mvvm.ObservableProperties
{
    partial struct ObservablePropertyDeclaration
    {
        private const string AGGRESSIVE_INLINING = "[MethodImpl(MethodImplOptions.AggressiveInlining)]";
        private const string GENERATED_CODE = $"[GeneratedCode(\"EncosyTower.SourceGen.Generators.Mvvm.ObservableProperties.ObservablePropertyGenerator\", \"{SourceGenVersion.VALUE}\")]";
        private const string EXCLUDE_COVERAGE = "[ExcludeFromCodeCoverage]";
        private const string NAMESPACE = "EncosyTower.Mvvm.ComponentModel";
        private const string GENERATED_OBSERVABLE_PROPERTY = "[GeneratedObservableProperty]";
        private const string GENERATED_PROPERTY_CHANGING_HANDLER = "[GeneratedPropertyChangingEventHandler]";
        private const string GENERATED_PROPERTY_CHANGED_HANDLER = "[GeneratedPropertyChangedEventHandler]";
        private const string GENERATED_PROPERTY_NAME_CONSTANT = "[GeneratedPropertyNameConstant]";
        private const string IS_OBSERVABLE_OBJECT = "[IsObservableObject(typeof({0}))]";
        private const string VARIANT = "Variant";
        private const string CACHED_VARIANT_CONVERTER = "CachedVariantConverter";
        private const string INOTIFY_PROPERTY_CHANGING_INTERFACE = "INotifyPropertyChanging";
        private const string INOTIFY_PROPERTY_CHANGED_INTERFACE = "INotifyPropertyChanged";
        private const string IOBSERVABLE_OBJECT_INTERFACE_SHORT = "IObservableObject";
        private const string PROPERTY_CHANGE_EVENT_LISTENER = "PropertyChangeEventListener";
        private const string PROPERTY_CHANGE_EVENT_ARGS = "PropertyChangeEventArgs";
        private const string EDITOR_BROWSABLE_NEVER = "[EditorBrowsable(EditorBrowsableState.Never)]";


        public readonly string WriteCodeWithoutMember()
        {
            var p = Printer.DefaultLarge;

            p.PrintEndLine();
            p.Print("#pragma warning disable").PrintEndLine();
            p.PrintEndLine();

            p = p.IncreasedIndent();

            WriteClassDeclaration(ref p);

            p.OpenScope();
            {
                if (isBaseObservableObject == false)
                {
                    var keyword = isSealed ? "" : "virtual ";

                    p.PrintLine($"/// <inheritdoc cref=\"{INOTIFY_PROPERTY_CHANGING_INTERFACE}.AttachPropertyChangingListener{{TInstance}}(string, {PROPERTY_CHANGE_EVENT_LISTENER}{{TInstance}})\" />");
                    p.PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
                    p.PrintLine($"public {keyword}bool AttachPropertyChangingListener<TInstance>(string propertyName, {PROPERTY_CHANGE_EVENT_LISTENER}<TInstance> listener) where TInstance : class");
                    p.OpenScope();
                    {
                        p.PrintLine("return false;");
                    }
                    p.CloseScope();
                    p.PrintEndLine();

                    p.PrintLine($"/// <inheritdoc cref=\"{INOTIFY_PROPERTY_CHANGED_INTERFACE}.AttachPropertyChangedListener{{TInstance}}(string, {PROPERTY_CHANGE_EVENT_LISTENER}{{TInstance}})\" />");
                    p.PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
                    p.PrintLine($"public {keyword}bool AttachPropertyChangedListener<TInstance>(string propertyName, {PROPERTY_CHANGE_EVENT_LISTENER}<TInstance> listener) where TInstance : class");
                    p.OpenScope();
                    {
                        p.PrintLine("return false;");
                    }
                    p.CloseScope();
                    p.PrintEndLine();

                    p.PrintLine($"/// <inheritdoc cref=\"{INOTIFY_PROPERTY_CHANGED_INTERFACE}.NotifyPropertyChanged{{TInstance}}(string, {PROPERTY_CHANGE_EVENT_LISTENER}{{TInstance}})\" />");
                    p.PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
                    p.PrintLine($"public {keyword}bool NotifyPropertyChanged<TInstance>(string propertyName, {PROPERTY_CHANGE_EVENT_LISTENER}<TInstance> listener) where TInstance : class");
                    p.OpenScope();
                    {
                        p.PrintLine("return false;");
                    }
                    p.CloseScope();
                    p.PrintEndLine();

                    p.PrintLine($"/// <inheritdoc cref=\"{INOTIFY_PROPERTY_CHANGED_INTERFACE}.NotifyPropertyChanged(string)\" />");
                    p.PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
                    p.PrintLine($"public {keyword}bool NotifyPropertyChanged(string propertyName)");
                    p.OpenScope();
                    {
                        p.PrintLine("return false;");
                    }
                    p.CloseScope();
                    p.PrintEndLine();

                    p.PrintLine($"/// <inheritdoc cref=\"{INOTIFY_PROPERTY_CHANGED_INTERFACE}.NotifyPropertyChanged()\" />");
                    p.PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
                    p.PrintLine($"public {keyword}void NotifyPropertyChanged()");
                    p.OpenScope();
                    p.CloseScope();
                    p.PrintEndLine();

                    WriteTryGetMemberObservableObject_Empty(ref p);
                }
            }
            p.CloseScope();

            p = p.DecreasedIndent();
            return p.Result;
        }

        public readonly string WriteCode()
        {
            var p = Printer.DefaultLarge;
            p = p.IncreasedIndent();

            p.PrintEndLine();
            p.Print("#pragma warning disable").PrintEndLine();
            p.PrintEndLine();

            WriteNotifyPropertyChangingInfoAttributes(ref p);
            WriteNotifyPropertyChangedInfoAttributes(ref p);
            WriteClassDeclaration(ref p);

            p.OpenScope();
            {
                WriteConstantFields(ref p);
                WriteEvents(ref p);
                WriteVariantConverters(ref p);
                WriteObservableForProperties(ref p);
                WriteProperties(ref p);
                WritePartialMethods(ref p);
                WriteAttachPropertyChangingListenerMethod(ref p);
                WriteAttachPropertyChangedListenerMethod(ref p);
                WriteNotifyPropertyChangedWithTwoArgumentsMethod(ref p);
                WriteNotifyPropertyChangedWithOneArgumentMethod(ref p);
                WriteNotifyPropertyChangedNoArgumentMethod(ref p);
                WriteTryGetMemberObservableObject(ref p);
            }
            p.CloseScope();

            p = p.DecreasedIndent();
            return p.Result;
        }

        private readonly void WriteClassDeclaration(ref Printer p)
        {
            p.PrintBeginLine("partial class ").Print(className)
                .PrintEndLine($" : {IOBSERVABLE_OBJECT_INTERFACE_SHORT}");
            p = p.IncreasedIndent();
            {
                p.PrintLine($", {INOTIFY_PROPERTY_CHANGING_INTERFACE}");
                p.PrintLine($", {INOTIFY_PROPERTY_CHANGED_INTERFACE}");
            }
            p.PrintEndLine();
            p = p.DecreasedIndent();
        }

        // Builds a lookup of memberKey -> list-of-NotifyForEntry for methods that need it.
        private readonly Dictionary<string, List<NotifyForEntry>> BuildNotifyForMap()
        {
            var map = new Dictionary<string, List<NotifyForEntry>>();

            foreach (var entry in notifyForEntries)
            {
                if (map.TryGetValue(entry.memberKey, out var list) == false)
                {
                    map[entry.memberKey] = list = new List<NotifyForEntry>();
                }

                list.Add(entry);
            }

            return map;
        }

        // Builds a set of unique target property names from the notifyForEntries for methods that
        // need to enumerate all additional properties (e.g. writing events/partial methods/converters).
        private readonly Dictionary<string, NotifyForEntry> BuildUniqueNotifyForTargets()
        {
            var dict = new Dictionary<string, NotifyForEntry>();

            foreach (var entry in notifyForEntries)
            {
                if (dict.ContainsKey(entry.propName) == false)
                {
                    dict[entry.propName] = entry;
                }
            }

            return dict;
        }

        private readonly void WriteConstantFields(ref Printer p)
        {
            var notifyForMap = BuildNotifyForMap();
            var additionalProps = new Dictionary<string, NotifyForEntry>();

            foreach (var member in fieldRefs)
            {
                var typeName = member.fieldTypeName;
                var name = member.propertyName;

                p.PrintLine($"/// <summary>The name of <see cref=\"{name}\"/></summary>");
                p.PrintLine(GENERATED_PROPERTY_NAME_CONSTANT);

                if (member.isObservableObject)
                {
                    p.PrintLine(string.Format(IS_OBSERVABLE_OBJECT, typeName));
                }

                p.PrintLine(GENERATED_CODE);
                p.PrintLine($"public const string {ConstName(name)} = nameof({className}.{name});");
                p.PrintEndLine();

                if (notifyForMap.TryGetValue(member.fieldName, out var entries))
                {
                    foreach (var entry in entries)
                    {
                        if (additionalProps.ContainsKey(entry.propName) == false)
                        {
                            additionalProps[entry.propName] = entry;
                        }
                    }
                }
            }

            foreach (var member in propRefs)
            {
                var typeName = member.propertyTypeName;
                var name = member.propertyName;

                p.PrintLine($"/// <summary>The name of <see cref=\"{name}\"/></summary>");
                p.PrintLine(GENERATED_PROPERTY_NAME_CONSTANT);

                if (member.isObservableObject)
                {
                    p.PrintLine(string.Format(IS_OBSERVABLE_OBJECT, typeName));
                }

                p.PrintLine(GENERATED_CODE);
                p.PrintLine($"public const string {ConstName(name)} = nameof({className}.{name});");
                p.PrintEndLine();

                if (notifyForMap.TryGetValue(member.fieldName, out var entries))
                {
                    foreach (var entry in entries)
                    {
                        if (additionalProps.ContainsKey(entry.propName) == false)
                        {
                            additionalProps[entry.propName] = entry;
                        }
                    }
                }
            }

            foreach (var entry in additionalProps.Values)
            {
                var name = entry.propName;

                p.PrintLine($"/// <summary>The name of <see cref=\"{name}\"/></summary>");
                p.PrintLine(GENERATED_PROPERTY_NAME_CONSTANT);
                p.PrintLine(GENERATED_CODE);
                p.PrintLine($"public const string {ConstName(name)} = nameof({className}.{name});");
                p.PrintEndLine();
            }

            p.PrintEndLine();
        }

        private readonly void WriteEvents(ref Printer p)
        {
            foreach (var member in fieldRefs)
            {
                p.PrintLine(GENERATED_CODE).PrintLine(GENERATED_PROPERTY_CHANGING_HANDLER).PrintLine(EDITOR_BROWSABLE_NEVER);
                p.PrintLine($"private event PropertyChangingEventHandler {OnChangingEventName(member.propertyName)};");
                p.PrintEndLine();

                p.PrintLine(GENERATED_CODE).PrintLine(GENERATED_PROPERTY_CHANGED_HANDLER).PrintLine(EDITOR_BROWSABLE_NEVER);
                p.PrintLine($"private event PropertyChangedEventHandler {OnChangedEventName(member.propertyName)};");
                p.PrintEndLine();
            }

            foreach (var member in propRefs)
            {
                p.PrintLine(GENERATED_CODE).PrintLine(GENERATED_PROPERTY_CHANGING_HANDLER).PrintLine(EDITOR_BROWSABLE_NEVER);
                p.PrintLine($"private event PropertyChangingEventHandler {OnChangingEventName(member.propertyName)};");
                p.PrintEndLine();

                p.PrintLine(GENERATED_CODE).PrintLine(GENERATED_PROPERTY_CHANGED_HANDLER).PrintLine(EDITOR_BROWSABLE_NEVER);
                p.PrintLine($"private event PropertyChangedEventHandler {OnChangedEventName(member.propertyName)};");
                p.PrintEndLine();
            }

            foreach (var entry in BuildUniqueNotifyForTargets().Values)
            {
                p.PrintLine(GENERATED_CODE).PrintLine(GENERATED_PROPERTY_CHANGED_HANDLER);
                p.PrintLine($"private event PropertyChangedEventHandler {OnChangedEventName(entry.propName)};");
                p.PrintEndLine();
            }
        }

        private readonly void WriteVariantConverters(ref Printer p)
        {
            var types = new Dictionary<string, string>();

            foreach (var member in fieldRefs)
            {
                var typeName = member.fieldTypeName;
                var validIdent = member.fieldTypeValidIdent;

                if (types.ContainsKey(typeName) == false)
                {
                    types.Add(typeName, validIdent);
                }

                foreach (var entry in notifyForEntries)
                {
                    if (entry.memberKey != member.fieldName)
                    {
                        continue;
                    }

                    if (types.ContainsKey(entry.propTypeName) == false)
                    {
                        types.Add(entry.propTypeName, entry.propTypeValidIdent);
                    }
                }
            }

            foreach (var member in propRefs)
            {
                var typeName = member.propertyTypeName;
                var validIdent = member.propertyTypeValidIdent;

                if (types.ContainsKey(typeName) == false)
                {
                    types.Add(typeName, validIdent);
                }

                foreach (var entry in notifyForEntries)
                {
                    if (entry.memberKey != member.fieldName)
                    {
                        continue;
                    }

                    if (types.ContainsKey(entry.propTypeName) == false)
                    {
                        types.Add(entry.propTypeName, entry.propTypeValidIdent);
                    }
                }
            }

            foreach (var kv in types)
            {
                var typeName = kv.Key;
                var propertyName = kv.Value.AsSpan().MakeFirstCharUpperCase();

                p.PrintLine(GENERATED_CODE).PrintLine(EDITOR_BROWSABLE_NEVER);
                p.PrintLine($"private readonly {CACHED_VARIANT_CONVERTER}<{typeName}> _variantConverter{propertyName} = {CACHED_VARIANT_CONVERTER}<{typeName}>.Default;");
                p.PrintEndLine();
            }
        }

        private readonly void WriteObservableForProperties(ref Printer p)
        {
            var hasSerializableAttr = hasSerializableAttribute;
            var canGeneratePropertyBagAttr = hasSerializableAttribute && hasGeneratePropertyBagAttribute;
            var notifyForMap = BuildNotifyForMap();

            foreach (var member in propRefs)
            {
                var fieldName = member.fieldName;
                var propertyName = member.propertyName;
                var typeName = member.propertyTypeName;
                var argsName = OnChangedArgsName(propertyName);
                var converterForField = member.propertyTypeValidIdent.AsSpan().MakeFirstCharUpperCase();
                var converterForFieldVariable = $"variantConverter{converterForField}";
                var willNotifyPropertyChanged = notifyForMap.TryGetValue(fieldName, out var notifyEntries);
                var constName = ConstName(propertyName);
                var withSerializeField = false;
                var withDontCreateProperty = false;

                foreach (var ffa in member.forwardedFieldAttributes)
                {
                    if (hasSerializableAttr && ffa.typeName == SERIALIZE_FIELD_ATTRIBUTE)
                    {
                        withSerializeField = true;
                    }
                    else if (canGeneratePropertyBagAttr && ffa.typeName == DONT_CREATE_PROPERTY_ATTRIBUTE)
                    {
                        withDontCreateProperty = true;
                    }

                    p.PrintLine($"[{ffa.attributeInfo.GetSyntax().ToFullString()}]");
                }

                if (member.isObservableObject)
                {
                    p.PrintLine(string.Format(IS_OBSERVABLE_OBJECT, typeName));
                }

                if (hasSerializableAttr && withSerializeField == false)
                {
                    p.PrintLine($"[{SERIALIZE_FIELD_ATTRIBUTE}]");
                }

                if (canGeneratePropertyBagAttr
                    && member.doesCreateProperty
                    && withDontCreateProperty == false
                )
                {
                    p.PrintLine($"[{DONT_CREATE_PROPERTY}]");
                }

                p.PrintLine(GENERATED_CODE).PrintLine(EDITOR_BROWSABLE_NEVER);
                p.PrintLine($"private {typeName} {fieldName};");
                p.PrintEndLine();

                p.PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE).PrintLine(AGGRESSIVE_INLINING);
                p.PrintLine($"private {typeName} Get_{propertyName}()");
                p.OpenScope();
                {
                    p.PrintLine($"return this.{fieldName};");
                }
                p.CloseScope();
                p.PrintEndLine();

                p.PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE).PrintLine(AGGRESSIVE_INLINING);
                p.PrintLine($"private void Set_{propertyName}({typeName} value)");
                p.OpenScope();
                {
                    p.PrintLine($"if (EqualityComparer<{typeName}>.Default.Equals(this.{fieldName}, value)) return;");
                    p.PrintEndLine();

                    p.OpenScope();
                    {
                        p.PrintLine($"var oldValue = this.{fieldName};");

                        p.PrintEndLine();

                        p.PrintLine($"{OnChangingMethodName(propertyName)}(oldValue, value);");
                        p.PrintEndLine();

                        p.PrintLine($"var {converterForFieldVariable} = this._variantConverter{converterForField};");
                        p.PrintLine($"var {argsName} = new {PROPERTY_CHANGE_EVENT_ARGS}(this, {constName}, {converterForFieldVariable}.ToVariant(oldValue), {converterForFieldVariable}.ToVariant(value));");
                        p.PrintLine($"this.{OnChangingEventName(propertyName)}?.Invoke({argsName});");
                        p.PrintEndLine();

                        p.PrintLine($"this.{fieldName} = value;");
                        p.PrintEndLine();

                        p.PrintLine($"{OnChangedMethodName(propertyName)}(oldValue, value);");
                        p.PrintLine($"this.{OnChangedEventName(propertyName)}?.Invoke({argsName});");
                    }
                    p.CloseScope();
                    p.PrintEndLine();

                    if (willNotifyPropertyChanged && notifyEntries != null)
                    {
                        foreach (var entry in notifyEntries)
                        {
                            var oldValueName = "oldValueProperty";
                            var otherArgsName = OnChangedArgsName(entry.propName);
                            var converterForProperty = entry.propTypeValidIdent.AsSpan().MakeFirstCharUpperCase();
                            var converterForPropertyVariable = $"converter{converterForProperty}";

                            p.OpenScope();
                            {
                                p.PrintLine($"var {oldValueName} = this.{entry.propName};");
                                p.PrintEndLine();

                                p.PrintLine($"{OnChangedMethodName(entry.propName)}({oldValueName}, this.{entry.propName});");
                                p.PrintEndLine();

                                p.PrintLine($"var {converterForPropertyVariable} = this._variantConverter{converterForProperty};");
                                p.PrintLine($"var {otherArgsName} = new {PROPERTY_CHANGE_EVENT_ARGS}(this, {ConstName(entry.propName)}, {converterForPropertyVariable}.ToVariant({oldValueName}), {converterForPropertyVariable}.ToVariant(this.{entry.propName}));");
                                p.PrintLine($"this.{OnChangedEventName(entry.propName)}?.Invoke({otherArgsName});");
                            }
                            p.CloseScope();
                            p.PrintEndLine();
                        }
                    }

                    foreach (var commandName in member.commandNames)
                    {
                        if (notifyCanExecuteChangedFor.AsImmutableArray().Contains(commandName))
                        {
                            p.PrintLine($"{commandName}.NotifyCanExecuteChanged();");
                        }
                    }
                }
                p.CloseScope();
                p.PrintEndLine();
            }
        }

        private readonly void WriteProperties(ref Printer p)
        {
            var notifyForMap = BuildNotifyForMap();

            foreach (var member in fieldRefs)
            {
                var fieldName = member.fieldName;
                var propertyName = member.propertyName;
                var typeName = member.fieldTypeName;
                var argsName = OnChangedArgsName(propertyName);
                var converterForField = member.fieldTypeValidIdent.AsSpan().MakeFirstCharUpperCase();
                var converterForFieldVariable = $"variantConverter{converterForField}";
                var willNotifyPropertyChanged = notifyForMap.TryGetValue(fieldName, out var notifyEntries);
                var constName = ConstName(propertyName);

                p.PrintLine($"/// <inheritdoc cref=\"{fieldName}\"/>");

                foreach (var attribute in member.forwardedPropertyAttributes)
                {
                    p.PrintLine($"[{attribute.GetSyntax().ToFullString()}]");
                }

                if (member.isObservableObject)
                {
                    p.PrintLine(string.Format(IS_OBSERVABLE_OBJECT, typeName));
                }

                p.PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE).PrintLine(GENERATED_OBSERVABLE_PROPERTY);
                p.PrintLine($"public {typeName} {propertyName}");
                p.OpenScope();
                {
                    p.PrintLine(AGGRESSIVE_INLINING);
                    p.PrintLine($"get => this.{fieldName};");
                    p.PrintLine("set");
                    p.OpenScope();
                    {
                        p.PrintLine($"if (EqualityComparer<{typeName}>.Default.Equals(this.{fieldName}, value)) return;");
                        p.PrintEndLine();

                        p.OpenScope();
                        {
                            p.PrintLine($"var oldValue = this.{fieldName};");

                            p.PrintEndLine();

                            p.PrintLine($"{OnChangingMethodName(propertyName)}(oldValue, value);");
                            p.PrintEndLine();

                            p.PrintLine($"var {converterForFieldVariable} = this._variantConverter{converterForField};");
                            p.PrintLine($"var {argsName} = new {PROPERTY_CHANGE_EVENT_ARGS}(this, {constName}, {converterForFieldVariable}.ToVariant(oldValue), {converterForFieldVariable}.ToVariant(value));");
                            p.PrintLine($"this.{OnChangingEventName(propertyName)}?.Invoke({argsName});");
                            p.PrintEndLine();

                            p.PrintLine($"this.{fieldName} = value;");
                            p.PrintEndLine();

                            p.PrintLine($"{OnChangedMethodName(propertyName)}(oldValue, value);");
                            p.PrintLine($"this.{OnChangedEventName(propertyName)}?.Invoke({argsName});");
                        }
                        p.CloseScope();
                        p.PrintEndLine();

                        if (willNotifyPropertyChanged && notifyEntries != null)
                        {
                            foreach (var entry in notifyEntries)
                            {
                                var oldValueName = "oldValueProperty";
                                var otherArgsName = OnChangedArgsName(entry.propName);
                                var converterForProperty = entry.propTypeValidIdent.AsSpan().MakeFirstCharUpperCase();
                                var converterForPropertyVariable = $"converter{converterForProperty}";

                                p.OpenScope();
                                {
                                    p.PrintLine($"var {oldValueName} = this.{entry.propName};");
                                    p.PrintEndLine();

                                    p.PrintLine($"{OnChangedMethodName(entry.propName)}({oldValueName}, this.{entry.propName});");
                                    p.PrintEndLine();

                                    p.PrintLine($"var {converterForPropertyVariable} = this._variantConverter{converterForProperty};");
                                    p.PrintLine($"var {otherArgsName} = new {PROPERTY_CHANGE_EVENT_ARGS}(this, {ConstName(entry.propName)}, {converterForPropertyVariable}.ToVariant({oldValueName}), {converterForPropertyVariable}.ToVariant(this.{entry.propName}));");
                                    p.PrintLine($"this.{OnChangedEventName(entry.propName)}?.Invoke({otherArgsName});");
                                }
                                p.CloseScope();
                                p.PrintEndLine();
                            }
                        }

                        foreach (var commandName in member.commandNames)
                        {
                            if (notifyCanExecuteChangedFor.AsImmutableArray().Contains(commandName))
                            {
                                p.PrintLine($"{commandName}.NotifyCanExecuteChanged();");
                            }
                        }
                    }
                    p.CloseScope();
                }
                p.CloseScope();
                p.PrintEndLine();
            }
        }

        private readonly void WritePartialMethods(ref Printer p)
        {
            foreach (var member in fieldRefs)
            {
                var typeName = member.fieldTypeName;
                var propName = member.propertyName;

                p.PrintLine($"/// <summary>Executes the logic for when <see cref=\"{propName}\"/> is changing.</summary>");
                p.PrintLine("/// <param name=\"oldValue\">The previous property value that is being replaced.</param>");
                p.PrintLine("/// <param name=\"newValue\">The new property value being set.</param>");
                p.PrintLine($"/// <remarks>This method is invoked right before the value of <see cref=\"{propName}\"/> is changed.</remarks>");
                p.PrintLine(GENERATED_CODE);
                p.PrintLine($"partial void {OnChangingMethodName(propName)}({typeName} oldValue, {typeName} newValue);");
                p.PrintEndLine();

                p.PrintLine($"/// <summary>Executes the logic for when <see cref=\"{propName}\"/> just changed.</summary>");
                p.PrintLine("/// <param name=\"oldValue\">The previous property value that was replaced.</param>");
                p.PrintLine("/// <param name=\"newValue\">The new property value that was set.</param>");
                p.PrintLine($"/// <remarks>This method is invoked right after the value of <see cref=\"{propName}\"/> is changed.</remarks>");
                p.PrintLine(GENERATED_CODE);
                p.PrintLine($"partial void {OnChangedMethodName(propName)}({typeName} oldValue, {typeName} newValue);");
                p.PrintEndLine();
            }

            foreach (var member in propRefs)
            {
                var typeName = member.propertyTypeName;
                var propName = member.propertyName;

                p.PrintLine($"/// <summary>Executes the logic for when <see cref=\"{propName}\"/> is changing.</summary>");
                p.PrintLine("/// <param name=\"oldValue\">The previous property value that is being replaced.</param>");
                p.PrintLine("/// <param name=\"newValue\">The new property value being set.</param>");
                p.PrintLine($"/// <remarks>This method is invoked right before the value of <see cref=\"{propName}\"/> is changed.</remarks>");
                p.PrintLine(GENERATED_CODE);
                p.PrintLine($"partial void {OnChangingMethodName(propName)}({typeName} oldValue, {typeName} newValue);");
                p.PrintEndLine();

                p.PrintLine($"/// <summary>Executes the logic for when <see cref=\"{propName}\"/> just changed.</summary>");
                p.PrintLine("/// <param name=\"oldValue\">The previous property value that was replaced.</param>");
                p.PrintLine("/// <param name=\"newValue\">The new property value that was set.</param>");
                p.PrintLine($"/// <remarks>This method is invoked right after the value of <see cref=\"{propName}\"/> is changed.</remarks>");
                p.PrintLine(GENERATED_CODE);
                p.PrintLine($"partial void {OnChangedMethodName(propName)}({typeName} oldValue, {typeName} newValue);");
                p.PrintEndLine();
            }

            foreach (var entry in BuildUniqueNotifyForTargets().Values)
            {
                var propName = entry.propName;
                var typeName = entry.propTypeName;

                p.PrintLine($"/// <summary>Executes the logic for when <see cref=\"{propName}\"/> just changed.</summary>");
                p.PrintLine("/// <param name=\"value\">The new property value that was set.</param>");
                p.PrintLine($"/// <remarks>This method is invoked right after the value of <see cref=\"{propName}\"/> is changed.</remarks>");
                p.PrintLine(GENERATED_CODE);
                p.PrintLine($"partial void {OnChangedMethodName(propName)}({typeName} oldValue, {typeName} newValue);");
                p.PrintEndLine();
            }
        }

        private readonly void WriteAttachPropertyChangingListenerMethod(ref Printer p)
        {
            var keyword = isBaseObservableObject ? "override " : isSealed ? "" : "virtual ";

            p.PrintLine($"/// <inheritdoc cref=\"{INOTIFY_PROPERTY_CHANGING_INTERFACE}.AttachPropertyChangingListener{{TInstance}}(string, {PROPERTY_CHANGE_EVENT_LISTENER}{{TInstance}})\" />");
            p.PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
            p.PrintLine($"public {keyword}bool AttachPropertyChangingListener<TInstance>(string propertyName, {PROPERTY_CHANGE_EVENT_LISTENER}<TInstance> listener) where TInstance : class");
            p.OpenScope();
            {
                if (isBaseObservableObject)
                {
                    p.PrintLine("if (base.AttachPropertyChangingListener<TInstance>(propertyName, listener)) return true;");
                }
                else
                {
                    p.PrintLine("if (listener == null) throw new ArgumentNullException(nameof(listener));");
                }

                p.PrintEndLine();

                p.PrintLine("switch (propertyName)");
                p.OpenScope();
                {
                    foreach (var member in fieldRefs)
                    {
                        var eventName = OnChangingEventName(member.propertyName);
                        var constName = ConstName(member.propertyName);

                        p.PrintLine($"case {constName}:");
                        p.OpenScope();
                        {
                            p.PrintLine($"this.{eventName} += listener.OnEvent;");
                            p.PrintLine($"listener.OnDetachAction = (listener) => this.{eventName} -= listener.OnEvent;");
                            p.PrintLine("return true;");
                        }
                        p.CloseScope();
                        p.PrintEndLine();
                    }

                    foreach (var member in propRefs)
                    {
                        var eventName = OnChangingEventName(member.propertyName);
                        var constName = ConstName(member.propertyName);

                        p.PrintLine($"case {constName}:");
                        p.OpenScope();
                        {
                            p.PrintLine($"this.{eventName} += listener.OnEvent;");
                            p.PrintLine($"listener.OnDetachAction = (listener) => this.{eventName} -= listener.OnEvent;");
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

        private readonly void WriteAttachPropertyChangedListenerMethod(ref Printer p)
        {
            var keyword = isBaseObservableObject ? "override " : isSealed ? "" : "virtual ";

            p.PrintLine($"/// <inheritdoc cref=\"{INOTIFY_PROPERTY_CHANGED_INTERFACE}.AttachPropertyChangedListener{{TInstance}}(string, {PROPERTY_CHANGE_EVENT_LISTENER}{{TInstance}})\" />");
            p.PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
            p.PrintLine($"public {keyword}bool AttachPropertyChangedListener<TInstance>(string propertyName, {PROPERTY_CHANGE_EVENT_LISTENER}<TInstance> listener) where TInstance : class");
            p.OpenScope();
            {
                if (isBaseObservableObject)
                {
                    p.PrintLine("if (base.AttachPropertyChangedListener<TInstance>(propertyName, listener)) return true;");
                }
                else
                {
                    p.PrintLine("if (listener == null) throw new ArgumentNullException(nameof(listener));");
                }

                p.PrintEndLine();

                p.PrintLine("switch (propertyName)");
                p.OpenScope();
                {
                    foreach (var member in fieldRefs)
                    {
                        var eventName = OnChangedEventName(member.propertyName);
                        var constName = ConstName(member.propertyName);

                        p.PrintLine($"case {constName}:");
                        p.OpenScope();
                        {
                            p.PrintLine($"this.{eventName} += listener.OnEvent;");
                            p.PrintLine($"listener.OnDetachAction = (listener) => this.{eventName} -= listener.OnEvent;");
                            p.PrintLine("return true;");
                        }
                        p.CloseScope();
                        p.PrintEndLine();
                    }

                    foreach (var member in propRefs)
                    {
                        var eventName = OnChangedEventName(member.propertyName);
                        var constName = ConstName(member.propertyName);

                        p.PrintLine($"case {constName}:");
                        p.OpenScope();
                        {
                            p.PrintLine($"this.{eventName} += listener.OnEvent;");
                            p.PrintLine($"listener.OnDetachAction = (listener) => this.{eventName} -= listener.OnEvent;");
                            p.PrintLine("return true;");
                        }
                        p.CloseScope();
                        p.PrintEndLine();
                    }

                    foreach (var entry in BuildUniqueNotifyForTargets().Values)
                    {
                        var constName = ConstName(entry.propName);
                        var eventName = OnChangedEventName(entry.propName);

                        p.PrintLine($"case {constName}:");
                        p.OpenScope();
                        {
                            p.PrintLine($"this.{eventName} += listener.OnEvent;");
                            p.PrintLine($"listener.OnDetachAction = (listener) => this.{eventName} -= listener.OnEvent;");
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

        private readonly void WriteNotifyPropertyChangedWithTwoArgumentsMethod(ref Printer p)
        {
            var keyword = isBaseObservableObject ? "override " : isSealed ? "" : "virtual ";

            p.PrintLine($"/// <inheritdoc cref=\"{INOTIFY_PROPERTY_CHANGED_INTERFACE}.NotifyPropertyChanged{{TInstance}}(string, {PROPERTY_CHANGE_EVENT_LISTENER}{{TInstance}})\" />");
            p.PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
            p.PrintLine($"public {keyword}bool NotifyPropertyChanged<TInstance>(string propertyName, {PROPERTY_CHANGE_EVENT_LISTENER}<TInstance> listener) where TInstance : class");
            p.OpenScope();
            {
                if (isBaseObservableObject)
                {
                    p.PrintLine("if (base.NotifyPropertyChanged<TInstance>(propertyName, listener)) return true;");
                }
                else
                {
                    p.PrintLine("if (listener == null) throw new ArgumentNullException(nameof(listener));");
                }

                p.PrintEndLine();

                p.PrintLine("switch (propertyName)");
                p.OpenScope();
                {
                    foreach (var member in fieldRefs)
                    {
                        var fieldName = member.fieldName;
                        var argsName = OnChangedArgsName(member.propertyName);
                        var converterForField = member.fieldTypeValidIdent.AsSpan().MakeFirstCharUpperCase();
                        var converterForFieldVariable = $"variantConverter{converterForField}";
                        var constName = ConstName(member.propertyName);

                        p.PrintLine($"case {constName}:");
                        p.OpenScope();
                        {
                            p.PrintLine($"var {converterForFieldVariable} = this._variantConverter{converterForField};");
                            p.PrintLine($"var variant = {converterForFieldVariable}.ToVariant(this.{fieldName});");
                            p.PrintLine($"var {argsName} = new {PROPERTY_CHANGE_EVENT_ARGS}(this, {constName}, variant, variant);");
                            p.PrintLine($"listener.OnEvent({argsName});");
                            p.PrintLine("return true;");
                        }
                        p.CloseScope();
                        p.PrintEndLine();
                    }

                    foreach (var member in propRefs)
                    {
                        var fieldName = member.fieldName;
                        var argsName = OnChangedArgsName(member.propertyName);
                        var converterForField = member.propertyTypeValidIdent.AsSpan().MakeFirstCharUpperCase();
                        var converterForFieldVariable = $"variantConverter{converterForField}";
                        var constName = ConstName(member.propertyName);

                        p.PrintLine($"case {constName}:");
                        p.OpenScope();
                        {
                            p.PrintLine($"var {converterForFieldVariable} = this._variantConverter{converterForField};");
                            p.PrintLine($"var variant = {converterForFieldVariable}.ToVariant(this.{fieldName});");
                            p.PrintLine($"var {argsName} = new {PROPERTY_CHANGE_EVENT_ARGS}(this, {constName}, variant, variant);");
                            p.PrintLine($"listener.OnEvent({argsName});");
                            p.PrintLine("return true;");
                        }
                        p.CloseScope();
                        p.PrintEndLine();
                    }

                    foreach (var entry in BuildUniqueNotifyForTargets().Values)
                    {
                        var constName = ConstName(entry.propName);
                        var otherArgsName = OnChangedArgsName(entry.propName);
                        var converterForProperty = entry.propTypeValidIdent.AsSpan().MakeFirstCharUpperCase();
                        var converterForPropertyVariable = $"converter{converterForProperty}";

                        p.PrintLine($"case {constName}:");
                        p.OpenScope();
                        {
                            p.PrintLine($"var {converterForPropertyVariable} = this._variantConverter{converterForProperty};");
                            p.PrintLine($"var variant = {converterForPropertyVariable}.ToVariant(this.{entry.propName});");
                            p.PrintLine($"var {otherArgsName} = new {PROPERTY_CHANGE_EVENT_ARGS}(this, {constName}, variant, variant);");
                            p.PrintLine($"listener.OnEvent({otherArgsName});");
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

        private readonly void WriteNotifyPropertyChangedWithOneArgumentMethod(ref Printer p)
        {
            var keyword = isBaseObservableObject ? "override " : isSealed ? "" : "virtual ";

            p.PrintLine($"/// <inheritdoc cref=\"{INOTIFY_PROPERTY_CHANGED_INTERFACE}.NotifyPropertyChanged(string)\" />");
            p.PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
            p.PrintLine($"public {keyword}bool NotifyPropertyChanged(string propertyName)");
            p.OpenScope();
            {
                if (isBaseObservableObject)
                {
                    p.PrintLine("if (base.NotifyPropertyChanged(propertyName)) return true;");
                }

                p.PrintEndLine();

                p.PrintLine("switch (propertyName)");
                p.OpenScope();
                {
                    foreach (var member in fieldRefs)
                    {
                        var fieldName = member.fieldName;
                        var argsName = OnChangedArgsName(member.propertyName);
                        var converterForField = member.fieldTypeValidIdent.AsSpan().MakeFirstCharUpperCase();
                        var converterForFieldVariable = $"variantConverter{converterForField}";
                        var constName = ConstName(member.propertyName);

                        p.PrintLine($"case {constName}:");
                        p.OpenScope();
                        {
                            p.PrintLine($"var {converterForFieldVariable} = this._variantConverter{converterForField};");
                            p.PrintLine($"var variant = {converterForFieldVariable}.ToVariant(this.{fieldName});");
                            p.PrintLine($"var {argsName} = new {PROPERTY_CHANGE_EVENT_ARGS}(this, {constName}, variant, variant);");
                            p.PrintLine($"this.{OnChangedEventName(member.propertyName)}?.Invoke({argsName});");
                            p.PrintLine("return true;");
                        }
                        p.CloseScope();
                        p.PrintEndLine();
                    }

                    foreach (var member in propRefs)
                    {
                        var fieldName = member.fieldName;
                        var argsName = OnChangedArgsName(member.propertyName);
                        var converterForField = member.propertyTypeValidIdent.AsSpan().MakeFirstCharUpperCase();
                        var converterForFieldVariable = $"variantConverter{converterForField}";
                        var constName = ConstName(member.propertyName);

                        p.PrintLine($"case {constName}:");
                        p.OpenScope();
                        {
                            p.PrintLine($"var {converterForFieldVariable} = this._variantConverter{converterForField};");
                            p.PrintLine($"var variant = {converterForFieldVariable}.ToVariant(this.{fieldName});");
                            p.PrintLine($"var {argsName} = new {PROPERTY_CHANGE_EVENT_ARGS}(this, {constName}, variant, variant);");
                            p.PrintLine($"this.{OnChangedEventName(member.propertyName)}?.Invoke({argsName});");
                            p.PrintLine("return true;");
                        }
                        p.CloseScope();
                        p.PrintEndLine();
                    }

                    foreach (var entry in BuildUniqueNotifyForTargets().Values)
                    {
                        var constName = ConstName(entry.propName);
                        var otherArgsName = OnChangedArgsName(entry.propName);
                        var converterForProperty = entry.propTypeValidIdent.AsSpan().MakeFirstCharUpperCase();
                        var converterForPropertyVariable = $"converter{converterForProperty}";

                        p.PrintLine($"case {constName}:");
                        p.OpenScope();
                        {
                            p.PrintLine($"var {converterForPropertyVariable} = this._variantConverter{converterForProperty};");
                            p.PrintLine($"var variant = {converterForPropertyVariable}.ToVariant(this.{entry.propName});");
                            p.PrintLine($"var {otherArgsName} = new {PROPERTY_CHANGE_EVENT_ARGS}(this, {constName}, variant, variant);");
                            p.PrintLine($"this.{OnChangedEventName(entry.propName)}?.Invoke({otherArgsName});");
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

        private readonly void WriteNotifyPropertyChangedNoArgumentMethod(ref Printer p)
        {
            var keyword = isBaseObservableObject ? "override " : isSealed ? "" : "virtual ";

            p.PrintLine($"/// <inheritdoc cref=\"{INOTIFY_PROPERTY_CHANGED_INTERFACE}.NotifyPropertyChanged()\" />");
            p.PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
            p.PrintLine($"public {keyword}void NotifyPropertyChanged()");
            p.OpenScope();
            {
                if (isBaseObservableObject)
                {
                    p.PrintLine("base.NotifyPropertyChanged();");
                }

                p.PrintEndLine();

                foreach (var member in fieldRefs)
                {
                    var fieldName = member.fieldName;
                    var argsName = OnChangedArgsName(member.propertyName);
                    var converterForField = member.fieldTypeValidIdent.AsSpan().MakeFirstCharUpperCase();
                    var converterForFieldVariable = $"variantConverter{converterForField}";
                    var constName = ConstName(member.propertyName);

                    p.OpenScope();
                    {
                        p.PrintLine($"var {converterForFieldVariable} = this._variantConverter{converterForField};");
                        p.PrintLine($"var variant = {converterForFieldVariable}.ToVariant(this.{fieldName});");
                        p.PrintLine($"var {argsName} = new {PROPERTY_CHANGE_EVENT_ARGS}(this, {constName}, variant, variant);");
                        p.PrintLine($"this.{OnChangedEventName(member.propertyName)}?.Invoke({argsName});");
                    }
                    p.CloseScope();
                    p.PrintEndLine();
                }

                foreach (var member in propRefs)
                {
                    var fieldName = member.fieldName;
                    var argsName = OnChangedArgsName(member.propertyName);
                    var converterForField = member.propertyTypeValidIdent.AsSpan().MakeFirstCharUpperCase();
                    var converterForFieldVariable = $"variantConverter{converterForField}";
                    var constName = ConstName(member.propertyName);

                    p.OpenScope();
                    {
                        p.PrintLine($"var {converterForFieldVariable} = this._variantConverter{converterForField};");
                        p.PrintLine($"var variant = {converterForFieldVariable}.ToVariant(this.{fieldName});");
                        p.PrintLine($"var {argsName} = new {PROPERTY_CHANGE_EVENT_ARGS}(this, {constName}, variant, variant);");
                        p.PrintLine($"this.{OnChangedEventName(member.propertyName)}?.Invoke({argsName});");
                    }
                    p.CloseScope();
                    p.PrintEndLine();
                }

                foreach (var entry in BuildUniqueNotifyForTargets().Values)
                {
                    var constName = ConstName(entry.propName);
                    var otherArgsName = OnChangedArgsName(entry.propName);
                    var converterForProperty = entry.propTypeValidIdent.AsSpan().MakeFirstCharUpperCase();
                    var converterForPropertyVariable = $"converter{converterForProperty}";

                    p.OpenScope();
                    {
                        p.PrintLine($"var {converterForPropertyVariable} = this._variantConverter{converterForProperty};");
                        p.PrintLine($"var variant = {converterForPropertyVariable}.ToVariant(this.{entry.propName});");
                        p.PrintLine($"var {otherArgsName} = new {PROPERTY_CHANGE_EVENT_ARGS}(this, {constName}, variant, variant);");
                        p.PrintLine($"this.{OnChangedEventName(entry.propName)}?.Invoke({otherArgsName});");
                    }
                    p.CloseScope();
                    p.PrintEndLine();
                }
            }
            p.CloseScope();
            p.PrintEndLine();
        }

        private readonly void WriteNotifyPropertyChangingInfoAttributes(ref Printer p)
        {
            const string ATTRIBUTE = "[NotifyPropertyChangingInfo(\"{0}\", typeof({1}))]";

            foreach (var member in fieldRefs)
            {
                p.PrintLine(string.Format(ATTRIBUTE, member.propertyName, member.fieldTypeName));
            }

            foreach (var member in propRefs)
            {
                p.PrintLine(string.Format(ATTRIBUTE, member.propertyName, member.propertyTypeName));
            }
        }

        private readonly void WriteNotifyPropertyChangedInfoAttributes(ref Printer p)
        {
            const string ATTRIBUTE = "[NotifyPropertyChangedInfo(\"{0}\", typeof({1}))]";

            var additionalProps = new Dictionary<string, NotifyForEntry>();
            var notifyForMap = BuildNotifyForMap();

            foreach (var member in fieldRefs)
            {
                p.PrintLine(string.Format(ATTRIBUTE, member.propertyName, member.fieldTypeName));

                if (notifyForMap.TryGetValue(member.fieldName, out var entries))
                {
                    foreach (var entry in entries)
                    {
                        if (additionalProps.ContainsKey(entry.propName) == false)
                        {
                            additionalProps[entry.propName] = entry;
                        }
                    }
                }
            }

            foreach (var member in propRefs)
            {
                p.PrintLine(string.Format(ATTRIBUTE, member.propertyName, member.propertyTypeName));

                if (notifyForMap.TryGetValue(member.fieldName, out var entries))
                {
                    foreach (var entry in entries)
                    {
                        if (additionalProps.ContainsKey(entry.propName) == false)
                        {
                            additionalProps[entry.propName] = entry;
                        }
                    }
                }
            }

            foreach (var entry in additionalProps.Values)
            {
                p.PrintLine(string.Format(ATTRIBUTE, entry.propName, entry.propTypeName));
            }
        }

        private static void WriteTryGetMemberObservableObject_Empty(ref Printer p)
        {
            p.PrintLine($"/// <inheritdoc cref=\"{IOBSERVABLE_OBJECT_INTERFACE_SHORT}.TryGetMemberObservableObject(Queue{{string}}, out {IOBSERVABLE_OBJECT_INTERFACE_SHORT})\"/>");
            p.PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
            p.PrintLine($"public bool TryGetMemberObservableObject(Queue<string> propertyNames, out {IOBSERVABLE_OBJECT_INTERFACE_SHORT} result)");
            p.OpenScope();
            {
                p.PrintLine("result = default;");
                p.PrintLine("return false;");
            }
            p.CloseScope();
            p.PrintEndLine();
        }

        private readonly void WriteTryGetMemberObservableObject(ref Printer p)
        {
            if (hasMemberObservableObject == false)
            {
                WriteTryGetMemberObservableObject_Empty(ref p);
                return;
            }

            p.PrintLine($"/// <inheritdoc cref=\"{IOBSERVABLE_OBJECT_INTERFACE_SHORT}.TryGetMemberObservableObject(Queue{{string}}, out {IOBSERVABLE_OBJECT_INTERFACE_SHORT})\"/>");
            p.PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
            p.PrintLine($"public bool TryGetMemberObservableObject(Queue<string> propertyNames, out {IOBSERVABLE_OBJECT_INTERFACE_SHORT} result)");
            p.OpenScope();
            {
                p.PrintLine("if (propertyNames.Count > 0)");
                p.OpenScope();
                {
                    p.PrintLine("var propertyName = propertyNames.Dequeue();");
                    p.PrintEndLine();

                    p.PrintLine("switch (propertyName)");
                    p.OpenScope();
                    {
                        foreach (var member in fieldRefs)
                        {
                            if (member.isObservableObject == false)
                            {
                                continue;
                            }

                            var fieldName = member.fieldName;
                            var typeName = member.fieldTypeName;
                            var constName = ConstName(member.propertyName);

                            p.PrintLine($"case {constName}:");
                            p.OpenScope();
                            {
                                p.PrintLine($"if (this.{fieldName} is not {typeName} candidate)");
                                p.OpenScope();
                                {
                                    p.PrintLine("goto INVALID;");
                                }
                                p.CloseScope();
                                p.PrintEndLine();

                                p.PrintLine("if (propertyNames.Count > 0)");
                                p.OpenScope();
                                {
                                    p.PrintLine("if (candidate.TryGetMemberObservableObject(propertyNames, out result))");
                                    p.OpenScope();
                                    {
                                        p.PrintLine("return true;");
                                    }
                                    p.CloseScope();
                                    p.PrintEndLine();

                                    p.PrintLine("goto INVALID;");
                                }
                                p.CloseScope();
                                p.PrintLine("else");
                                p.OpenScope();
                                {
                                    p.PrintLine("result = candidate;");
                                    p.PrintLine("return true;");
                                }
                                p.CloseScope();
                            }
                            p.CloseScope();
                        }

                        foreach (var member in propRefs)
                        {
                            if (member.isObservableObject == false)
                            {
                                continue;
                            }

                            var propName = member.propertyName;
                            var typeName = member.propertyTypeName;
                            var constName = ConstName(propName);

                            p.PrintLine($"case {constName}:");
                            p.OpenScope();
                            {
                                p.PrintLine($"if (this.{propName} is not {typeName} candidate)");
                                p.OpenScope();
                                {
                                    p.PrintLine("goto INVALID;");
                                }
                                p.CloseScope();
                                p.PrintEndLine();

                                p.PrintLine("if (propertyNames.Count > 0)");
                                p.OpenScope();
                                {
                                    p.PrintLine("if (candidate.TryGetMemberObservableObject(propertyNames, out result))");
                                    p.OpenScope();
                                    {
                                        p.PrintLine("return true;");
                                    }
                                    p.CloseScope();
                                    p.PrintEndLine();

                                    p.PrintLine("goto INVALID;");
                                }
                                p.CloseScope();
                                p.PrintLine("else");
                                p.OpenScope();
                                {
                                    p.PrintLine("result = candidate;");
                                    p.PrintLine("return true;");
                                }
                                p.CloseScope();
                            }
                            p.CloseScope();
                        }
                    }
                    p.CloseScope();
                }
                p.CloseScope();
                p.PrintEndLine();

                p.PrintLine("INVALID:");
                p.PrintLine("result = default;");
                p.PrintLine("return false;");
            }
            p.CloseScope();
            p.PrintEndLine();
        }

        private static string ConstName(string propertyName)
            => $"PropertyName_{propertyName}";

        private static string OnChangingEventName(string propertyName)
            => $"_onChanging{propertyName}";

        private static string OnChangedEventName(string name)
            => $"_onChanged{name}";

        private static string OnChangedArgsName(string name)
            => $"args{name}";

        private static string OnChangingMethodName(string propertyName)
            => $"On{propertyName}Changing";

        private static string OnChangedMethodName(string propertyName)
            => $"On{propertyName}Changed";
    }
}

