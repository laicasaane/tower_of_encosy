using System;
using System.Threading;
using Microsoft.CodeAnalysis;

namespace EncosyTower.SourceGen.Generators.EnumTemplates
{
    /// <summary>
    /// Cache-friendly pipeline model representing a single type that contributes members
    /// to an enum template (the source-2 external member candidates flowing through the pipeline).
    /// Holds only primitive values and equatable collections — no <see cref="ISymbol"/>
    /// or <see cref="AttributeData"/> references.
    /// Replaces the non-cache-friendly <c>KindCandidate</c>.
    /// </summary>
    internal struct TemplateMemberCandidate : IEquatable<TemplateMemberCandidate>
    {
        /// <summary>
        /// Location of the attribute that declares this membership.
        /// Intentionally excluded from <see cref="Equals(TemplateMemberCandidate)"/> and
        /// <see cref="GetHashCode"/>: location data is not stable across incremental runs
        /// and must not drive cache invalidation.
        /// </summary>
        public LocationInfo attributeLocation;

        /// <summary>Fully qualified name of the type contributing members.</summary>
        public string typeFullName;

        /// <summary>Simple, valid C# identifier derived from the type name.</summary>
        public string typeSimpleName;

        /// <summary>Fully qualified name of the template struct this candidate targets.</summary>
        public string templateFullName;

        public string displayName;
        public string alternateName;
        public ulong order;
        public bool enumMembers;

        /// <summary>
        /// Special type of the enum's underlying numeric type.
        /// Only meaningful when <see cref="enumMembers"/> is <see langword="true"/>.
        /// </summary>
        public SpecialType underlyingType;

        /// <summary>
        /// Pre-extracted enum field entries.
        /// Only populated when <see cref="enumMembers"/> is <see langword="true"/>.
        /// </summary>
        public EquatableArray<TemplateMemberEntry> enumEntries;

        public readonly bool IsValid
            => string.IsNullOrEmpty(typeFullName) == false
            && string.IsNullOrEmpty(templateFullName) == false;

        /// <summary>
        /// Extracts a <see cref="TemplateMemberCandidate"/> from a resolved type symbol
        /// and its corresponding attribute data. Used for both inline members (attributes
        /// directly on the template struct, with <paramref name="templateFullName"/>
        /// passed from the template) and external members (attributes on the contributing
        /// type, with <paramref name="templateFullName"/> read from the attribute arg).
        /// </summary>
        public static TemplateMemberCandidate Extract(
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
            var enumEntries = default(EquatableArray<TemplateMemberEntry>);

            if (enumMembers)
            {
                underlyingType = typeSymbol.EnumUnderlyingType?.SpecialType ?? SpecialType.None;

                using var entryBuilder = ImmutableArrayBuilder<TemplateMemberEntry>.Rent();

                foreach (var member in typeSymbol.GetMembers())
                {
                    token.ThrowIfCancellationRequested();

                    if (member is not IFieldSymbol field || field.ConstantValue is null)
                    {
                        continue;
                    }

                    entryBuilder.Add(new TemplateMemberEntry {
                        name = field.Name,
                        displayName = GetDisplayName(field) ?? string.Empty,
                        value = Convert.ToUInt64(field.ConstantValue),
                        attributes = new EquatableArray<AttributeInfo>(field.GatherAttributes()),
                    });
                }

                enumEntries = new EquatableArray<TemplateMemberEntry>(entryBuilder.ToImmutable());
            }

            return new TemplateMemberCandidate {
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

        private static string GetDisplayName(IFieldSymbol field)
        {
            string displayName = null;

            foreach (var attribute in field.GetAttributes())
            {
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

        public readonly bool Equals(TemplateMemberCandidate other)
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
            => obj is TemplateMemberCandidate other && Equals(other);

        public readonly override int GetHashCode()
            => HashValue.Combine(typeFullName, typeSimpleName, templateFullName, displayName, alternateName)
            .Add(order)
            .Add(enumMembers)
            .Add(underlyingType)
            .Add(enumEntries.GetHashCode());
    }

    /// <summary>
    /// A single pre-extracted enum field entry stored inside <see cref="TemplateMemberCandidate"/>.
    /// Cache-friendly: holds only primitives and equatable collections.
    /// </summary>
    internal struct TemplateMemberEntry : IEquatable<TemplateMemberEntry>
    {
        public string name;
        public string displayName;
        public ulong value;
        public EquatableArray<AttributeInfo> attributes;

        public readonly bool Equals(TemplateMemberEntry other)
            => string.Equals(name, other.name, StringComparison.Ordinal)
            && string.Equals(displayName, other.displayName, StringComparison.Ordinal)
            && value == other.value
            && attributes.Equals(other.attributes)
            ;

        public readonly override bool Equals(object obj)
            => obj is TemplateMemberEntry other && Equals(other);

        public readonly override int GetHashCode()
            => HashValue.Combine(name, displayName, value)
            .Add(attributes.GetHashCode());
    }
}
