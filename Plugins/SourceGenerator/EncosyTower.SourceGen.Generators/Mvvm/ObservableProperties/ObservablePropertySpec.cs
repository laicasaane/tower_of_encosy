using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace EncosyTower.SourceGen.Generators.Mvvm.ObservableProperties
{
    public partial struct ObservablePropertySpec : IEquatable<ObservablePropertySpec>
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

        public LocationInfo location;

        public string className;
        public string hintName;
        public string openingSource;
        public string closingSource;
        public bool isBaseObservableObject;
        public bool isSealed;
        public bool hasMemberObservableObject;
        public bool hasSerializableAttribute;
        public bool hasGeneratePropertyBagAttribute;
        public EquatableArray<FieldMemberSpec> fieldRefs;
        public EquatableArray<PropMemberSpec> propRefs;
        public EquatableArray<NotifyForEntrySpec> notifyForEntries;
        public EquatableArray<string> notifyCanExecuteChangedFor;

        public readonly bool IsValid
            => string.IsNullOrEmpty(className) == false
            && string.IsNullOrEmpty(hintName) == false;

        public readonly bool Equals(ObservablePropertySpec other)
            => string.Equals(className, other.className, StringComparison.Ordinal)
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
            => obj is ObservablePropertySpec other && Equals(other);

        public readonly override int GetHashCode()
            => HashValue.Combine(
                  className
                , isBaseObservableObject
                , isSealed
                , hasMemberObservableObject
                , hasSerializableAttribute
                , hasGeneratePropertyBagAttribute
                , fieldRefs
            )
            .Add(propRefs)
            .Add(notifyForEntries)
            .Add(notifyCanExecuteChangedFor)
            ;

        public static ObservablePropertySpec Extract(
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
                    token.ThrowIfCancellationRequested();

                    classNameSb.Append(typeParams[i].Identifier.Text);

                    if (i < last)
                    {
                        classNameSb.Append(", ");
                    }
                }

                classNameSb.Append(">");
            }

            token.ThrowIfCancellationRequested();

            var className = classNameSb.ToString();
            var semanticModel = context.SemanticModel;
            var syntaxTree = classSyntax.SyntaxTree;
            var hintName = syntaxTree.GetHintName(classSyntax, classSymbol.ToFileName());

            TypeCreationHelpers.GenerateOpeningAndClosingSource(
                  classSyntax
                , token
                , out var openingSource
                , out var closingSource
                , printAdditionalUsings: PrintAdditionalUsings
            );

            var isBaseObservableObject = false;

            if (classSymbol.BaseType != null
                && classSymbol.BaseType.TypeKind == TypeKind.Class
                && classSymbol.BaseType.HasAttribute(OBSERVABLE_OBJECT_ATTRIBUTE_METADATA, token)
            )
            {
                isBaseObservableObject = true;
            }

            using var fieldRefsBuilder = ImmutableArrayBuilder<FieldMemberSpec>.Rent();
            using var propRefsBuilder = ImmutableArrayBuilder<PropMemberSpec>.Rent();

            var hasSerializableAttribute = classSymbol.HasAttribute(SERIALIZABLE_ATTRIBUTE, token);
            var hasGeneratePropertyBagAttribute = classSymbol.HasAttribute(GENERATE_PROPERTY_BAG_ATTRIBUTE, token);
            var hasMemberObservableObject = false;

            var observableFieldMap = new Dictionary<string, (
                  string typeName
                , bool hasSerializeField
                , List<string> commandNames
            )>();

            var observablePropMap = new Dictionary<string, (
                  string typeName
                , bool doesCreateProperty
                , List<string> commandNames
            )>();

            var propertyMap = new Dictionary<string, (
                  string propTypeName
                , string propTypeValidIdent
            )>();

            var observablePropNameSet = new HashSet<string>();
            var propChangedMapByPropName = new Dictionary<string, List<string>>();
            var propertyChangedMap = new Dictionary<string, List<string>>();
            var commandSet = new HashSet<string>();
            var members = classSymbol.GetMembers();
            var methods = new List<IMethodSymbol>();

            foreach (var member in members)
            {
                token.ThrowIfCancellationRequested();

                if (member is IFieldSymbol field)
                {
                    if (field.HasAttribute(OBSERVABLE_PROPERTY_ATTRIBUTE, token))
                    {
                        var fieldTypeName = field.Type.ToFullName();
                        var isObservableObject = field.Type.ImplementsInterface(IOBSERVABLE_OBJECT_INTERFACE, token: token);

                        if (isObservableObject)
                        {
                            hasMemberObservableObject = true;
                        }

                        var uniqueCommandNames = new HashSet<string>();
                        var notifyPropChangedFors = field.GetAttributes(NOTIFY_PROPERTY_CHANGED_FOR_ATTRIBUTE, token);

                        foreach (var notifyPropChangedFor in notifyPropChangedFors)
                        {
                            token.ThrowIfCancellationRequested();

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

                        token.ThrowIfCancellationRequested();

                        var notifyCanExecuteChangedFors = field.GetAttributes(NOTIFY_CAN_EXECUTE_CHANGED_FOR_ATTRIBUTE, token);

                        using var commandNames = ImmutableArrayBuilder<string>.Rent();

                        foreach (var notifyCanExecuteChangedFor in notifyCanExecuteChangedFors)
                        {
                            token.ThrowIfCancellationRequested();

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
                            , out ImmutableArray<AttributeInfo> propertyAttributes
                        );

                        fieldRefsBuilder.Add(new FieldMemberSpec {
                            location = LocationInfo.From(field.Locations.Length > 0 ? field.Locations[0] : Location.None),
                            fieldName = field.Name,
                            propertyName = field.ToPropertyName(),
                            fieldTypeName = fieldTypeName,
                            fieldTypeValidIdent = field.Type.ToValidIdentifier(),
                            isObservableObject = isObservableObject,
                            hasSerializeFieldAttribute = field.HasAttribute(SERIALIZE_FIELD_ATTRIBUTE, token),
                            commandNames = commandNames.ToImmutable().AsEquatableArray(),
                            forwardedPropertyAttributes = propertyAttributes.AsEquatableArray(),
                        });
                    }

                    continue;
                }

                if (member is IPropertySymbol property)
                {
                    if (property.HasAttribute(OBSERVABLE_PROPERTY_ATTRIBUTE, token) == false)
                    {
                        propertyMap[property.Name] = (property.Type.ToFullName(), property.Type.ToValidIdentifier());
                    }
                    else
                    {
                        var propertyTypeName = property.Type.ToFullName();
                        var fieldName = property.ToPrivateFieldName();
                        var isObservableObject = property.Type.ImplementsInterface(IOBSERVABLE_OBJECT_INTERFACE, token: token);

                        if (isObservableObject)
                        {
                            hasMemberObservableObject = true;
                        }

                        token.ThrowIfCancellationRequested();

                        var uniqueCommandNames = new HashSet<string>();
                        var notifyPropChangedFors = property.GetAttributes(NOTIFY_PROPERTY_CHANGED_FOR_ATTRIBUTE, token);

                        foreach (var notifyPropChangedFor in notifyPropChangedFors)
                        {
                            token.ThrowIfCancellationRequested();

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

                        var notifyCanExecuteChangedFors = property.GetAttributes(NOTIFY_CAN_EXECUTE_CHANGED_FOR_ATTRIBUTE, token);

                        using var commandNames = ImmutableArrayBuilder<string>.Rent();

                        foreach (var notifyCanExecuteChangedFor in notifyCanExecuteChangedFors)
                        {
                            token.ThrowIfCancellationRequested();

                            if (notifyCanExecuteChangedFor != null
                                && notifyCanExecuteChangedFor.ConstructorArguments.Length > 0
                            )
                            {
                                var args = notifyCanExecuteChangedFor.ConstructorArguments;

                                foreach (var arg in args)
                                {
                                    token.ThrowIfCancellationRequested();

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
                            , out ImmutableArray<(string, AttributeInfo)> fieldAttributes
                        );

                        using var forwardedFieldAttribsBuilder = ImmutableArrayBuilder<ForwardedFieldAttributeSpec>.Rent();


                        foreach (var (typeName, attribInfo) in fieldAttributes)
                        {
                            token.ThrowIfCancellationRequested();

                            forwardedFieldAttribsBuilder.Add(new ForwardedFieldAttributeSpec {
                                typeName = typeName,
                                attributeInfo = attribInfo,
                            });
                        }

                        propRefsBuilder.Add(new PropMemberSpec {
                            location = LocationInfo.From(property.Locations.Length > 0 ? property.Locations[0] : Location.None),
                            propertyName = property.Name,
                            fieldName = fieldName,
                            propertyTypeName = propertyTypeName,
                            propertyTypeValidIdent = property.Type.ToValidIdentifier(),
                            isObservableObject = isObservableObject,
                            doesCreateProperty = property.HasAttribute(CREATE_PROPERTY_ATTRIBUTE, token),
                            commandNames = commandNames.ToImmutable().AsEquatableArray(),
                            forwardedFieldAttributes = forwardedFieldAttribsBuilder.ToImmutable().AsEquatableArray(),
                        });
                    }

                    continue;
                }

                if (member is IMethodSymbol method && method.Parameters.Length <= 1)
                {
                    if (method.HasAttribute(RELAY_COMMAND_ATTRIBUTE, token))
                    {
                        methods.Add(method);
                    }

                    continue;
                }
            }

            token.ThrowIfCancellationRequested();

            using var notifyForBuilder = ImmutableArrayBuilder<NotifyForEntrySpec>.Rent();

            foreach (var kv in propertyChangedMap)
            {
                token.ThrowIfCancellationRequested();

                var memberKey = kv.Key;

                foreach (var propName in kv.Value)
                {
                    token.ThrowIfCancellationRequested();

                    if (propertyMap.TryGetValue(propName, out var propInfo) == false)
                    {
                        continue;
                    }

                    notifyForBuilder.Add(new NotifyForEntrySpec {
                        memberKey = memberKey,
                        propName = propName,
                        propTypeName = propInfo.propTypeName,
                        propTypeValidIdent = propInfo.propTypeValidIdent,
                    });
                }
            }

            token.ThrowIfCancellationRequested();

            using var notifyCanExecBuilder = ImmutableArrayBuilder<string>.Rent();

            foreach (var method in methods)
            {
                token.ThrowIfCancellationRequested();

                var commandName = $"{method.Name}Command";

                if (commandSet.Contains(commandName))
                {
                    notifyCanExecBuilder.Add(commandName);
                }
            }

            return new ObservablePropertySpec {
                location = LocationInfo.From(classSyntax.GetLocation()),
                className = className,
                hintName = hintName,
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
            p.PrintLine("using S = global::System;");
            p.PrintLine("using SCDC = global::System.CodeDom.Compiler;");
            p.PrintLine("using SCM = global::System.ComponentModel;");
            p.PrintLine("using SCG = global::System.Collections.Generic;");
            p.PrintLine("using SDCA = global::System.Diagnostics.CodeAnalysis;");
            p.PrintLine("using SRCS = global::System.Runtime.CompilerServices;");
            p.PrintLine("using ETMCM = global::EncosyTower.Mvvm.ComponentModel;");
            p.PrintLine("using ETMCMSG = global::EncosyTower.Mvvm.ComponentModel.SourceGen;");
            p.PrintLine("using ETV = global::EncosyTower.Variants;");
            p.PrintLine("using ETVC = global::EncosyTower.Variants.Converters;");
            p.PrintEndLine();
            p.Print("#pragma warning restore CS0105 // Using directive appeared previously in this namespace").PrintEndLine();
            p.PrintEndLine();
        }

        public struct FieldMemberSpec : IEquatable<FieldMemberSpec>
        {
            public LocationInfo location;

            public string fieldName;
            public string propertyName;
            public string fieldTypeName;
            public string fieldTypeValidIdent;
            public bool isObservableObject;
            public bool hasSerializeFieldAttribute;
            public EquatableArray<string> commandNames;
            public EquatableArray<AttributeInfo> forwardedPropertyAttributes;

            public readonly bool Equals(FieldMemberSpec other)
                => string.Equals(fieldName, other.fieldName, StringComparison.Ordinal)
                && string.Equals(propertyName, other.propertyName, StringComparison.Ordinal)
                && string.Equals(fieldTypeName, other.fieldTypeName, StringComparison.Ordinal)
                && string.Equals(fieldTypeValidIdent, other.fieldTypeValidIdent, StringComparison.Ordinal)
                && isObservableObject == other.isObservableObject
                && hasSerializeFieldAttribute == other.hasSerializeFieldAttribute
                && commandNames.Equals(other.commandNames)
                && forwardedPropertyAttributes.Equals(other.forwardedPropertyAttributes);

            public readonly override bool Equals(object obj)
                => obj is FieldMemberSpec other && Equals(other);

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

        public struct PropMemberSpec : IEquatable<PropMemberSpec>
        {
            public LocationInfo location;

            public string propertyName;
            public string fieldName;
            public string propertyTypeName;
            public string propertyTypeValidIdent;
            public bool isObservableObject;
            public bool doesCreateProperty;
            public EquatableArray<string> commandNames;
            public EquatableArray<ForwardedFieldAttributeSpec> forwardedFieldAttributes;

            public readonly bool Equals(PropMemberSpec other)
                => string.Equals(propertyName, other.propertyName, StringComparison.Ordinal)
                && string.Equals(fieldName, other.fieldName, StringComparison.Ordinal)
                && string.Equals(propertyTypeName, other.propertyTypeName, StringComparison.Ordinal)
                && string.Equals(propertyTypeValidIdent, other.propertyTypeValidIdent, StringComparison.Ordinal)
                && isObservableObject == other.isObservableObject
                && doesCreateProperty == other.doesCreateProperty
                && commandNames.Equals(other.commandNames)
                && forwardedFieldAttributes.Equals(other.forwardedFieldAttributes);

            public readonly override bool Equals(object obj)
                => obj is PropMemberSpec other && Equals(other);

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

        public struct ForwardedFieldAttributeSpec : IEquatable<ForwardedFieldAttributeSpec>
        {
            public string typeName;
            public AttributeInfo attributeInfo;

            public readonly bool Equals(ForwardedFieldAttributeSpec other)
                => string.Equals(typeName, other.typeName, StringComparison.Ordinal)
                && attributeInfo.Equals(other.attributeInfo);

            public readonly override bool Equals(object obj)
                => obj is ForwardedFieldAttributeSpec other && Equals(other);

            public readonly override int GetHashCode()
            {
                var hash = new HashValue();
                hash.Add(typeName);
                hash.Add(attributeInfo);
                return hash.ToHashCode();
            }
        }

        public struct NotifyForEntrySpec : IEquatable<NotifyForEntrySpec>
        {
            public string memberKey;
            public string propName;
            public string propTypeName;
            public string propTypeValidIdent;

            public readonly bool Equals(NotifyForEntrySpec other)
                => string.Equals(memberKey, other.memberKey, StringComparison.Ordinal)
                && string.Equals(propName, other.propName, StringComparison.Ordinal)
                && string.Equals(propTypeName, other.propTypeName, StringComparison.Ordinal)
                && string.Equals(propTypeValidIdent, other.propTypeValidIdent, StringComparison.Ordinal);

            public readonly override bool Equals(object obj)
                => obj is NotifyForEntrySpec other && Equals(other);

            public readonly override int GetHashCode()
                => HashValue.Combine(memberKey, propName, propTypeName, propTypeValidIdent);
        }
    }
}
