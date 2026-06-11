using System;
using System.Threading;
using Microsoft.CodeAnalysis;

namespace EncosyTower.SourceGen.Generators.EnumTemplates
{
    internal struct TemplateMemberSpec : IEquatable<TemplateMemberSpec>
    {
        public LocationInfo attributeLocation;
        public string typeFullName;
        public string typeSimpleName;
        public string templateFullName;
        public string displayName;
        public string alternateName;
        public ulong order;
        public bool enumMembers;
        public SpecialType underlyingType;
        public EquatableArray<TemplateMemberEntrySpec> enumEntries;

        public readonly bool IsValid
            => string.IsNullOrEmpty(typeFullName) == false
            && string.IsNullOrEmpty(templateFullName) == false;

        public static TemplateMemberSpec Extract(
              INamedTypeSymbol typeSymbol
            , string templateFullName
            , AttributeData attrib
            , bool enumMembers
            , LocationInfo attributeLocation
            , CancellationToken token
        )
        {
            token.ThrowIfCancellationRequested();

            var args = attrib.ConstructorArguments;
            ulong order = 0;

            if (args.Length > 1)
            {
                var arg = args[1];

                if (arg.Kind == TypedConstantKind.Primitive && arg.Value is ulong v)
                {
                    order = v;
                }
            }

            var displayName = string.Empty;
            var alternateName = string.Empty;

            if (enumMembers == false)
            {
                if (args.Length > 2)
                {
                    var arg = args[2];

                    if (arg.Kind == TypedConstantKind.Primitive && arg.Value is string v)
                    {
                        displayName = v ?? string.Empty;
                    }
                }

                if (args.Length > 3)
                {
                    var arg = args[3];

                    if (arg.Kind == TypedConstantKind.Primitive && arg.Value is string v)
                    {
                        alternateName = v ?? string.Empty;
                    }
                }
            }

            var underlyingType = SpecialType.None;
            var enumEntries = default(EquatableArray<TemplateMemberEntrySpec>);

            if (enumMembers)
            {
                underlyingType = typeSymbol.EnumUnderlyingType?.SpecialType ?? SpecialType.None;

                using var entryBuilder = ImmutableArrayBuilder<TemplateMemberEntrySpec>.Rent();

                foreach (var member in typeSymbol.GetMembers())
                {
                    token.ThrowIfCancellationRequested();

                    if (member is not IFieldSymbol field || field.ConstantValue is null)
                    {
                        continue;
                    }

                    entryBuilder.Add(new TemplateMemberEntrySpec {
                        name = field.Name,
                        displayName = GetDisplayName(field, token) ?? string.Empty,
                        value = Convert.ToUInt64(field.ConstantValue),
                        attributes = new EquatableArray<AttributeInfo>(field.GatherAttributes(token)),
                    });
                }

                enumEntries = new EquatableArray<TemplateMemberEntrySpec>(entryBuilder.ToImmutable());
            }

            return new TemplateMemberSpec {
                attributeLocation = attributeLocation,
                typeFullName = typeSymbol.ToFullName(),
                typeSimpleName = typeSymbol.ToSimpleValidIdentifier(),
                templateFullName = templateFullName,
                displayName = displayName,
                alternateName = alternateName,
                order = order,
                enumMembers = enumMembers,
                underlyingType = underlyingType,
                enumEntries = enumEntries,
            };
        }

        private static string GetDisplayName(IFieldSymbol field, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            string displayName = null;

            foreach (var attribute in field.GetAttributes())
            {
                token.ThrowIfCancellationRequested();

                var attributeName = attribute.AttributeClass?.Name ?? string.Empty;

                switch (attributeName)
                {
                    case nameof(System.ObsoleteAttribute):
                    {
                        goto RETURN;
                    }

                    case "LabelAttribute":
                    case "DescriptionAttribute":
                    case "DisplayAttribute":
                    case "DisplayNameAttribute":
                    case "InspectorNameAttribute":
                    {
                        if (attribute.ConstructorArguments.Length > 0)
                        {
                            var arg = attribute.ConstructorArguments[0];

                            if (arg.Kind == TypedConstantKind.Primitive && arg.Value?.ToString() is string dn)
                            {
                                displayName = dn;
                                goto RETURN;
                            }
                        }
                        else if (attribute.NamedArguments.Length > 0)
                        {
                            foreach (var arg in attribute.NamedArguments)
                            {
                                token.ThrowIfCancellationRequested();

                                if (arg.Key is "Name" or "DisplayName"
                                    && arg.Value.Kind == TypedConstantKind.Primitive
                                    && arg.Value.Value?.ToString() is string dn
                                )
                                {
                                    displayName = dn;
                                    goto RETURN;
                                }
                            }
                        }

                        break;
                    }
                }
            }

            RETURN:
            return displayName;
        }

        public readonly bool Equals(TemplateMemberSpec other)
            => string.Equals(typeFullName, other.typeFullName, StringComparison.Ordinal)
            && string.Equals(typeSimpleName, other.typeSimpleName, StringComparison.Ordinal)
            && string.Equals(templateFullName, other.templateFullName, StringComparison.Ordinal)
            && string.Equals(displayName, other.displayName, StringComparison.Ordinal)
            && string.Equals(alternateName, other.alternateName, StringComparison.Ordinal)
            && order == other.order
            && enumMembers == other.enumMembers
            && underlyingType == other.underlyingType
            && enumEntries.Equals(other.enumEntries)
            ;

        public readonly override bool Equals(object obj)
            => obj is TemplateMemberSpec other && Equals(other);

        public readonly override int GetHashCode()
            => HashValue.Combine(typeFullName, typeSimpleName, templateFullName, displayName, alternateName)
            .Add(order)
            .Add(enumMembers)
            .Add(underlyingType)
            .Add(enumEntries.GetHashCode());
    }

    internal struct TemplateMemberEntrySpec : IEquatable<TemplateMemberEntrySpec>
    {
        public string name;
        public string displayName;
        public ulong value;
        public EquatableArray<AttributeInfo> attributes;

        public readonly bool Equals(TemplateMemberEntrySpec other)
            => string.Equals(name, other.name, StringComparison.Ordinal)
            && string.Equals(displayName, other.displayName, StringComparison.Ordinal)
            && value == other.value
            && attributes.Equals(other.attributes)
            ;

        public readonly override bool Equals(object obj)
            => obj is TemplateMemberEntrySpec other && Equals(other);

        public readonly override int GetHashCode()
            => HashValue.Combine(name, displayName, value)
            .Add(attributes.GetHashCode());
    }
}
