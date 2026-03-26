using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace EncosyTower.SourceGen.Generators.Mvvm.ObservableProperties
{
    /// <summary>
    /// Cache-friendly, equatable pipeline model for the ObservableProperty source generator.
    /// Holds only primitive values and equatable collections — no <see cref="SyntaxNode"/>
    /// or <see cref="ISymbol"/> references — so that Roslyn's incremental generator engine
    /// can cache and compare instances cheaply across multiple compilations.
    /// </summary>
    public partial struct ObservablePropertyDeclaration : IEquatable<ObservablePropertyDeclaration>
    {
        public const string IOBSERVABLE_OBJECT_INTERFACE = "global::EncosyTower.Mvvm.ComponentModel.IObservableObject";
        public const string OBSERVABLE_OBJECT_ATTRIBUTE_METADATA = "EncosyTower.Mvvm.ComponentModel.ObservableObjectAttribute";
        public const string OBSERVABLE_PROPERTY_ATTRIBUTE = "global::EncosyTower.Mvvm.ComponentModel.ObservablePropertyAttribute";
        public const string NOTIFY_PROPERTY_CHANGED_FOR_ATTRIBUTE = "global::EncosyTower.Mvvm.ComponentModel.NotifyPropertyChangedForAttribute";
        public const string NOTIFY_CAN_EXECUTE_CHANGED_FOR_ATTRIBUTE = "global::EncosyTower.Mvvm.ComponentModel.NotifyCanExecuteChangedForAttribute";
        public const string RELAY_COMMAND_ATTRIBUTE = "global::EncosyTower.Mvvm.Input.RelayCommandAttribute";

        public const string SERIALIZABLE_ATTRIBUTE = "global::System.SerializableAttribute";
        public const string SERIALIZE_FIELD_ATTRIBUTE = "global::UnityEngine.SerializeField";

        public const string GENERATE_PROPERTY_BAG_ATTRIBUTE = "global::Unity.Properties.GeneratePropertyBagAttribute";
        public const string CREATE_PROPERTY_ATTRIBUTE = "global::Unity.Properties.CreatePropertyAttribute";
        public const string CREATE_PROPERTY = "global::Unity.Properties.CreateProperty";
        public const string DONT_CREATE_PROPERTY_ATTRIBUTE = "global::Unity.Properties.DontCreatePropertyAttribute";
        public const string DONT_CREATE_PROPERTY = "global::Unity.Properties.DontCreateProperty";

        /// <summary>Excluded from <see cref="Equals(ObservablePropertyDeclaration)"/> and
        /// <see cref="GetHashCode"/> — location data is not stable across incremental runs.</summary>
        public LocationInfo location;

        public string className;
        public string hintName;
        public string sourceFilePath;
        public string openingSource;
        public string closingSource;
        public bool isBaseObservableObject;
        public bool isSealed;
        public bool hasMemberObservableObject;
        public bool hasSerializableAttribute;
        public bool hasGeneratePropertyBagAttribute;
        public EquatableArray<FieldMemberDeclaration> fieldRefs;
        public EquatableArray<PropMemberDeclaration> propRefs;

        /// <summary>Flattened entries of the NotifyPropertyChangedFor map.
        /// Each entry records which observable member (by key) should also fire
        /// change notifications for the target property (by name/type).</summary>
        public EquatableArray<NotifyForEntry> notifyForEntries;

        /// <summary>Set of command names (e.g. "SaveCommand") that should receive
        /// <c>NotifyCanExecuteChanged</c> when an observable member changes.</summary>
        public EquatableArray<string> notifyCanExecuteChangedFor;

        public readonly bool IsValid
            => string.IsNullOrEmpty(className) == false
            && string.IsNullOrEmpty(hintName) == false;

        public readonly bool Equals(ObservablePropertyDeclaration other)
            => string.Equals(className, other.className, StringComparison.Ordinal)
            && string.Equals(hintName, other.hintName, StringComparison.Ordinal)
            && string.Equals(sourceFilePath, other.sourceFilePath, StringComparison.Ordinal)
            && string.Equals(openingSource, other.openingSource, StringComparison.Ordinal)
            && string.Equals(closingSource, other.closingSource, StringComparison.Ordinal)
            && isBaseObservableObject == other.isBaseObservableObject
            && isSealed == other.isSealed
            && hasMemberObservableObject == other.hasMemberObservableObject
            && hasSerializableAttribute == other.hasSerializableAttribute
            && hasGeneratePropertyBagAttribute == other.hasGeneratePropertyBagAttribute
            && fieldRefs.Equals(other.fieldRefs)
            && propRefs.Equals(other.propRefs)
            && notifyForEntries.Equals(other.notifyForEntries)
            && notifyCanExecuteChangedFor.Equals(other.notifyCanExecuteChangedFor);

        public readonly override bool Equals(object obj)
            => obj is ObservablePropertyDeclaration other && Equals(other);

        public readonly override int GetHashCode()
        {
            var hash = new HashValue();
            hash.Add(className);
            hash.Add(hintName);
            hash.Add(sourceFilePath);
            hash.Add(openingSource);
            hash.Add(closingSource);
            hash.Add(isBaseObservableObject);
            hash.Add(isSealed);
            hash.Add(hasMemberObservableObject);
            hash.Add(hasSerializableAttribute);
            hash.Add(hasGeneratePropertyBagAttribute);
            hash.Add(fieldRefs);
            hash.Add(propRefs);
            hash.Add(notifyForEntries);
            hash.Add(notifyCanExecuteChangedFor);
            return hash.ToHashCode();
        }

        /// <summary>
        /// Extracts all observable-property metadata from the annotated class symbol into a fully
        /// populated, cache-friendly <see cref="ObservablePropertyDeclaration"/>.
        /// Called once per class inside the <c>ForAttributeWithMetadataName</c> transform.
        /// </summary>
        public static ObservablePropertyDeclaration Extract(
              GeneratorAttributeSyntaxContext context
            , CancellationToken token
        )
        {
            token.ThrowIfCancellationRequested();

            if (context.TargetNode is not ClassDeclarationSyntax classSyntax
                || context.TargetSymbol is not INamedTypeSymbol classSymbol
            )
            {
                return default;
            }

            var classNameSb = new StringBuilder(classSyntax.Identifier.Text);

            if (classSyntax.TypeParameterList is TypeParameterListSyntax typeParamList
                && typeParamList.Parameters.Count > 0
            )
            {
                classNameSb.Append("<");

                var typeParams = typeParamList.Parameters;
                var last = typeParams.Count - 1;

                for (var i = 0; i <= last; i++)
                {
                    classNameSb.Append(typeParams[i].Identifier.Text);

                    if (i < last)
                    {
                        classNameSb.Append(", ");
                    }
                }

                classNameSb.Append(">");
            }

            var className = classNameSb.ToString();
            var semanticModel = context.SemanticModel;
            var syntaxTree = classSyntax.SyntaxTree;
            var fileTypeName = classSymbol.ToFileName();

            var hintName = syntaxTree.GetGeneratedSourceFileName(
                  ObservablePropertyGenerator.GENERATOR_NAME
                , classSyntax
                , fileTypeName
            );

            var sourceFilePath = syntaxTree.GetGeneratedSourceFilePath(
                  semanticModel.Compilation.AssemblyName
                , ObservablePropertyGenerator.GENERATOR_NAME
                , fileTypeName
            );

            TypeCreationHelpers.GenerateOpeningAndClosingSource(
                  classSyntax
                , token
                , out var openingSource
                , out var closingSource
                , printAdditionalUsings: PrintAdditionalUsings
            );

            var isBaseObservableObject = false;

            if (classSymbol.BaseType != null && classSymbol.BaseType.TypeKind == TypeKind.Class)
            {
                if (classSymbol.BaseType.ImplementsInterface(IOBSERVABLE_OBJECT_INTERFACE))
                {
                    isBaseObservableObject = true;
                }
            }

            using var fieldRefsBuilder = ImmutableArrayBuilder<FieldMemberDeclaration>.Rent();
            using var propRefsBuilder = ImmutableArrayBuilder<PropMemberDeclaration>.Rent();
            using var diagnosticBuilder = ImmutableArrayBuilder<DiagnosticInfo>.Rent();

            var hasSerializableAttribute = classSymbol.HasAttribute(SERIALIZABLE_ATTRIBUTE);
            var hasGeneratePropertyBagAttribute = classSymbol.HasAttribute(GENERATE_PROPERTY_BAG_ATTRIBUTE);
            var hasMemberObservableObject = false;

            var members = classSymbol.GetMembers();
            var propertyChangedMap = new Dictionary<string, List<string>>();
            var commandSet = new HashSet<string>();
            var propertyMap = new Dictionary<string, (string propTypeName, string propTypeValidIdent)>();
            var methods = new List<IMethodSymbol>();

            foreach (var member in members)
            {
                if (member is IFieldSymbol field)
                {
                    if (field.HasAttribute(OBSERVABLE_PROPERTY_ATTRIBUTE))
                    {
                        var fieldTypeName = field.Type.ToFullName();
                        var isObservableObject = field.Type.ImplementsInterface(IOBSERVABLE_OBJECT_INTERFACE);

                        if (isObservableObject)
                        {
                            hasMemberObservableObject = true;
                        }

                        var uniqueCommandNames = new HashSet<string>();
                        var notifyPropChangedFors = field.GetAttributes(NOTIFY_PROPERTY_CHANGED_FOR_ATTRIBUTE);

                        foreach (var notifyPropChangedFor in notifyPropChangedFors)
                        {
                            if (notifyPropChangedFor != null
                                && notifyPropChangedFor.ConstructorArguments.Length > 0
                                && notifyPropChangedFor.ConstructorArguments[0].Value is string propName
                            )
                            {
                                if (propertyChangedMap.TryGetValue(field.Name, out var propNames) == false)
                                {
                                    propertyChangedMap[field.Name] = propNames = new List<string>();
                                }

                                propNames.Add(propName);
                            }
                        }

                        var notifyCanExecuteChangedFors = field.GetAttributes(NOTIFY_CAN_EXECUTE_CHANGED_FOR_ATTRIBUTE);

                        using var commandNames = ImmutableArrayBuilder<string>.Rent();

                        foreach (var notifyCanExecuteChangedFor in notifyCanExecuteChangedFors)
                        {
                            if (notifyCanExecuteChangedFor != null
                                && notifyCanExecuteChangedFor.ConstructorArguments.Length > 0
                            )
                            {
                                var args = notifyCanExecuteChangedFor.ConstructorArguments;

                                foreach (var arg in args)
                                {
                                    if (arg.Value is string commandName)
                                    {
                                        uniqueCommandNames.Add(commandName);
                                        commandSet.Add(commandName);
                                    }
                                }
                            }

                            commandNames.AddRange(uniqueCommandNames);
                        }

                        field.GatherForwardedAttributes(
                              semanticModel
                            , token
                            , diagnosticBuilder
                            , out ImmutableArray<AttributeInfo> propertyAttributes
                            , DiagnosticDescriptors.InvalidPropertyTargetedAttributeOnObservableProperty
                        );

                        fieldRefsBuilder.Add(new FieldMemberDeclaration {
                            location = LocationInfo.From(field.Locations.Length > 0 ? field.Locations[0] : Location.None),
                            fieldName = field.Name,
                            propertyName = field.ToPropertyName(),
                            fieldTypeName = fieldTypeName,
                            fieldTypeValidIdent = field.Type.ToValidIdentifier(),
                            isObservableObject = isObservableObject,
                            hasSerializeFieldAttribute = field.HasAttribute(SERIALIZE_FIELD_ATTRIBUTE),
                            commandNames = commandNames.ToImmutable().AsEquatableArray(),
                            forwardedPropertyAttributes = propertyAttributes.AsEquatableArray(),
                        });
                    }

                    continue;
                }

                if (member is IPropertySymbol property)
                {
                    if (property.HasAttribute(OBSERVABLE_PROPERTY_ATTRIBUTE) == false)
                    {
                        propertyMap[property.Name] = (property.Type.ToFullName(), property.Type.ToValidIdentifier());
                    }
                    else
                    {
                        var propertyTypeName = property.Type.ToFullName();
                        var fieldName = property.ToPrivateFieldName();
                        var isObservableObject = property.Type.ImplementsInterface(IOBSERVABLE_OBJECT_INTERFACE);

                        if (isObservableObject)
                        {
                            hasMemberObservableObject = true;
                        }

                        var uniqueCommandNames = new HashSet<string>();
                        var notifyPropChangedFors = property.GetAttributes(NOTIFY_PROPERTY_CHANGED_FOR_ATTRIBUTE);

                        foreach (var notifyPropChangedFor in notifyPropChangedFors)
                        {
                            if (notifyPropChangedFor != null
                                && notifyPropChangedFor.ConstructorArguments.Length > 0
                                && notifyPropChangedFor.ConstructorArguments[0].Value is string propName
                            )
                            {
                                if (propertyChangedMap.TryGetValue(fieldName, out var propNames) == false)
                                {
                                    propertyChangedMap[fieldName] = propNames = new List<string>();
                                }

                                propNames.Add(propName);
                            }
                        }

                        var notifyCanExecuteChangedFors = property.GetAttributes(NOTIFY_CAN_EXECUTE_CHANGED_FOR_ATTRIBUTE);

                        using var commandNames = ImmutableArrayBuilder<string>.Rent();

                        foreach (var notifyCanExecuteChangedFor in notifyCanExecuteChangedFors)
                        {
                            if (notifyCanExecuteChangedFor != null
                                && notifyCanExecuteChangedFor.ConstructorArguments.Length > 0
                            )
                            {
                                var args = notifyCanExecuteChangedFor.ConstructorArguments;

                                foreach (var arg in args)
                                {
                                    if (arg.Value is string commandName)
                                    {
                                        uniqueCommandNames.Add(commandName);
                                        commandSet.Add(commandName);
                                    }
                                }
                            }

                            commandNames.AddRange(uniqueCommandNames);
                        }

                        property.GatherForwardedAttributes(
                              semanticModel
                            , token
                            , diagnosticBuilder
                            , out ImmutableArray<(string, AttributeInfo)> fieldAttributes
                            , DiagnosticDescriptors.InvalidFieldMethodTargetedAttributeOnObservableProperty
                        );

                        using var forwardedFieldAttribsBuilder = ImmutableArrayBuilder<ForwardedFieldAttribute>.Rent();

                        foreach (var (typeName, attribInfo) in fieldAttributes)
                        {
                            forwardedFieldAttribsBuilder.Add(new ForwardedFieldAttribute { typeName = typeName, attributeInfo = attribInfo });
                        }

                        propRefsBuilder.Add(new PropMemberDeclaration {
                            location = LocationInfo.From(property.Locations.Length > 0 ? property.Locations[0] : Location.None),
                            propertyName = property.Name,
                            fieldName = fieldName,
                            propertyTypeName = propertyTypeName,
                            propertyTypeValidIdent = property.Type.ToValidIdentifier(),
                            isObservableObject = isObservableObject,
                            doesCreateProperty = property.HasAttribute(CREATE_PROPERTY_ATTRIBUTE),
                            commandNames = commandNames.ToImmutable().AsEquatableArray(),
                            forwardedFieldAttributes = forwardedFieldAttribsBuilder.ToImmutable().AsEquatableArray(),
                        });
                    }

                    continue;
                }

                if (member is IMethodSymbol method && method.Parameters.Length <= 1)
                {
                    if (method.HasAttribute(RELAY_COMMAND_ATTRIBUTE))
                    {
                        methods.Add(method);
                    }

                    continue;
                }
            }

            // Build flattened NotifyForEntry array from propertyChangedMap + propertyMap
            using var notifyForBuilder = ImmutableArrayBuilder<NotifyForEntry>.Rent();

            foreach (var kv in propertyChangedMap)
            {
                var memberKey = kv.Key;

                foreach (var propName in kv.Value)
                {
                    if (propertyMap.TryGetValue(propName, out var propInfo) == false)
                    {
                        continue;
                    }

                    notifyForBuilder.Add(new NotifyForEntry {
                        memberKey = memberKey,
                        propName = propName,
                        propTypeName = propInfo.propTypeName,
                        propTypeValidIdent = propInfo.propTypeValidIdent,
                    });
                }
            }

            // Build notifyCanExecuteChangedFor array
            using var notifyCanExecBuilder = ImmutableArrayBuilder<string>.Rent();

            foreach (var method in methods)
            {
                var commandName = $"{method.Name}Command";

                if (commandSet.Contains(commandName))
                {
                    notifyCanExecBuilder.Add(commandName);
                }
            }

            return new ObservablePropertyDeclaration {
                location = LocationInfo.From(classSyntax.GetLocation()),
                className = className,
                hintName = hintName,
                sourceFilePath = sourceFilePath,
                openingSource = openingSource,
                closingSource = closingSource,
                isBaseObservableObject = isBaseObservableObject,
                isSealed = classSymbol.IsSealed,
                hasMemberObservableObject = hasMemberObservableObject,
                hasSerializableAttribute = hasSerializableAttribute,
                hasGeneratePropertyBagAttribute = hasGeneratePropertyBagAttribute,
                fieldRefs = fieldRefsBuilder.ToImmutable().AsEquatableArray(),
                propRefs = propRefsBuilder.ToImmutable().AsEquatableArray(),
                notifyForEntries = notifyForBuilder.ToImmutable().AsEquatableArray(),
                notifyCanExecuteChangedFor = notifyCanExecBuilder.ToImmutable().AsEquatableArray(),
            };
        }

        private static void PrintAdditionalUsings(ref Printer p)
        {
            p.PrintEndLine();
            p.Print("#pragma warning disable CS0105 // Using directive appeared previously in this namespace").PrintEndLine();
            p.PrintEndLine();
            p.PrintLine("using System;");
            p.PrintLine("using System.CodeDom.Compiler;");
            p.PrintLine("using System.Collections.Generic;");
            p.PrintLine("using System.Diagnostics.CodeAnalysis;");
            p.PrintLine("using System.Runtime.CompilerServices;");
            p.PrintLine("using EncosyTower.Mvvm.ComponentModel;");
            p.PrintLine("using EncosyTower.Mvvm.ComponentModel.SourceGen;");
            p.PrintLine("using EncosyTower.Variants;");
            p.PrintLine("using EncosyTower.Variants.Converters;");
            p.PrintEndLine();
            p.PrintLine("using EditorBrowsableAttribute = global::System.ComponentModel.EditorBrowsableAttribute;");
            p.PrintLine("using EditorBrowsableState = global::System.ComponentModel.EditorBrowsableState;");
            p.PrintLine("using INotifyPropertyChanging = global::EncosyTower.Mvvm.ComponentModel.INotifyPropertyChanging;");
            p.PrintLine("using INotifyPropertyChanged = global::EncosyTower.Mvvm.ComponentModel.INotifyPropertyChanged;");
            p.PrintLine("using PropertyChangeEventArgs = global::EncosyTower.Mvvm.ComponentModel.PropertyChangeEventArgs;");
            p.PrintLine("using PropertyChangingEventHandler = global::EncosyTower.Mvvm.ComponentModel.PropertyChangingEventHandler;");
            p.PrintLine("using PropertyChangedEventHandler = global::EncosyTower.Mvvm.ComponentModel.PropertyChangedEventHandler;");
            p.PrintEndLine();
            p.Print("#pragma warning restore CS0105 // Using directive appeared previously in this namespace").PrintEndLine();
            p.PrintEndLine();
        }

        /// <summary>
        /// Cache-friendly, equatable model for a single <c>[ObservableProperty]</c>-decorated field.
        /// All symbol-derived data is pre-computed as strings — no <see cref="ISymbol"/> or
        /// <see cref="SyntaxNode"/> references are retained.
        /// </summary>
        public struct FieldMemberDeclaration : IEquatable<FieldMemberDeclaration>
        {
            /// <summary>Excluded from <see cref="Equals(FieldMemberDeclaration)"/> and
            /// <see cref="GetHashCode"/> — not stable across incremental runs.</summary>
            public LocationInfo location;

            public string fieldName;
            public string propertyName;
            public string fieldTypeName;
            public string fieldTypeValidIdent;
            public bool isObservableObject;
            public bool hasSerializeFieldAttribute;
            public EquatableArray<string> commandNames;
            public EquatableArray<AttributeInfo> forwardedPropertyAttributes;

            public readonly bool Equals(FieldMemberDeclaration other)
                => string.Equals(fieldName, other.fieldName, StringComparison.Ordinal)
                && string.Equals(propertyName, other.propertyName, StringComparison.Ordinal)
                && string.Equals(fieldTypeName, other.fieldTypeName, StringComparison.Ordinal)
                && string.Equals(fieldTypeValidIdent, other.fieldTypeValidIdent, StringComparison.Ordinal)
                && isObservableObject == other.isObservableObject
                && hasSerializeFieldAttribute == other.hasSerializeFieldAttribute
                && commandNames.Equals(other.commandNames)
                && forwardedPropertyAttributes.Equals(other.forwardedPropertyAttributes);

            public readonly override bool Equals(object obj)
                => obj is FieldMemberDeclaration other && Equals(other);

            public readonly override int GetHashCode()
            {
                var hash = new HashValue();
                hash.Add(fieldName);
                hash.Add(propertyName);
                hash.Add(fieldTypeName);
                hash.Add(fieldTypeValidIdent);
                hash.Add(isObservableObject);
                hash.Add(hasSerializeFieldAttribute);
                hash.Add(commandNames);
                hash.Add(forwardedPropertyAttributes);
                return hash.ToHashCode();
            }
        }

        /// <summary>
        /// Cache-friendly, equatable model for a single <c>[ObservableProperty]</c>-decorated property.
        /// All symbol-derived data is pre-computed as strings — no <see cref="ISymbol"/> or
        /// <see cref="SyntaxNode"/> references are retained.
        /// </summary>
        public struct PropMemberDeclaration : IEquatable<PropMemberDeclaration>
        {
            /// <summary>Excluded from <see cref="Equals(PropMemberDeclaration)"/> and
            /// <see cref="GetHashCode"/> — not stable across incremental runs.</summary>
            public LocationInfo location;

            public string propertyName;
            public string fieldName;
            public string propertyTypeName;
            public string propertyTypeValidIdent;
            public bool isObservableObject;
            public bool doesCreateProperty;
            public EquatableArray<string> commandNames;
            public EquatableArray<ForwardedFieldAttribute> forwardedFieldAttributes;

            public readonly bool Equals(PropMemberDeclaration other)
                => string.Equals(propertyName, other.propertyName, StringComparison.Ordinal)
                && string.Equals(fieldName, other.fieldName, StringComparison.Ordinal)
                && string.Equals(propertyTypeName, other.propertyTypeName, StringComparison.Ordinal)
                && string.Equals(propertyTypeValidIdent, other.propertyTypeValidIdent, StringComparison.Ordinal)
                && isObservableObject == other.isObservableObject
                && doesCreateProperty == other.doesCreateProperty
                && commandNames.Equals(other.commandNames)
                && forwardedFieldAttributes.Equals(other.forwardedFieldAttributes);

            public readonly override bool Equals(object obj)
                => obj is PropMemberDeclaration other && Equals(other);

            public readonly override int GetHashCode()
            {
                var hash = new HashValue();
                hash.Add(propertyName);
                hash.Add(fieldName);
                hash.Add(propertyTypeName);
                hash.Add(propertyTypeValidIdent);
                hash.Add(isObservableObject);
                hash.Add(doesCreateProperty);
                hash.Add(commandNames);
                hash.Add(forwardedFieldAttributes);
                return hash.ToHashCode();
            }
        }

        /// <summary>
        /// Cache-friendly, equatable wrapper for a field-targeted forwarded attribute on a property member.
        /// Stores the attribute's fully-qualified type name alongside its <see cref="AttributeInfo"/> data.
        /// </summary>
        public struct ForwardedFieldAttribute : IEquatable<ForwardedFieldAttribute>
        {
            public string typeName;
            public AttributeInfo attributeInfo;

            public readonly bool Equals(ForwardedFieldAttribute other)
                => string.Equals(typeName, other.typeName, StringComparison.Ordinal)
                && (attributeInfo?.Equals(other.attributeInfo) ?? other.attributeInfo is null);

            public readonly override bool Equals(object obj)
                => obj is ForwardedFieldAttribute other && Equals(other);

            public readonly override int GetHashCode()
            {
                var hash = new HashValue();
                hash.Add(typeName);
                hash.Add(attributeInfo);
                return hash.ToHashCode();
            }
        }

        /// <summary>
        /// A single flattened entry of the <c>NotifyPropertyChangedFor</c> map.
        /// Records that when the observable member identified by <see cref="memberKey"/> changes,
        /// change notifications must also fire for the property identified by <see cref="propName"/>.
        /// </summary>
        public struct NotifyForEntry : IEquatable<NotifyForEntry>
        {
            /// <summary>The field name (for field members) or private-field name (for property members)
            /// used as the map key.</summary>
            public string memberKey;
            public string propName;
            public string propTypeName;
            public string propTypeValidIdent;

            public readonly bool Equals(NotifyForEntry other)
                => string.Equals(memberKey, other.memberKey, StringComparison.Ordinal)
                && string.Equals(propName, other.propName, StringComparison.Ordinal)
                && string.Equals(propTypeName, other.propTypeName, StringComparison.Ordinal)
                && string.Equals(propTypeValidIdent, other.propTypeValidIdent, StringComparison.Ordinal);

            public readonly override bool Equals(object obj)
                => obj is NotifyForEntry other && Equals(other);

            public readonly override int GetHashCode()
                => HashValue.Combine(memberKey, propName, propTypeName, propTypeValidIdent);
        }
    }
}
