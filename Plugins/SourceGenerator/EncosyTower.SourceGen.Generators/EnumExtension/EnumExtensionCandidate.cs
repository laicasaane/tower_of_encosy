using System;
using System.Threading;
using Microsoft.CodeAnalysis;

namespace EncosyTower.SourceGen.Generators.EnumExtensions
{
    /// <summary>
    /// Cache-friendly pipeline model for the enum-extensions source generators.
    /// Holds only primitive values and equatable collections — no <see cref="SyntaxNode"/>
    /// or <see cref="ISymbol"/> references — so that Roslyn's incremental generator engine
    /// can cache and compare instances cheaply across multiple compilations.
    /// Used by both <see cref="EnumExtensionsGenerator"/> and <see cref="EnumExtensionsForGenerator"/>.
    /// </summary>
    internal struct EnumExtensionCandidate : IEquatable<EnumExtensionCandidate>
    {
        private const string FLAGS_ATTRIBUTE = "global::System.FlagsAttribute";

        /// <summary>
        /// Location of the declaration in source. Intentionally excluded from
        /// <see cref="Equals(EnumExtensionCandidate)"/> and <see cref="GetHashCode"/>:
        /// location data is not stable across incremental runs and must not drive
        /// cache invalidation.
        /// </summary>
        public LocationInfo location;

        public string enumName;
        public string extensionsName;
        public string structName;
        public string fullyQualifiedName;
        public string underlyingTypeName;
        public string namespaceName;

        /// <summary>Hint name fragment, derived from <c>symbol.ToFileName()</c>.</summary>
        public string fileHintName;

        public Accessibility accessibility;
        public bool parentIsNamespace;
        public bool hasFlags;
        public bool isDisplayAttributeUsed;
        public int fixedStringBytes;
        public EquatableArray<EnumMemberDeclaration> members;

        /// <summary>
        /// Ordered chain of containing type declarations (outer → inner), each formatted as
        /// <c>&quot;&lt;accessibility&gt; partial &lt;keyword&gt; &lt;Name&gt;&quot;</c>.
        /// Empty when the enum/class is not nested inside another type.
        /// </summary>
        public EquatableArray<string> containingTypes;

        public readonly bool IsValid => location.IsValid;

        /// <summary>
        /// Extracts all enum metadata from <paramref name="enumSymbol"/> into a fully
        /// populated, cache-friendly <see cref="EnumExtensionCandidate"/>.
        /// Call this once per enum inside the incremental generator transform.
        /// </summary>
        public static EnumExtensionCandidate Extract(
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

            using var memberBuilder = ImmutableArrayBuilder<EnumMemberDeclaration>.Rent();

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

                memberBuilder.Add(new EnumMemberDeclaration {
                    name = member.Name,
                    displayName = displayName,
                });
            }

            return new EnumExtensionCandidate {
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
                hasFlags = enumSymbol.HasAttribute(FLAGS_ATTRIBUTE),
                isDisplayAttributeUsed = isDisplayAttributeUsed,
                fixedStringBytes = fixedStringBytes,
                members = new EquatableArray<EnumMemberDeclaration>(memberBuilder.ToImmutable()),
                containingTypes = containingTypes,
            };
        }

        public readonly bool Equals(EnumExtensionCandidate other)
            => string.Equals(enumName, other.enumName, StringComparison.Ordinal)
            && string.Equals(extensionsName, other.extensionsName, StringComparison.Ordinal)
            && string.Equals(structName, other.structName, StringComparison.Ordinal)
            && string.Equals(fullyQualifiedName, other.fullyQualifiedName, StringComparison.Ordinal)
            && string.Equals(underlyingTypeName, other.underlyingTypeName, StringComparison.Ordinal)
            && string.Equals(namespaceName, other.namespaceName, StringComparison.Ordinal)
            && string.Equals(fileHintName, other.fileHintName, StringComparison.Ordinal)
            && accessibility == other.accessibility
            && parentIsNamespace == other.parentIsNamespace
            && hasFlags == other.hasFlags
            && isDisplayAttributeUsed == other.isDisplayAttributeUsed
            && fixedStringBytes == other.fixedStringBytes
            && members.Equals(other.members)
            && containingTypes.Equals(other.containingTypes)
            ;

        public readonly override bool Equals(object obj)
            => obj is EnumExtensionCandidate other && Equals(other);

        public readonly override int GetHashCode()
            => HashValue.Combine(enumName, extensionsName, structName, fullyQualifiedName, underlyingTypeName, namespaceName, fileHintName)
            .Add(accessibility)
            .Add(parentIsNamespace)
            .Add(hasFlags)
            .Add(isDisplayAttributeUsed)
            .Add(fixedStringBytes)
            .Add(members.GetHashCode())
            .Add(containingTypes.GetHashCode());
    }
}
