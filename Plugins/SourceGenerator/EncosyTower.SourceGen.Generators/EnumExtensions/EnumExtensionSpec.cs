using System;
using System.Threading;
using Microsoft.CodeAnalysis;

namespace EncosyTower.SourceGen.Generators.EnumExtensions
{
    internal struct EnumExtensionSpec : IEquatable<EnumExtensionSpec>
    {
        private const string FLAGS_ATTRIBUTE = "global::System.FlagsAttribute";

        public LocationInfo location;
        public string openingSource;
        public string closingSource;
        public string enumName;
        public string extensionsName;
        public string structName;
        public string fullyQualifiedName;
        public string underlyingTypeName;
        public string namespaceName;
        public string fileHintName;
        public Accessibility accessibility;
        public bool parentIsNamespace;
        public bool hasFlags;
        public bool isDisplayAttributeUsed;
        public int fixedStringBytes;
        public EquatableArray<EnumMemberSpec> members;
        public EquatableArray<string> containingTypes;

        public readonly bool IsValid => location.IsValid;

        public static EnumExtensionSpec Extract(
              INamedTypeSymbol enumSymbol
            , bool parentIsNamespace
            , string extensionsName
            , Accessibility accessibility
            , LocationInfo location
            , string namespaceName
            , EquatableArray<string> containingTypes
            , CancellationToken token
        )
        {
            token.ThrowIfCancellationRequested();

            var enumName = enumSymbol.Name;
            var fixedStringBytes = 0;
            var isDisplayAttributeUsed = false;

            using var memberBuilder = ImmutableArrayBuilder<EnumMemberSpec>.Rent();

            foreach (var member in enumSymbol.GetMembers())
            {
                token.ThrowIfCancellationRequested();

                if (member is not IFieldSymbol field || field.ConstantValue is null)
                {
                    continue;
                }

                string displayName = null;
                var skip = false;

                foreach (var attribute in field.GetAttributes())
                {
                    token.ThrowIfCancellationRequested();

                    var attributeName = attribute.AttributeClass?.Name ?? string.Empty;

                    switch (attributeName)
                    {
                        case nameof(System.ObsoleteAttribute):
                        {
                            skip = true;
                            goto DONE_ATTRIBUTES;
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
                                    goto DONE_ATTRIBUTES;
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
                                        goto DONE_ATTRIBUTES;
                                    }
                                }
                            }

                            break;
                        }
                    }
                }

                DONE_ATTRIBUTES:

                if (skip)
                {
                    continue;
                }

                if (string.IsNullOrEmpty(displayName) == false && isDisplayAttributeUsed == false)
                {
                    isDisplayAttributeUsed = true;
                }

                var nameByteCount = member.Name.GetByteCount();
                var displayNameByteCount = displayName.GetByteCount();
                var byteCount = Math.Max(nameByteCount, displayNameByteCount);
                fixedStringBytes = Math.Max(fixedStringBytes, byteCount);

                memberBuilder.Add(new EnumMemberSpec {
                    name = member.Name,
                    displayName = displayName,
                });
            }

            return new EnumExtensionSpec {
                location = location,
                enumName = enumName,
                extensionsName = extensionsName,
                structName = EnumExtensionsDeclaration.GetNameExtendedStruct(enumName),
                fullyQualifiedName = enumSymbol.ToFullName(),
                underlyingTypeName = enumSymbol.EnumUnderlyingType.ToString(),
                namespaceName = namespaceName,
                fileHintName = enumSymbol.ToFileName(),
                accessibility = accessibility,
                parentIsNamespace = parentIsNamespace,
                hasFlags = enumSymbol.HasAttribute(FLAGS_ATTRIBUTE, token),
                isDisplayAttributeUsed = isDisplayAttributeUsed,
                fixedStringBytes = fixedStringBytes,
                members = new EquatableArray<EnumMemberSpec>(memberBuilder.ToImmutable()),
                containingTypes = containingTypes,
            };
        }

        public readonly bool Equals(EnumExtensionSpec other)
            => string.Equals(enumName, other.enumName, StringComparison.Ordinal)
            && string.Equals(extensionsName, other.extensionsName, StringComparison.Ordinal)
            && string.Equals(structName, other.structName, StringComparison.Ordinal)
            && string.Equals(fullyQualifiedName, other.fullyQualifiedName, StringComparison.Ordinal)
            && string.Equals(underlyingTypeName, other.underlyingTypeName, StringComparison.Ordinal)
            && string.Equals(namespaceName, other.namespaceName, StringComparison.Ordinal)
            && accessibility == other.accessibility
            && parentIsNamespace == other.parentIsNamespace
            && hasFlags == other.hasFlags
            && isDisplayAttributeUsed == other.isDisplayAttributeUsed
            && fixedStringBytes == other.fixedStringBytes
            && members.Equals(other.members)
            && containingTypes.Equals(other.containingTypes)
            ;

        public readonly override bool Equals(object obj)
            => obj is EnumExtensionSpec other && Equals(other);

        public readonly override int GetHashCode()
            => HashValue.Combine(enumName, extensionsName, structName, fullyQualifiedName, underlyingTypeName, namespaceName)
            .Add(accessibility)
            .Add(parentIsNamespace)
            .Add(hasFlags)
            .Add(isDisplayAttributeUsed)
            .Add(fixedStringBytes)
            .Add(members.GetHashCode())
            .Add(containingTypes.GetHashCode());
    }
}
