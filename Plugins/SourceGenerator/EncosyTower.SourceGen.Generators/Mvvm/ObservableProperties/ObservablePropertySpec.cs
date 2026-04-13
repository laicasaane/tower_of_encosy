using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;
using System.Threading;
using EncosyTower.SourceGen.Helpers.Mvvm;
using EncosyTower.SourceGen.TypeModeling.Models;
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
        public string sourceFilePath;
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
            => obj is ObservablePropertySpec other && Equals(other);

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

            using var fieldRefsBuilder = ImmutableArrayBuilder<FieldMemberSpec>.Rent();
            using var propRefsBuilder = ImmutableArrayBuilder<PropMemberSpec>.Rent();
            using var diagnosticBuilder = ImmutableArrayBuilder<DiagnosticInfo>.Rent();

            var typeModel = classSymbol.ToModel(
                  token
                , new ModelOptions(
                      ModelParts.Fields
                    | ModelParts.Properties
                    | ModelParts.Methods
                    | ModelParts.Attributes
                )
            );

            var typeModelAttrs = typeModel.Attributes;
            var typeModelAttrCount = typeModelAttrs.Count;
            var hasSerializableAttribute = false;
            var hasGeneratePropertyBagAttribute = false;

            for (var i = 0; i < typeModelAttrCount; i++)
            {
                var attrFullName = typeModelAttrs[i].FullName;

                if (string.Equals(attrFullName, SERIALIZABLE_ATTRIBUTE, StringComparison.Ordinal))
                {
                    hasSerializableAttribute = true;
                }

                if (string.Equals(attrFullName, GENERATE_PROPERTY_BAG_ATTRIBUTE, StringComparison.Ordinal))
                {
                    hasGeneratePropertyBagAttribute = true;
                }
            }

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
            var observablePropNameSet = new HashSet<string>();
            var propChangedMapByPropName = new Dictionary<string, List<string>>();
            var propertyChangedMap = new Dictionary<string, List<string>>();
            var commandSet = new HashSet<string>();
            var propertyMap = new Dictionary<string, (string propTypeName, string propTypeValidIdent)>();
            var methodNames = new List<string>();
            var typeFields = typeModel.Fields;
            var typeFieldCount = typeFields.Count;

            for (var i = 0; i < typeFieldCount; i++)
            {
                token.ThrowIfCancellationRequested();

                var fieldModel = typeFields[i];
                var fieldAttrs = fieldModel.Attributes;
                var fieldAttrCount = fieldAttrs.Count;
                var hasObservablePropAttr = false;
                var hasSerializeFieldAttr = false;

                for (var j = 0; j < fieldAttrCount; j++)
                {
                    var attrFullName = fieldAttrs[j].FullName;

                    if (string.Equals(attrFullName, OBSERVABLE_PROPERTY_ATTRIBUTE, StringComparison.Ordinal))
                    {
                        hasObservablePropAttr = true;
                    }
                    else if (string.Equals(attrFullName, SERIALIZE_FIELD_ATTRIBUTE, StringComparison.Ordinal))
                    {
                        hasSerializeFieldAttr = true;
                    }
                }

                if (hasObservablePropAttr == false)
                {
                    continue;
                }

                var fieldName = fieldModel.Name;
                var fieldCommandNames = new List<string>();
                var uniqueFieldCommandNames = new HashSet<string>();

                for (var j = 0; j < fieldAttrCount; j++)
                {
                    var attr = fieldAttrs[j];
                    var attrFullName = attr.FullName;

                    if (string.Equals(attrFullName, NOTIFY_PROPERTY_CHANGED_FOR_ATTRIBUTE, StringComparison.Ordinal))
                    {
                        var ctorArgs = attr.ConstructorArguments;

                        if (ctorArgs.Count > 0
                            && ctorArgs[0].Kind == TypedConstantKind.Primitive
                            && string.IsNullOrEmpty(ctorArgs[0].Value) == false
                        )
                        {
                            if (propertyChangedMap.TryGetValue(fieldName, out var propNames) == false)
                            {
                                propertyChangedMap[fieldName] = propNames = new List<string>();
                            }

                            propNames.Add(ctorArgs[0].Value);
                        }
                    }
                    else if (string.Equals(
                        attrFullName, NOTIFY_CAN_EXECUTE_CHANGED_FOR_ATTRIBUTE, StringComparison.Ordinal
                    ))
                    {
                        var ctorArgs = attr.ConstructorArguments;
                        var ctorArgCount = ctorArgs.Count;

                        for (var k = 0; k < ctorArgCount; k++)
                        {
                            var arg = ctorArgs[k];

                            if (arg.Kind == TypedConstantKind.Primitive && string.IsNullOrEmpty(arg.Value) == false)
                            {
                                uniqueFieldCommandNames.Add(arg.Value);
                                commandSet.Add(arg.Value);
                            }
                        }
                    }
                }

                fieldCommandNames.AddRange(uniqueFieldCommandNames);
                observableFieldMap[fieldName] = (fieldModel.TypeFullName, hasSerializeFieldAttr, fieldCommandNames);
            }

            var typeProperties = typeModel.Properties;
            var typePropertyCount = typeProperties.Count;

            for (var i = 0; i < typePropertyCount; i++)
            {
                token.ThrowIfCancellationRequested();

                var propModel = typeProperties[i];
                var propAttrs = propModel.Attributes;
                var propAttrCount = propAttrs.Count;
                var hasObservablePropAttr = false;
                var hasCreatePropAttr = false;

                for (var j = 0; j < propAttrCount; j++)
                {
                    var attrFullName = propAttrs[j].FullName;

                    if (string.Equals(attrFullName, OBSERVABLE_PROPERTY_ATTRIBUTE, StringComparison.Ordinal))
                    {
                        hasObservablePropAttr = true;
                    }
                    else if (string.Equals(attrFullName, CREATE_PROPERTY_ATTRIBUTE, StringComparison.Ordinal))
                    {
                        hasCreatePropAttr = true;
                    }
                }

                if (hasObservablePropAttr == false)
                {
                    continue;
                }

                var propName = propModel.Name;
                var propCommandNames = new List<string>();
                var uniquePropCommandNames = new HashSet<string>();

                for (var j = 0; j < propAttrCount; j++)
                {
                    var attr = propAttrs[j];
                    var attrFullName = attr.FullName;

                    if (string.Equals(attrFullName, NOTIFY_PROPERTY_CHANGED_FOR_ATTRIBUTE, StringComparison.Ordinal))
                    {
                        var ctorArgs = attr.ConstructorArguments;

                        if (ctorArgs.Count > 0
                            && ctorArgs[0].Kind == TypedConstantKind.Primitive
                            && string.IsNullOrEmpty(ctorArgs[0].Value) == false
                        )
                        {
                            if (propChangedMapByPropName.TryGetValue(propName, out var propNames) == false)
                            {
                                propChangedMapByPropName[propName] = propNames = new List<string>();
                            }

                            propNames.Add(ctorArgs[0].Value);
                        }
                    }
                    else if (string.Equals(
                        attrFullName, NOTIFY_CAN_EXECUTE_CHANGED_FOR_ATTRIBUTE, StringComparison.Ordinal
                    ))
                    {
                        var ctorArgs = attr.ConstructorArguments;
                        var ctorArgCount = ctorArgs.Count;

                        for (var k = 0; k < ctorArgCount; k++)
                        {
                            var arg = ctorArgs[k];

                            if (arg.Kind == TypedConstantKind.Primitive && string.IsNullOrEmpty(arg.Value) == false)
                            {
                                uniquePropCommandNames.Add(arg.Value);
                                commandSet.Add(arg.Value);
                            }
                        }
                    }
                }

                propCommandNames.AddRange(uniquePropCommandNames);
                observablePropNameSet.Add(propName);
                observablePropMap[propName] = (propModel.TypeFullName, hasCreatePropAttr, propCommandNames);
            }

            var typeMethods = typeModel.Methods;
            var typeMethodCount = typeMethods.Count;

            for (var i = 0; i < typeMethodCount; i++)
            {
                token.ThrowIfCancellationRequested();

                var methodModel = typeMethods[i];

                if (methodModel.Parameters.Count > 1)
                {
                    continue;
                }

                var methodAttrs = methodModel.Attributes;
                var methodAttrCount = methodAttrs.Count;

                for (var j = 0; j < methodAttrCount; j++)
                {
                    if (string.Equals(methodAttrs[j].FullName, RELAY_COMMAND_ATTRIBUTE, StringComparison.Ordinal))
                    {
                        methodNames.Add(methodModel.Name);
                        break;
                    }
                }
            }

            var allMembers = classSymbol.GetMembers();
            var allMemberCount = allMembers.Length;

            for (var i = 0; i < allMemberCount; i++)
            {
                token.ThrowIfCancellationRequested();

                var member = allMembers[i];

                if (member is IFieldSymbol field)
                {
                    if (observableFieldMap.TryGetValue(field.Name, out var fieldData) == false)
                    {
                        continue;
                    }

                    var isObservableObject = field.Type.ImplementsInterface(IOBSERVABLE_OBJECT_INTERFACE);

                    if (isObservableObject)
                    {
                        hasMemberObservableObject = true;
                    }

                    field.GatherForwardedAttributes(
                          semanticModel
                        , token
                        , diagnosticBuilder
                        , out ImmutableArray<AttributeInfo> propertyAttributes
                        , DiagnosticDescriptors.InvalidPropertyTargetedAttributeOnObservableProperty
                    );

                    fieldRefsBuilder.Add(new FieldMemberSpec {
                        location = LocationInfo.From(field.Locations.Length > 0 ? field.Locations[0] : Location.None),
                        fieldName = field.Name,
                        propertyName = field.ToPropertyName(),
                        fieldTypeName = fieldData.typeName,
                        fieldTypeValidIdent = field.Type.ToValidIdentifier(),
                        isObservableObject = isObservableObject,
                        hasSerializeFieldAttribute = fieldData.hasSerializeField,
                        commandNames = fieldData.commandNames.ToImmutableArray().AsEquatableArray(),
                        forwardedPropertyAttributes = propertyAttributes.AsEquatableArray(),
                    });

                    continue;
                }

                if (member is IPropertySymbol property)
                {
                    if (observablePropNameSet.Contains(property.Name) == false)
                    {
                        propertyMap[property.Name] = (property.Type.ToFullName(), property.Type.ToValidIdentifier());
                        continue;
                    }

                    var propData = observablePropMap[property.Name];
                    var fieldName = property.ToPrivateFieldName();
                    var isObservableObject = property.Type.ImplementsInterface(IOBSERVABLE_OBJECT_INTERFACE);

                    if (isObservableObject)
                    {
                        hasMemberObservableObject = true;
                    }

                    if (propChangedMapByPropName.TryGetValue(property.Name, out var propChangedNames))
                    {
                        propertyChangedMap[fieldName] = propChangedNames;
                    }

                    property.GatherForwardedAttributes(
                          semanticModel
                        , token
                        , diagnosticBuilder
                        , out ImmutableArray<(string, AttributeInfo)> fieldAttributes
                        , DiagnosticDescriptors.InvalidFieldMethodTargetedAttributeOnObservableProperty
                    );

                    using var forwardedFieldAttribsBuilder = ImmutableArrayBuilder<ForwardedFieldAttributeSpec>.Rent();
                    var fieldAttrCount = fieldAttributes.Length;

                    for (var j = 0; j < fieldAttrCount; j++)
                    {
                        var (typeName, attribInfo) = fieldAttributes[j];
                        forwardedFieldAttribsBuilder.Add(new ForwardedFieldAttributeSpec { typeName = typeName, attributeInfo = attribInfo });
                    }

                    propRefsBuilder.Add(new PropMemberSpec {
                        location = LocationInfo.From(property.Locations.Length > 0 ? property.Locations[0] : Location.None),
                        propertyName = property.Name,
                        fieldName = fieldName,
                        propertyTypeName = propData.typeName,
                        propertyTypeValidIdent = property.Type.ToValidIdentifier(),
                        isObservableObject = isObservableObject,
                        doesCreateProperty = propData.doesCreateProperty,
                        commandNames = propData.commandNames.ToImmutableArray().AsEquatableArray(),
                        forwardedFieldAttributes = forwardedFieldAttribsBuilder.ToImmutable().AsEquatableArray(),
                    });

                    continue;
                }
            }

            using var notifyForBuilder = ImmutableArrayBuilder<NotifyForEntrySpec>.Rent();

            foreach (var kv in propertyChangedMap)
            {
                var memberKey = kv.Key;

                foreach (var propName in kv.Value)
                {
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

            using var notifyCanExecBuilder = ImmutableArrayBuilder<string>.Rent();

            var methodNameCount = methodNames.Count;

            for (var i = 0; i < methodNameCount; i++)
            {
                var commandName = $"{methodNames[i]}Command";

                if (commandSet.Contains(commandName))
                {
                    notifyCanExecBuilder.Add(commandName);
                }
            }

            return new ObservablePropertySpec {
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
                && (attributeInfo?.Equals(other.attributeInfo) ?? other.attributeInfo is null);

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
