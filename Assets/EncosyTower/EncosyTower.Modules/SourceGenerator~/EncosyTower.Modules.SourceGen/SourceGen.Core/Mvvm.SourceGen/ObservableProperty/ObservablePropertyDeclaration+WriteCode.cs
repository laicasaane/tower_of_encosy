using System;
using System.Collections.Generic;
using EncosyTower.Modules.SourceGen;
using Microsoft.CodeAnalysis;

namespace EncosyTower.Modules.Mvvm.ObservablePropertySourceGen
{
    partial class ObservablePropertyDeclaration
    {
        private const string AGGRESSIVE_INLINING = "[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]";
        private const string GENERATED_CODE = "[global::System.CodeDom.Compiler.GeneratedCode(\"EncosyTower.Modules.Mvvm.ObservablePropertyGenerator\", \"1.2.0\")]";
        private const string EXCLUDE_COVERAGE = "[global::System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]";
        private const string GENERATED_OBSERVABLE_PROPERTY = "[global::EncosyTower.Modules.Mvvm.ComponentModel.SourceGen.GeneratedObservableProperty]";
        private const string GENERATED_PROPERTY_CHANGING_HANDLER = "[global::EncosyTower.Modules.Mvvm.ComponentModel.SourceGen.GeneratedPropertyChangingEventHandler]";
        private const string GENERATED_PROPERTY_CHANGED_HANDLER = "[global::EncosyTower.Modules.Mvvm.ComponentModel.SourceGen.GeneratedPropertyChangedEventHandler]";
        private const string GENERATED_PROPERTY_NAME_CONSTANT = "[global::EncosyTower.Modules.Mvvm.ComponentModel.SourceGen.GeneratedPropertyNameConstant]";
        private const string IS_OBSERVABLE_OBJECT = "[global::EncosyTower.Modules.Mvvm.ComponentModel.SourceGen.IsObservableObject(typeof({0}))]";
        private const string UNION = "global::EncosyTower.Modules.Unions.Union";
        private const string CACHED_UNION_CONVERTER = "global::EncosyTower.Modules.Unions.Converters.CachedUnionConverter";
        private const string INOTIFY_PROPERTY_CHANGING_INTERFACE = "global::EncosyTower.Modules.Mvvm.ComponentModel.INotifyPropertyChanging";
        private const string INOTIFY_PROPERTY_CHANGED_INTERFACE = "global::EncosyTower.Modules.Mvvm.ComponentModel.INotifyPropertyChanged";
        private const string PROPERTY_CHANGE_EVENT_LISTENER = "global::EncosyTower.Modules.Mvvm.ComponentModel.PropertyChangeEventListener";
        private const string PROPERTY_CHANGE_EVENT_ARGS = "global::EncosyTower.Modules.Mvvm.ComponentModel.PropertyChangeEventArgs";

        public string WriteCodeWithoutMember()
        {
            var scopePrinter = new SyntaxNodeScopePrinter(Printer.DefaultLarge, Syntax.Parent);
            var p = scopePrinter.printer;

            p.PrintEndLine();
            p.Print("#pragma warning disable").PrintEndLine();
            p.PrintEndLine();

            p = p.IncreasedIndent();

            WriteClassDeclaration(ref p);

            p.OpenScope();
            {
                if (IsBaseObservableObject == false)
                {
                    var keyword = Symbol.IsSealed ? "" : "virtual ";

                    p.PrintLine($"/// <inheritdoc cref=\"{INOTIFY_PROPERTY_CHANGING_INTERFACE}.AttachPropertyChangingListener{{TInstance}}(string, {PROPERTY_CHANGE_EVENT_LISTENER}{{TInstance}})\" />");
                    p.PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
                    p.PrintLine($"public {keyword}bool AttachPropertyChangingListener<TInstance>(string propertyName, {PROPERTY_CHANGE_EVENT_LISTENER}<TInstance> listener) where TInstance : class");
                    p.OpenScope();
                    {
                        if (IsBaseObservableObject)
                        {
                            p.PrintLine("return base.AttachPropertyChangingListener<TInstance>(propertyName, listener);");
                        }
                        else
                        {
                            p.PrintLine("return false;");
                        }
                    }
                    p.CloseScope();
                    p.PrintEndLine();

                    p.PrintLine($"/// <inheritdoc cref=\"{INOTIFY_PROPERTY_CHANGED_INTERFACE}.AttachPropertyChangedListener{{TInstance}}(string, {PROPERTY_CHANGE_EVENT_LISTENER}{{TInstance}})\" />");
                    p.PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
                    p.PrintLine($"public {keyword}bool AttachPropertyChangedListener<TInstance>(string propertyName, {PROPERTY_CHANGE_EVENT_LISTENER}<TInstance> listener) where TInstance : class");
                    p.OpenScope();
                    {
                        if (IsBaseObservableObject)
                        {
                            p.PrintLine("return base.AttachPropertyChangedListener<TInstance>(propertyName, listener);");
                        }
                        else
                        {
                            p.PrintLine("return false;");
                        }
                    }
                    p.CloseScope();
                    p.PrintEndLine();

                    p.PrintLine($"/// <inheritdoc cref=\"{INOTIFY_PROPERTY_CHANGED_INTERFACE}.NotifyPropertyChanged{{TInstance}}(string, {PROPERTY_CHANGE_EVENT_LISTENER}{{TInstance}})\" />");
                    p.PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
                    p.PrintLine($"public {keyword}bool NotifyPropertyChanged<TInstance>(string propertyName, {PROPERTY_CHANGE_EVENT_LISTENER}<TInstance> listener) where TInstance : class");
                    p.OpenScope();
                    {
                        if (IsBaseObservableObject)
                        {
                            p.PrintLine("return base.NotifyPropertyChanged<TInstance>(propertyName, listener);");
                        }
                        else
                        {
                            p.PrintLine("return false;");
                        }
                    }
                    p.CloseScope();
                    p.PrintEndLine();

                    p.PrintLine($"/// <inheritdoc cref=\"{INOTIFY_PROPERTY_CHANGED_INTERFACE}.NotifyPropertyChanged(string)\" />");
                    p.PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
                    p.PrintLine($"public {keyword}bool NotifyPropertyChanged(string propertyName)");
                    p.OpenScope();
                    {
                        if (IsBaseObservableObject)
                        {
                            p.PrintLine("return base.NotifyPropertyChanged(propertyName);");
                        }
                        else
                        {
                            p.PrintLine("return false;");
                        }
                    }
                    p.CloseScope();
                    p.PrintEndLine();

                    p.PrintLine($"/// <inheritdoc cref=\"{INOTIFY_PROPERTY_CHANGED_INTERFACE}.NotifyPropertyChanged()\" />");
                    p.PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
                    p.PrintLine($"public {keyword}void NotifyPropertyChanged()");
                    p.OpenScope();
                    {
                        if (IsBaseObservableObject)
                        {
                            p.PrintLine("base.NotifyPropertyChanged();");
                        }
                    }
                    p.CloseScope();
                    p.PrintEndLine();

                    WriteTryGetMemberObservableObject_Empty(ref p);
                }
            }
            p.CloseScope();

            p = p.DecreasedIndent();
            return p.Result;
        }

        public string WriteCode()
        {
            var scopePrinter = new SyntaxNodeScopePrinter(Printer.DefaultLarge, Syntax.Parent);
            var p = scopePrinter.printer;
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
                WriteUnionConverters(ref p);
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

        private void WriteClassDeclaration(ref Printer p)
        {
            p.PrintBeginLine("partial class ").Print(ClassName);

            if (IsBaseObservableObject)
            {
                p.PrintEndLine($" : {IOBSERVABLE_OBJECT_INTERFACE}");

                p = p.IncreasedIndent();
                p.Print($", {INOTIFY_PROPERTY_CHANGING_INTERFACE}");
            }
            else
            {
                p.PrintEndLine($" : {INOTIFY_PROPERTY_CHANGING_INTERFACE}");

                p = p.IncreasedIndent();
            }

            p.PrintBeginLine($", {INOTIFY_PROPERTY_CHANGED_INTERFACE}");
            p.PrintEndLine();
            p = p.DecreasedIndent();
        }

        private void WriteConstantFields(ref Printer p)
        {
            var additionalProperties = new Dictionary<string, IPropertySymbol>();

            foreach (var member in FieldRefs)
            {
                var fieldName = member.Field.Name;
                var typeName = member.Field.Type.ToFullName();
                var name = member.PropertyName;

                p.PrintLine($"/// <summary>The name of <see cref=\"{name}\"/></summary>");
                p.PrintLine(GENERATED_PROPERTY_NAME_CONSTANT);

                if (member.IsObservableObject)
                {
                    p.PrintLine(string.Format(IS_OBSERVABLE_OBJECT, typeName));
                }

                p.PrintLine(GENERATED_CODE);
                p.PrintLine($"public const string {ConstName(member)} = nameof({ClassName}.{name});");
                p.PrintEndLine();

                if (NotifyPropertyChangedForMap.TryGetValue(fieldName, out var properties))
                {
                    foreach (var property in properties)
                    {
                        if (additionalProperties.ContainsKey(property.Name) == false)
                        {
                            additionalProperties[property.Name] = property;
                        }
                    }
                }
            }

            foreach (var member in PropRefs)
            {
                var fieldName = member.Property.ToFieldName();
                var typeName = member.Property.Type.ToFullName();
                var name = member.Property.Name;

                p.PrintLine($"/// <summary>The name of <see cref=\"{name}\"/></summary>");
                p.PrintLine(GENERATED_PROPERTY_NAME_CONSTANT);

                if (member.IsObservableObject)
                {
                    p.PrintLine(string.Format(IS_OBSERVABLE_OBJECT, typeName));
                }

                p.PrintLine(GENERATED_CODE);
                p.PrintLine($"public const string {ConstName(member)} = nameof({ClassName}.{name});");
                p.PrintEndLine();

                if (NotifyPropertyChangedForMap.TryGetValue(fieldName, out var properties))
                {
                    foreach (var property in properties)
                    {
                        if (additionalProperties.ContainsKey(property.Name) == false)
                        {
                            additionalProperties[property.Name] = property;
                        }
                    }
                }
            }

            foreach (var property in additionalProperties.Values)
            {
                var name = property.Name;

                p.PrintLine($"/// <summary>The name of <see cref=\"{name}\"/></summary>");
                p.PrintLine(GENERATED_CODE);
                p.PrintLine($"public const string {ConstName(property)} = nameof({ClassName}.{name});");
                p.PrintEndLine();
            }

            p.PrintEndLine();
        }

        private void WriteEvents(ref Printer p)
        {
            foreach (var member in FieldRefs)
            {
                p.PrintLine(GENERATED_CODE).PrintLine(GENERATED_PROPERTY_CHANGING_HANDLER);
                p.PrintLine($"private event global::EncosyTower.Modules.Mvvm.ComponentModel.PropertyChangingEventHandler {OnChangingEventName(member)};");
                p.PrintEndLine();

                p.PrintLine(GENERATED_CODE).PrintLine(GENERATED_PROPERTY_CHANGED_HANDLER);
                p.PrintLine($"private event global::EncosyTower.Modules.Mvvm.ComponentModel.PropertyChangedEventHandler {OnChangedEventName(member)};");
                p.PrintEndLine();
            }

            foreach (var member in PropRefs)
            {
                p.PrintLine(GENERATED_CODE).PrintLine(GENERATED_PROPERTY_CHANGING_HANDLER);
                p.PrintLine($"private event global::EncosyTower.Modules.Mvvm.ComponentModel.PropertyChangingEventHandler {OnChangingEventName(member)};");
                p.PrintEndLine();

                p.PrintLine(GENERATED_CODE).PrintLine(GENERATED_PROPERTY_CHANGED_HANDLER);
                p.PrintLine($"private event global::EncosyTower.Modules.Mvvm.ComponentModel.PropertyChangedEventHandler {OnChangedEventName(member)};");
                p.PrintEndLine();
            }

            var properties = new Dictionary<string, IPropertySymbol>();

            foreach (var propertyList in NotifyPropertyChangedForMap.Values)
            {
                foreach (var property in propertyList)
                {
                    if (properties.ContainsKey(property.Name) == false)
                    {
                        properties[property.Name] = property;
                    }
                }
            }

            foreach (var property in properties.Values)
            {
                p.PrintLine(GENERATED_CODE).PrintLine(GENERATED_PROPERTY_CHANGED_HANDLER);
                p.PrintLine($"private event global::EncosyTower.Modules.Mvvm.ComponentModel.PropertyChangedEventHandler {OnChangedEventName(property)};");
                p.PrintEndLine();
            }
        }

        private void WriteUnionConverters(ref Printer p)
        {
            var types = new Dictionary<string, ITypeSymbol>();

            foreach (var member in FieldRefs)
            {
                var fieldName = member.Field.Name;
                var typeName = member.Field.Type.ToFullName();

                if (types.ContainsKey(typeName) == false)
                {
                    types.Add(typeName, member.Field.Type);
                }

                if (NotifyPropertyChangedForMap.TryGetValue(fieldName, out var properties))
                {
                    foreach (var property in properties)
                    {
                        typeName = property.Type.ToFullName();

                        if (types.ContainsKey(typeName) == false)
                        {
                            types.Add(typeName, property.Type);
                        }
                    }
                }
            }

            foreach (var member in PropRefs)
            {
                var fieldName = member.Property.ToFieldName();
                var typeName = member.Property.Type.ToFullName();

                if (types.ContainsKey(typeName) == false)
                {
                    types.Add(typeName, member.Property.Type);
                }

                if (NotifyPropertyChangedForMap.TryGetValue(fieldName, out var properties))
                {
                    foreach (var property in properties)
                    {
                        typeName = property.Type.ToFullName();

                        if (types.ContainsKey(typeName) == false)
                        {
                            types.Add(typeName, property.Type);
                        }
                    }
                }
            }

            foreach (var type in types.Values)
            {
                var typeName = type.ToFullName();
                var propertyName = type.ToValidIdentifier().AsSpan().ToTitleCase();

                p.PrintLine(GENERATED_CODE);
                p.PrintLine($"private readonly {CACHED_UNION_CONVERTER}<{typeName}> _unionConverter{propertyName} = new {CACHED_UNION_CONVERTER}<{typeName}>();");
                p.PrintEndLine();
            }
        }

        private void WriteObservableForProperties(ref Printer p)
        {
            foreach (var member in PropRefs)
            {
                var fieldName = member.FieldName;
                var propertyName = member.Property.Name;
                var typeName = member.Property.Type.ToFullName();
                var argsName = OnChangedArgsName(member);
                var converterForField = member.Property.Type.ToValidIdentifier().AsSpan().ToTitleCase();
                var converterForFieldVariable = $"unionConverter{converterForField}";
                var willNotifyPropertyChanged = NotifyPropertyChangedForMap.TryGetValue(fieldName, out var properties);
                var constName = ConstName(member);

                foreach (var attribute in member.ForwardedFieldAttributes)
                {
                    p.PrintLine($"[{attribute.GetSyntax().ToFullString()}]");
                }

                if (member.IsObservableObject)
                {
                    p.PrintLine(string.Format(IS_OBSERVABLE_OBJECT, typeName));
                }

                p.PrintLine(GENERATED_CODE);
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
                    p.PrintLine($"if (global::System.Collections.Generic.EqualityComparer<{typeName}>.Default.Equals(this.{fieldName}, value)) return;");
                    p.PrintEndLine();

                    p.OpenScope();
                    {
                        p.PrintLine($"var oldValue = this.{fieldName};");

                        p.PrintEndLine();

                        p.PrintLine($"{OnChangingMethodName(member)}(oldValue, value);");
                        p.PrintEndLine();

                        p.PrintLine($"var {converterForFieldVariable} = this._unionConverter{converterForField};");
                        p.PrintLine($"var {argsName} = new {PROPERTY_CHANGE_EVENT_ARGS}(this, {constName}, {converterForFieldVariable}.ToUnion(oldValue), {converterForFieldVariable}.ToUnion(value));");
                        p.PrintLine($"this.{OnChangingEventName(member)}?.Invoke({argsName});");
                        p.PrintEndLine();

                        p.PrintLine($"this.{fieldName} = value;");
                        p.PrintEndLine();

                        p.PrintLine($"{OnChangedMethodName(member)}(oldValue, value);");
                        p.PrintLine($"this.{OnChangedEventName(member)}?.Invoke({argsName});");
                    }
                    p.CloseScope();
                    p.PrintEndLine();

                    if (willNotifyPropertyChanged)
                    {
                        foreach (var property in properties)
                        {
                            var oldValueName = "oldValueProperty";
                            var otherArgsName = OnChangedArgsName(property);
                            var converterForProperty = property.Type.ToValidIdentifier().AsSpan().ToTitleCase();
                            var converterForPropertyVariable = $"converter{converterForProperty}";

                            p.OpenScope();
                            {
                                p.PrintLine($"var {oldValueName} = this.{property.Name};");
                                p.PrintEndLine();

                                p.PrintLine($"{OnChangedMethodName(property)}({oldValueName}, this.{property.Name});");
                                p.PrintEndLine();

                                p.PrintLine($"var {converterForPropertyVariable} = this._unionConverter{converterForProperty};");
                                p.PrintLine($"var {otherArgsName} = new {PROPERTY_CHANGE_EVENT_ARGS}(this, {ConstName(property)}, {converterForPropertyVariable}.ToUnion({oldValueName}), {converterForPropertyVariable}.ToUnion(this.{property.Name}));");
                                p.PrintLine($"this.{OnChangedEventName(property)}?.Invoke({otherArgsName});");
                            }
                            p.CloseScope();
                            p.PrintEndLine();
                        }
                    }

                    p.PrintEndLine();

                    foreach (var commandName in member.CommandNames)
                    {
                        if (NotifyCanExecuteChangedForSet.Contains(commandName))
                        {
                            p.PrintLine($"{commandName}.NotifyCanExecuteChanged();");
                        }
                    }
                }
                p.CloseScope();
                p.PrintEndLine();
            }
        }

        private void WriteProperties(ref Printer p)
        {
            foreach (var member in FieldRefs)
            {
                var fieldName = member.Field.Name;
                var propertyName = member.PropertyName;
                var typeName = member.Field.Type.ToFullName();
                var argsName = OnChangedArgsName(member);
                var converterForField = member.Field.Type.ToValidIdentifier().AsSpan().ToTitleCase();
                var converterForFieldVariable = $"unionConverter{converterForField}";
                var willNotifyPropertyChanged = NotifyPropertyChangedForMap.TryGetValue(fieldName, out var properties);
                var constName = ConstName(member);

                p.PrintLine($"/// <inheritdoc cref=\"{fieldName}\"/>");

                foreach (var attribute in member.ForwardedPropertyAttributes)
                {
                    p.PrintLine($"[{attribute.GetSyntax().ToFullString()}]");
                }

                if (member.IsObservableObject)
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
                        p.PrintLine($"if (global::System.Collections.Generic.EqualityComparer<{typeName}>.Default.Equals(this.{fieldName}, value)) return;");
                        p.PrintEndLine();

                        p.OpenScope();
                        {
                            p.PrintLine($"var oldValue = this.{fieldName};");

                            p.PrintEndLine();

                            p.PrintLine($"{OnChangingMethodName(member)}(oldValue, value);");
                            p.PrintEndLine();

                            p.PrintLine($"var {converterForFieldVariable} = this._unionConverter{converterForField};");
                            p.PrintLine($"var {argsName} = new {PROPERTY_CHANGE_EVENT_ARGS}(this, {constName}, {converterForFieldVariable}.ToUnion(oldValue), {converterForFieldVariable}.ToUnion(value));");
                            p.PrintLine($"this.{OnChangingEventName(member)}?.Invoke({argsName});");
                            p.PrintEndLine();

                            p.PrintLine($"this.{fieldName} = value;");
                            p.PrintEndLine();

                            p.PrintLine($"{OnChangedMethodName(member)}(oldValue, value);");
                            p.PrintLine($"this.{OnChangedEventName(member)}?.Invoke({argsName});");
                        }
                        p.CloseScope();
                        p.PrintEndLine();

                        if (willNotifyPropertyChanged)
                        {
                            foreach (var property in properties)
                            {
                                var oldValueName = "oldValueProperty";
                                var otherArgsName = OnChangedArgsName(property);
                                var converterForProperty = property.Type.ToValidIdentifier().AsSpan().ToTitleCase();
                                var converterForPropertyVariable = $"converter{converterForProperty}";

                                p.OpenScope();
                                {
                                    p.PrintLine($"var {oldValueName} = this.{property.Name};");
                                    p.PrintEndLine();

                                    p.PrintLine($"{OnChangedMethodName(property)}({oldValueName}, this.{property.Name});");
                                    p.PrintEndLine();

                                    p.PrintLine($"var {converterForPropertyVariable} = this._unionConverter{converterForProperty};");
                                    p.PrintLine($"var {otherArgsName} = new {PROPERTY_CHANGE_EVENT_ARGS}(this, {ConstName(property)}, {converterForPropertyVariable}.ToUnion({oldValueName}), {converterForPropertyVariable}.ToUnion(this.{property.Name}));");
                                    p.PrintLine($"this.{OnChangedEventName(property)}?.Invoke({otherArgsName});");
                                }
                                p.CloseScope();
                                p.PrintEndLine();
                            }
                        }

                        p.PrintEndLine();

                        foreach (var commandName in member.CommandNames)
                        {
                            if (NotifyCanExecuteChangedForSet.Contains(commandName))
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

        private void WritePartialMethods(ref Printer p)
        {
            foreach (var member in FieldRefs)
            {
                var typeName = member.Field.Type.ToFullName();
                var propName = member.PropertyName;

                p.PrintLine($"/// <summary>Executes the logic for when <see cref=\"{propName}\"/> is changing.</summary>");
                p.PrintLine("/// <param name=\"oldValue\">The previous property value that is being replaced.</param>");
                p.PrintLine("/// <param name=\"newValue\">The new property value being set.</param>");
                p.PrintLine($"/// <remarks>This method is invoked right before the value of <see cref=\"{propName}\"/> is changed.</remarks>");
                p.PrintLine(GENERATED_CODE);
                p.PrintLine($"partial void {OnChangingMethodName(member)}({typeName} oldValue, {typeName} newValue);");
                p.PrintEndLine();

                p.PrintLine($"/// <summary>Executes the logic for when <see cref=\"{propName}\"/> just changed.</summary>");
                p.PrintLine("/// <param name=\"oldValue\">The previous property value that was replaced.</param>");
                p.PrintLine("/// <param name=\"newValue\">The new property value that was set.</param>");
                p.PrintLine($"/// <remarks>This method is invoked right after the value of <see cref=\"{propName}\"/> is changed.</remarks>");
                p.PrintLine(GENERATED_CODE);
                p.PrintLine($"partial void {OnChangedMethodName(member)}({typeName} oldValue, {typeName} newValue);");
                p.PrintEndLine();
            }

            foreach (var member in PropRefs)
            {
                var typeName = member.Property.Type.ToFullName();
                var propName = member.Property.Name;

                p.PrintLine($"/// <summary>Executes the logic for when <see cref=\"{propName}\"/> is changing.</summary>");
                p.PrintLine("/// <param name=\"oldValue\">The previous property value that is being replaced.</param>");
                p.PrintLine("/// <param name=\"newValue\">The new property value being set.</param>");
                p.PrintLine($"/// <remarks>This method is invoked right before the value of <see cref=\"{propName}\"/> is changed.</remarks>");
                p.PrintLine(GENERATED_CODE);
                p.PrintLine($"partial void {OnChangingMethodName(member)}({typeName} oldValue, {typeName} newValue);");
                p.PrintEndLine();

                p.PrintLine($"/// <summary>Executes the logic for when <see cref=\"{propName}\"/> just changed.</summary>");
                p.PrintLine("/// <param name=\"oldValue\">The previous property value that was replaced.</param>");
                p.PrintLine("/// <param name=\"newValue\">The new property value that was set.</param>");
                p.PrintLine($"/// <remarks>This method is invoked right after the value of <see cref=\"{propName}\"/> is changed.</remarks>");
                p.PrintLine(GENERATED_CODE);
                p.PrintLine($"partial void {OnChangedMethodName(member)}({typeName} oldValue, {typeName} newValue);");
                p.PrintEndLine();
            }

            var properties = new Dictionary<string, IPropertySymbol>();

            foreach (var propertyList in NotifyPropertyChangedForMap.Values)
            {
                foreach (var property in propertyList)
                {
                    if (properties.ContainsKey(property.Name) == false)
                    {
                        properties[property.Name] = property;
                    }
                }
            }

            foreach (var property in properties.Values)
            {
                var propName = property.Name;
                var typeName = property.Type.ToFullName();

                p.PrintLine($"/// <summary>Executes the logic for when <see cref=\"{propName}\"/> just changed.</summary>");
                p.PrintLine("/// <param name=\"value\">The new property value that was set.</param>");
                p.PrintLine($"/// <remarks>This method is invoked right after the value of <see cref=\"{propName}\"/> is changed.</remarks>");
                p.PrintLine(GENERATED_CODE);
                p.PrintLine($"partial void {OnChangedMethodName(property)}({typeName} oldValue, {typeName} newValue);");
                p.PrintEndLine();
            }
        }

        private void WriteAttachPropertyChangingListenerMethod(ref Printer p)
        {
            var keyword = IsBaseObservableObject ? "override " : Symbol.IsSealed ? "" : "virtual ";

            p.PrintLine($"/// <inheritdoc cref=\"{INOTIFY_PROPERTY_CHANGING_INTERFACE}.AttachPropertyChangingListener{{TInstance}}(string, {PROPERTY_CHANGE_EVENT_LISTENER}{{TInstance}})\" />");
            p.PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
            p.PrintLine($"public {keyword}bool AttachPropertyChangingListener<TInstance>(string propertyName, {PROPERTY_CHANGE_EVENT_LISTENER}<TInstance> listener) where TInstance : class");
            p.OpenScope();
            {
                if (IsBaseObservableObject)
                {
                    p.PrintLine("if (base.AttachPropertyChangingListener<TInstance>(propertyName, listener)) return true;");
                }
                else
                {
                    p.PrintLine("if (listener == null) throw new global::System.ArgumentNullException(nameof(listener));");
                }

                p.PrintEndLine();

                p.PrintLine("switch (propertyName)");
                p.OpenScope();
                {
                    foreach (var member in FieldRefs)
                    {
                        var eventName = OnChangingEventName(member);
                        var constName = ConstName(member);

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

                    foreach (var member in PropRefs)
                    {
                        var eventName = OnChangingEventName(member);
                        var constName = ConstName(member);

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

        private void WriteAttachPropertyChangedListenerMethod(ref Printer p)
        {
            var keyword = IsBaseObservableObject ? "override " : Symbol.IsSealed ? "" : "virtual ";

            p.PrintLine($"/// <inheritdoc cref=\"{INOTIFY_PROPERTY_CHANGED_INTERFACE}.AttachPropertyChangedListener{{TInstance}}(string, {PROPERTY_CHANGE_EVENT_LISTENER}{{TInstance}})\" />");
            p.PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
            p.PrintLine($"public {keyword}bool AttachPropertyChangedListener<TInstance>(string propertyName, {PROPERTY_CHANGE_EVENT_LISTENER}<TInstance> listener) where TInstance : class");
            p.OpenScope();
            {
                if (IsBaseObservableObject)
                {
                    p.PrintLine("if (base.AttachPropertyChangedListener<TInstance>(propertyName, listener)) return true;");
                }
                else
                {
                    p.PrintLine("if (listener == null) throw new global::System.ArgumentNullException(nameof(listener));");
                }

                p.PrintEndLine();

                p.PrintLine("switch (propertyName)");
                p.OpenScope();
                {
                    foreach (var member in FieldRefs)
                    {
                        var eventName = OnChangedEventName(member);
                        var constName = ConstName(member);

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

                    foreach (var member in PropRefs)
                    {
                        var eventName = OnChangedEventName(member);
                        var constName = ConstName(member);

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

                    var properties = new Dictionary<string, IPropertySymbol>();

                    foreach (var propertyList in NotifyPropertyChangedForMap.Values)
                    {
                        foreach (var property in propertyList)
                        {
                            if (properties.ContainsKey(property.Name) == false)
                            {
                                properties[property.Name] = property;
                            }
                        }
                    }

                    foreach (var property in properties.Values)
                    {
                        var constName = ConstName(property);
                        var eventName = OnChangedEventName(property);

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

        private void WriteNotifyPropertyChangedWithTwoArgumentsMethod(ref Printer p)
        {
            var keyword = IsBaseObservableObject ? "override " : Symbol.IsSealed ? "" : "virtual ";

            p.PrintLine($"/// <inheritdoc cref=\"{INOTIFY_PROPERTY_CHANGED_INTERFACE}.NotifyPropertyChanged{{TInstance}}(string, {PROPERTY_CHANGE_EVENT_LISTENER}{{TInstance}})\" />");
            p.PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
            p.PrintLine($"public {keyword}bool NotifyPropertyChanged<TInstance>(string propertyName, {PROPERTY_CHANGE_EVENT_LISTENER}<TInstance> listener) where TInstance : class");
            p.OpenScope();
            {
                if (IsBaseObservableObject)
                {
                    p.PrintLine("if (base.NotifyPropertyChanged<TInstance>(propertyName, listener)) return true;");
                }
                else
                {
                    p.PrintLine("if (listener == null) throw new global::System.ArgumentNullException(nameof(listener));");
                }

                p.PrintEndLine();

                p.PrintLine("switch (propertyName)");
                p.OpenScope();
                {
                    foreach (var member in FieldRefs)
                    {
                        var fieldName = member.Field.Name;
                        var argsName = OnChangedArgsName(member);
                        var converterForField = member.Field.Type.ToValidIdentifier().AsSpan().ToTitleCase();
                        var converterForFieldVariable = $"unionConverter{converterForField}";
                        var constName = ConstName(member);

                        p.PrintLine($"case {constName}:");
                        p.OpenScope();
                        {
                            p.PrintLine($"var {converterForFieldVariable} = this._unionConverter{converterForField};");
                            p.PrintLine($"var union = {converterForFieldVariable}.ToUnion(this.{fieldName});");
                            p.PrintLine($"var {argsName} = new {PROPERTY_CHANGE_EVENT_ARGS}(this, {constName}, union, union);");
                            p.PrintLine($"listener.OnEvent({argsName});");
                            p.PrintLine("return true;");
                        }
                        p.CloseScope();
                        p.PrintEndLine();
                    }

                    foreach (var member in PropRefs)
                    {
                        var fieldName = member.FieldName;
                        var argsName = OnChangedArgsName(member);
                        var converterForField = member.Property.Type.ToValidIdentifier().AsSpan().ToTitleCase();
                        var converterForFieldVariable = $"unionConverter{converterForField}";
                        var constName = ConstName(member);

                        p.PrintLine($"case {constName}:");
                        p.OpenScope();
                        {
                            p.PrintLine($"var {converterForFieldVariable} = this._unionConverter{converterForField};");
                            p.PrintLine($"var union = {converterForFieldVariable}.ToUnion(this.{fieldName});");
                            p.PrintLine($"var {argsName} = new {PROPERTY_CHANGE_EVENT_ARGS}(this, {constName}, union, union);");
                            p.PrintLine($"listener.OnEvent({argsName});");
                            p.PrintLine("return true;");
                        }
                        p.CloseScope();
                        p.PrintEndLine();
                    }

                    var properties = new Dictionary<string, IPropertySymbol>();

                    foreach (var propertyList in NotifyPropertyChangedForMap.Values)
                    {
                        foreach (var property in propertyList)
                        {
                            if (properties.ContainsKey(property.Name) == false)
                            {
                                properties[property.Name] = property;
                            }
                        }
                    }

                    foreach (var property in properties.Values)
                    {
                        var constName = ConstName(property);
                        var otherArgsName = OnChangedArgsName(property);
                        var converterForProperty = property.Type.ToValidIdentifier().AsSpan().ToTitleCase();
                        var converterForPropertyVariable = $"converter{converterForProperty}";

                        p.PrintLine($"case {constName}:");
                        p.OpenScope();
                        {
                            p.PrintLine($"var {converterForPropertyVariable} = this._unionConverter{converterForProperty};");
                            p.PrintLine($"var union = {converterForPropertyVariable}.ToUnion(this.{property.Name});");
                            p.PrintLine($"var {otherArgsName} = new {PROPERTY_CHANGE_EVENT_ARGS}(this, {ConstName(property)}, union, union);");
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

        private void WriteNotifyPropertyChangedWithOneArgumentMethod(ref Printer p)
        {
            var keyword = IsBaseObservableObject ? "override " : Symbol.IsSealed ? "" : "virtual ";

            p.PrintLine($"/// <inheritdoc cref=\"{INOTIFY_PROPERTY_CHANGED_INTERFACE}.NotifyPropertyChanged(string)\" />");
            p.PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
            p.PrintLine($"public {keyword}bool NotifyPropertyChanged(string propertyName)");
            p.OpenScope();
            {
                if (IsBaseObservableObject)
                {
                    p.PrintLine("if (base.NotifyPropertyChanged(propertyName)) return true;");
                }

                p.PrintEndLine();

                p.PrintLine("switch (propertyName)");
                p.OpenScope();
                {
                    foreach (var member in FieldRefs)
                    {
                        var fieldName = member.Field.Name;
                        var argsName = OnChangedArgsName(member);
                        var converterForField = member.Field.Type.ToValidIdentifier().AsSpan().ToTitleCase();
                        var converterForFieldVariable = $"unionConverter{converterForField}";
                        var constName = ConstName(member);

                        p.PrintLine($"case {constName}:");
                        p.OpenScope();
                        {
                            p.PrintLine($"var {converterForFieldVariable} = this._unionConverter{converterForField};");
                            p.PrintLine($"var union = {converterForFieldVariable}.ToUnion(this.{fieldName});");
                            p.PrintLine($"var {argsName} = new {PROPERTY_CHANGE_EVENT_ARGS}(this, {constName}, union, union);");
                            p.PrintLine($"this.{OnChangedEventName(member)}?.Invoke({argsName});");
                            p.PrintLine("return true;");
                        }
                        p.CloseScope();
                        p.PrintEndLine();
                    }

                    foreach (var member in PropRefs)
                    {
                        var fieldName = member.FieldName;
                        var argsName = OnChangedArgsName(member);
                        var converterForField = member.Property.Type.ToValidIdentifier().AsSpan().ToTitleCase();
                        var converterForFieldVariable = $"unionConverter{converterForField}";
                        var constName = ConstName(member);

                        p.PrintLine($"case {constName}:");
                        p.OpenScope();
                        {
                            p.PrintLine($"var {converterForFieldVariable} = this._unionConverter{converterForField};");
                            p.PrintLine($"var union = {converterForFieldVariable}.ToUnion(this.{fieldName});");
                            p.PrintLine($"var {argsName} = new {PROPERTY_CHANGE_EVENT_ARGS}(this, {constName}, union, union);");
                            p.PrintLine($"this.{OnChangedEventName(member)}?.Invoke({argsName});");
                            p.PrintLine("return true;");
                        }
                        p.CloseScope();
                        p.PrintEndLine();
                    }

                    var properties = new Dictionary<string, IPropertySymbol>();

                    foreach (var propertyList in NotifyPropertyChangedForMap.Values)
                    {
                        foreach (var property in propertyList)
                        {
                            if (properties.ContainsKey(property.Name) == false)
                            {
                                properties[property.Name] = property;
                            }
                        }
                    }

                    foreach (var property in properties.Values)
                    {
                        var constName = ConstName(property);
                        var otherArgsName = OnChangedArgsName(property);
                        var converterForProperty = property.Type.ToValidIdentifier().AsSpan().ToTitleCase();
                        var converterForPropertyVariable = $"converter{converterForProperty}";

                        p.PrintLine($"case {constName}:");
                        p.OpenScope();
                        {
                            p.PrintLine($"var {converterForPropertyVariable} = this._unionConverter{converterForProperty};");
                            p.PrintLine($"var union = {converterForPropertyVariable}.ToUnion(this.{property.Name});");
                            p.PrintLine($"var {otherArgsName} = new {PROPERTY_CHANGE_EVENT_ARGS}(this, {ConstName(property)}, union, union);");
                            p.PrintLine($"this.{OnChangedEventName(property)}?.Invoke({otherArgsName});");
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

        private void WriteNotifyPropertyChangedNoArgumentMethod(ref Printer p)
        {
            var keyword = IsBaseObservableObject ? "override " : Symbol.IsSealed ? "" : "virtual ";

            p.PrintLine($"/// <inheritdoc cref=\"{INOTIFY_PROPERTY_CHANGED_INTERFACE}.NotifyPropertyChanged()\" />");
            p.PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
            p.PrintLine($"public {keyword}void NotifyPropertyChanged()");
            p.OpenScope();
            {
                if (IsBaseObservableObject)
                {
                    p.PrintLine("base.NotifyPropertyChanged();");
                }

                p.PrintEndLine();

                foreach (var member in FieldRefs)
                {
                    var fieldName = member.Field.Name;
                    var argsName = OnChangedArgsName(member);
                    var converterForField = member.Field.Type.ToValidIdentifier().AsSpan().ToTitleCase();
                    var converterForFieldVariable = $"unionConverter{converterForField}";
                    var constName = ConstName(member);

                    p.OpenScope();
                    {
                        p.PrintLine($"var {converterForFieldVariable} = this._unionConverter{converterForField};");
                        p.PrintLine($"var union = {converterForFieldVariable}.ToUnion(this.{fieldName});");
                        p.PrintLine($"var {argsName} = new {PROPERTY_CHANGE_EVENT_ARGS}(this, {constName}, union, union);");
                        p.PrintLine($"this.{OnChangedEventName(member)}?.Invoke({argsName});");
                    }
                    p.CloseScope();
                    p.PrintEndLine();
                }

                foreach (var member in PropRefs)
                {
                    var fieldName = member.FieldName;
                    var argsName = OnChangedArgsName(member);
                    var converterForField = member.Property.Type.ToValidIdentifier().AsSpan().ToTitleCase();
                    var converterForFieldVariable = $"unionConverter{converterForField}";
                    var constName = ConstName(member);

                    p.OpenScope();
                    {
                        p.PrintLine($"var {converterForFieldVariable} = this._unionConverter{converterForField};");
                        p.PrintLine($"var union = {converterForFieldVariable}.ToUnion(this.{fieldName});");
                        p.PrintLine($"var {argsName} = new {PROPERTY_CHANGE_EVENT_ARGS}(this, {constName}, union, union);");
                        p.PrintLine($"this.{OnChangedEventName(member)}?.Invoke({argsName});");
                    }
                    p.CloseScope();
                    p.PrintEndLine();
                }

                var properties = new Dictionary<string, IPropertySymbol>();

                foreach (var propertyList in NotifyPropertyChangedForMap.Values)
                {
                    foreach (var property in propertyList)
                    {
                        if (properties.ContainsKey(property.Name) == false)
                        {
                            properties[property.Name] = property;
                        }
                    }
                }

                foreach (var property in properties.Values)
                {
                    var constName = ConstName(property);
                    var otherArgsName = OnChangedArgsName(property);
                    var converterForProperty = property.Type.ToValidIdentifier().AsSpan().ToTitleCase();
                    var converterForPropertyVariable = $"converter{converterForProperty}";

                    p.OpenScope();
                    {
                        p.PrintLine($"var {converterForPropertyVariable} = this._unionConverter{converterForProperty};");
                        p.PrintLine($"var union = {converterForPropertyVariable}.ToUnion(this.{property.Name});");
                        p.PrintLine($"var {otherArgsName} = new {PROPERTY_CHANGE_EVENT_ARGS}(this, {constName}, union, union);");
                        p.PrintLine($"this.{OnChangedEventName(property)}?.Invoke({otherArgsName});");
                    }
                    p.CloseScope();
                    p.PrintEndLine();
                }
            }
            p.CloseScope();
            p.PrintEndLine();
        }

        private void WriteNotifyPropertyChangingInfoAttributes(ref Printer p)
        {
            const string ATTRIBUTE = "[global::EncosyTower.Modules.Mvvm.ComponentModel.SourceGen.NotifyPropertyChangingInfo(\"{0}\", typeof({1}))]";

            foreach (var member in FieldRefs)
            {
                var propName = member.GetPropertyName();
                var typeName = member.Field.Type.ToFullName();

                p.PrintLine(string.Format(ATTRIBUTE, propName, typeName));
            }

            foreach (var member in PropRefs)
            {
                var propName = member.GetPropertyName();
                var typeName = member.Property.Type.ToFullName();

                p.PrintLine(string.Format(ATTRIBUTE, propName, typeName));
            }
        }

        private void WriteNotifyPropertyChangedInfoAttributes(ref Printer p)
        {
            const string ATTRIBUTE = "[global::EncosyTower.Modules.Mvvm.ComponentModel.SourceGen.NotifyPropertyChangedInfo(\"{0}\", typeof({1}))]";

            var additionalProperties = new Dictionary<string, IPropertySymbol>();

            foreach (var member in FieldRefs)
            {
                var fieldName = member.Field.Name;
                var propName = member.GetPropertyName();
                var typeName = member.Field.Type.ToFullName();

                p.PrintLine(string.Format(ATTRIBUTE, propName, typeName));

                if (NotifyPropertyChangedForMap.TryGetValue(fieldName, out var properties))
                {
                    foreach (var property in properties)
                    {
                        if (additionalProperties.ContainsKey(property.Name) == false)
                        {
                            additionalProperties[property.Name] = property;
                        }
                    }
                }
            }

            foreach (var member in PropRefs)
            {
                var fieldName = member.Property.ToFieldName();
                var propName = member.GetPropertyName();
                var typeName = member.Property.Type.ToFullName();

                p.PrintLine(string.Format(ATTRIBUTE, propName, typeName));

                if (NotifyPropertyChangedForMap.TryGetValue(fieldName, out var properties))
                {
                    foreach (var property in properties)
                    {
                        if (additionalProperties.ContainsKey(property.Name) == false)
                        {
                            additionalProperties[property.Name] = property;
                        }
                    }
                }
            }

            foreach (var property in additionalProperties.Values)
            {
                p.PrintLine(string.Format(ATTRIBUTE, property.Name, property.Type.ToFullName()));
            }
        }

        private static void WriteTryGetMemberObservableObject_Empty(ref Printer p)
        {
            p.PrintLine($"/// <inheritdoc cref=\"{IOBSERVABLE_OBJECT_INTERFACE}.TryGetMemberObservableObject(global::System.Collections.Generic.Queue{{string}}, out {IOBSERVABLE_OBJECT_INTERFACE})\"/>");
            p.PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
            p.PrintLine($"public bool TryGetMemberObservableObject(global::System.Collections.Generic.Queue<string> propertyNames, out {IOBSERVABLE_OBJECT_INTERFACE} result)");
            p.OpenScope();
            {
                p.PrintLine("result = default;");
                p.PrintLine("return false;");
            }
            p.CloseScope();
            p.PrintEndLine();
        }

        private void WriteTryGetMemberObservableObject(ref Printer p)
        {
            if (HasMemberObservableObject == false)
            {
                WriteTryGetMemberObservableObject_Empty(ref p);
                return;
            }

            p.PrintLine($"/// <inheritdoc cref=\"{IOBSERVABLE_OBJECT_INTERFACE}.TryGetMemberObservableObject(global::System.Collections.Generic.Queue{{string}}, out {IOBSERVABLE_OBJECT_INTERFACE})\"/>");
            p.PrintLine(GENERATED_CODE).PrintLine(EXCLUDE_COVERAGE);
            p.PrintLine($"public bool TryGetMemberObservableObject(global::System.Collections.Generic.Queue<string> propertyNames, out {IOBSERVABLE_OBJECT_INTERFACE} result)");
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
                        foreach (var member in FieldRefs)
                        {
                            if (member.IsObservableObject == false)
                            {
                                continue;
                            }

                            var fieldName = member.Field.Name;
                            var typeName = member.Field.Type.ToFullName();
                            var constName = ConstName(member);

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

                        foreach (var member in PropRefs)
                        {
                            if (member.IsObservableObject == false)
                            {
                                continue;
                            }

                            var propName = member.Property.Name;
                            var typeName = member.Property.Type.ToFullName();
                            var constName = ConstName(member);

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

        private static string ConstName(MemberRef member)
            => $"PropertyName_{member.GetPropertyName()}";

        private static string ConstName(IPropertySymbol member)
            => $"PropertyName_{member.Name}";

        private static string OnChangingEventName(MemberRef member)
            => $"_onChanging{member.GetPropertyName()}";

        private static string OnChangedEventName(MemberRef member)
            => $"_onChanged{member.GetPropertyName()}";

        private static string OnChangedEventName(ISymbol member)
            => $"_onChanged{member.Name}";

        private static string OnChangedArgsName(ISymbol member)
            => $"args{member.Name}";

        private static string OnChangedArgsName(MemberRef member)
            => $"args{member.GetPropertyName()}";

        private static string OnChangingMethodName(MemberRef member)
            => $"On{member.GetPropertyName()}Changing";

        private static string OnChangedMethodName(MemberRef member)
            => $"On{member.GetPropertyName()}Changed";

        private static string OnChangedMethodName(IPropertySymbol member)
            => $"On{member.Name}Changed";
    }
}
