using System;
using System.Collections.Generic;
using EncosyTower.Modules.SourceGen;
using Microsoft.CodeAnalysis;

namespace EncosyTower.Modules.EnumExtensions.SourceGen
{
    public partial class EnumExtensionsDeclaration
    {
        private const string FLAGS_ATTRIBUTE = "global::System.FlagsAttribute";

        public string Name { get; set; }

        public string ExtensionsName { get; set; }

        public string ExtensionsWrapperName { get; set; }

        public bool ParentIsNamespace { get; set; }

        public string FullyQualifiedName { get; set; }

        public string UnderlyingTypeName { get; set; }

        public List<EnumMemberDeclaration> Members { get; set; }

        public Accessibility Accessibility { get; set; }

        public bool HasFlags { get; set; }

        public bool ReferenceUnityCollections { get; }

        public int FixedStringBytes { get; }

        public string FixedStringTypeName { get; }

        public string FixedStringTypeFullyQualifiedName { get; }

        public bool IsDisplayAttributeUsed { get; set; }

        public bool OnlyNames { get; set; }

        public bool NoDocumentation { get; set; }

        public bool OnlyClass { get; set; }

        public EnumExtensionsDeclaration(
              INamedTypeSymbol symbol
            , bool parentIsNamespace
            , string extensionsName
            , Accessibility accessibility
            , bool referencesUnityCollections
        )
        {
            ExtensionsName = extensionsName;
            ExtensionsWrapperName = $"{extensionsName}Wrapper";
            ParentIsNamespace = parentIsNamespace;
            Name = symbol.Name;
            FullyQualifiedName = symbol.ToFullName();
            UnderlyingTypeName = symbol.EnumUnderlyingType.ToString();
            Accessibility = accessibility;
            HasFlags = symbol.HasAttribute(FLAGS_ATTRIBUTE);
            ReferenceUnityCollections = referencesUnityCollections;

            var enumMembers = symbol.GetMembers();
            var members = Members = new List<EnumMemberDeclaration>(enumMembers.Length);
            var fixedStringBytes = 0;
            var isDisplayAttributeUsed = false;

            foreach (var member in enumMembers)
            {
                if (member is not IFieldSymbol field
                    || field.ConstantValue is null
                )
                {
                    continue;
                }

                string displayName = null;

                foreach (var attribute in field.GetAttributes())
                {
                    var attributeName = attribute.AttributeClass?.Name ?? string.Empty;

                    switch (attributeName)
                    {
                        case nameof(System.ObsoleteAttribute):
                        {
                            goto CONTINUE;
                        }

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
                                    goto ADD_DISPLAY_NAME;
                                }
                            }

                            if (attribute.NamedArguments.Length > 0)
                            {
                                foreach (var arg in attribute.NamedArguments)
                                {
                                    if (arg.Key == "Name"
                                        && arg.Value.Kind == TypedConstantKind.Primitive
                                        && arg.Value.Value?.ToString() is string dn
                                    )
                                    {
                                        displayName = dn;
                                        goto ADD_DISPLAY_NAME;
                                    }
                                }
                            }

                            break;
                        }
                    }
                }

                ADD_DISPLAY_NAME:
                {
                    if (displayName is not null && isDisplayAttributeUsed == false)
                    {
                        isDisplayAttributeUsed = true;
                    }

                    var nameByteCount = member.Name.GetByteCount();
                    var displayNameByteCount = displayName.GetByteCount();
                    var byteCount = Math.Max(nameByteCount, displayNameByteCount);
                    fixedStringBytes = Math.Max(fixedStringBytes, byteCount);

                    members.Add(new EnumMemberDeclaration {
                        name = member.Name,
                        displayName = displayName,
                    });
                    continue;
                }

                CONTINUE:
                {
                    continue;
                }
            }

            IsDisplayAttributeUsed = isDisplayAttributeUsed;
            FixedStringBytes = fixedStringBytes;

            if (ReferenceUnityCollections)
            {
                FixedStringTypeName = GeneratorHelpers.GetFixedStringTypeName(fixedStringBytes);
                FixedStringTypeFullyQualifiedName = $"global::Unity.Collections.{FixedStringTypeName}";
            }
        }

        public EnumExtensionsDeclaration(
              bool referencesUnityCollections
            , int fixedStringBytes
        )
        {
            ReferenceUnityCollections = referencesUnityCollections;
            FixedStringBytes = fixedStringBytes;

            if (referencesUnityCollections)
            {
                FixedStringTypeName = GeneratorHelpers.GetFixedStringTypeName(fixedStringBytes);
                FixedStringTypeFullyQualifiedName = $"global::Unity.Collections.{FixedStringTypeName}";
            }
        }
    }
}
