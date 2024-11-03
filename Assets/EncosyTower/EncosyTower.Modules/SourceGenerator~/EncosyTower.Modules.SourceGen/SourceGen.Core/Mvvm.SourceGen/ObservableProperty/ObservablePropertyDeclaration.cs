using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;
using System.Threading;
using EncosyTower.Modules.SourceGen;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace EncosyTower.Modules.Mvvm.ObservablePropertySourceGen
{
    public partial class ObservablePropertyDeclaration
    {
        public const string IOBSERVABLE_OBJECT_INTERFACE = "global::EncosyTower.Modules.Mvvm.ComponentModel.IObservableObject";
        public const string OBSERVABLE_PROPERTY_ATTRIBUTE = "global::EncosyTower.Modules.Mvvm.ComponentModel.ObservablePropertyAttribute";
        public const string NOTIFY_PROPERTY_CHANGED_FOR_ATTRIBUTE = "global::EncosyTower.Modules.Mvvm.ComponentModel.NotifyPropertyChangedForAttribute";
        public const string NOTIFY_CAN_EXECUTE_CHANGED_FOR_ATTRIBUTE = "global::EncosyTower.Modules.Mvvm.ComponentModel.NotifyCanExecuteChangedForAttribute";
        public const string RELAY_COMMAND_ATTRIBUTE = "global::EncosyTower.Modules.Mvvm.Input.RelayCommandAttribute";

        public ClassDeclarationSyntax Syntax { get; }

        public INamedTypeSymbol Symbol { get; }

        public string ClassName { get; }

        public string FullyQualifiedName { get; private set; }

        public bool IsBaseObservableObject { get; }

        public ImmutableArray<FieldRef> FieldRefs { get; }

        public ImmutableArray<PropertyRef> PropRefs { get; }

        public bool HasMemberObservableObject { get; }

        /// <summary>
        /// Key is <c>Field.Name</c>
        /// </summary>
        public Dictionary<string, List<IPropertySymbol>> NotifyPropertyChangedForMap { get; }

        /// <summary>
        /// Key is the command name (<c>Method.Name + "Command"</c>)
        /// </summary>
        public HashSet<string> NotifyCanExecuteChangedForSet { get; }

        public ObservablePropertyDeclaration(
              ClassDeclarationSyntax candidate
            , SemanticModel semanticModel
            , CancellationToken token
        )
        {
            using var fieldRefs = ImmutableArrayBuilder<FieldRef>.Rent();
            using var propRefs = ImmutableArrayBuilder<PropertyRef>.Rent();
            using var diagnosticBuilder = ImmutableArrayBuilder<DiagnosticInfo>.Rent();

            Syntax = candidate;
            Symbol = semanticModel.GetDeclaredSymbol(candidate, token);

            var classNameSb = new StringBuilder(Syntax.Identifier.Text);

            if (candidate.TypeParameterList is TypeParameterListSyntax typeParamList
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

            ClassName = classNameSb.ToString();
            FullyQualifiedName = Symbol.ToFullName();
            NotifyPropertyChangedForMap = new Dictionary<string, List<IPropertySymbol>>();
            NotifyCanExecuteChangedForSet = new HashSet<string>();

            if (Symbol.BaseType != null && Symbol.BaseType.TypeKind == TypeKind.Class)
            {
                if (Symbol.BaseType.ImplementsInterface(IOBSERVABLE_OBJECT_INTERFACE))
                {
                    IsBaseObservableObject = true;
                }
            }

            var members = Symbol.GetMembers();
            var propertyChangedMap = new Dictionary<string, List<string>>();
            var commandSet = new HashSet<string>();
            var propertyMap = new Dictionary<string, IPropertySymbol>();
            var methods = new List<IMethodSymbol>();

            foreach (var member in members)
            {
                if (member is IFieldSymbol field)
                {
                    if (field.HasAttribute(OBSERVABLE_PROPERTY_ATTRIBUTE))
                    {
                        var fieldRef = new FieldRef {
                            Field = field,
                            PropertyName = field.ToPropertyName(),
                            IsObservableObject = field.Type.ImplementsInterface(IOBSERVABLE_OBJECT_INTERFACE),
                        };

                        if (fieldRef.IsObservableObject)
                        {
                            HasMemberObservableObject = true;
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

                        fieldRef.Field.GatherForwardedAttributes(
                              semanticModel
                            , token
                            , diagnosticBuilder
                            , out var propertyAttributes
                            , DiagnosticDescriptors.InvalidPropertyTargetedAttributeOnObservableProperty
                        );

                        fieldRef.ForwardedPropertyAttributes = propertyAttributes;
                        fieldRef.CommandNames = commandNames.ToImmutable();
                        fieldRefs.Add(fieldRef);
                    }

                    continue;
                }

                if (member is IPropertySymbol property)
                {
                    if (property.HasAttribute(OBSERVABLE_PROPERTY_ATTRIBUTE) == false)
                    {
                        propertyMap[property.Name] = property;
                    }
                    else
                    {
                        var fieldName = property.ToFieldName();
                        var propRef = new PropertyRef {
                            Property = property,
                            FieldName = fieldName,
                            IsObservableObject = property.Type.ImplementsInterface(IOBSERVABLE_OBJECT_INTERFACE),
                        };

                        if (propRef.IsObservableObject)
                        {
                            HasMemberObservableObject = true;
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

                        propRef.Property.GatherForwardedAttributes(
                              semanticModel
                            , token
                            , diagnosticBuilder
                            , out ImmutableArray<AttributeInfo> fieldAttributes
                            , DiagnosticDescriptors.InvalidFieldMethodTargetedAttributeOnObservableProperty
                        );

                        propRef.ForwardedFieldAttributes = fieldAttributes;
                        propRef.CommandNames = commandNames.ToImmutable();
                        propRefs.Add(propRef);
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

            foreach (var kv in propertyChangedMap)
            {
                var fieldName = kv.Key;
                var propNames = kv.Value;

                foreach (var propName in propNames)
                {
                    if (propertyMap.TryGetValue(propName, out var property) == false)
                    {
                        continue;
                    }

                    if (NotifyPropertyChangedForMap.TryGetValue(fieldName, out var properties) == false)
                    {
                        NotifyPropertyChangedForMap[fieldName] = properties = new List<IPropertySymbol>();
                    }

                    properties.Add(property);
                }
            }

            foreach (var method in methods)
            {
                var commandName = $"{method.Name}Command";

                if (commandSet.Contains(commandName))
                {
                    NotifyCanExecuteChangedForSet.Add(commandName);
                }
            }

            FieldRefs = fieldRefs.ToImmutable();
            PropRefs = propRefs.ToImmutable();
        }

        public abstract class MemberRef
        {
            public bool IsObservableObject { get; set; }

            public ImmutableArray<string> CommandNames { get; set; }

            public abstract string GetPropertyName();
        }

        public class FieldRef : MemberRef
        {
            public IFieldSymbol Field { get; set; }

            public string PropertyName { get; set; }

            public ImmutableArray<AttributeInfo> ForwardedPropertyAttributes { get; set; }

            public override string GetPropertyName()
                => PropertyName;
        }

        public class PropertyRef : MemberRef
        {
            public IPropertySymbol Property { get; set; }

            public string FieldName { get; set; }

            public ImmutableArray<AttributeInfo> ForwardedFieldAttributes { get; set; }

            public override string GetPropertyName()
                => Property.Name;
        }
    }
}
