using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace EncosyTower.SourceGen.Generators.EnumExtensions
{
    public partial class EnumExtensionsDeclaration
    {
        private const string FLAGS_ATTRIBUTE = "global::System.FlagsAttribute";

        public string Name { get; set; }

        public string ExtensionsName { get; set; }

        public string StructName { get; set; }

        public string FullyQualifiedName { get; set; }

        public string UnderlyingTypeName { get; set; }

        public string FixedStringTypeName { get; }

        public string PrintFixedStringTypeName { get; }

        public List<EnumMemberSpec> Members { get; set; }

        public Accessibility Accessibility { get; set; }

        public int FixedStringBytes { get; }

        public bool ParentIsNamespace { get; set; }

        public bool HasFlags { get; set; }

        public bool ReferenceUnityCollections { get; }

        public bool IsDisplayAttributeUsed { get; set; }

        public bool OnlyNames { get; set; }

        public bool NoDocumentation { get; set; }

        public bool OnlyClass { get; set; }

        public bool WithoutTryParse { get; set; }

        public bool WithoutIsDefined { get; set; }

        public string NamespaceName { get; set; }

        public EquatableArray<string> ContainingTypes { get; set; }

        internal EnumExtensionsDeclaration(
              in EnumExtensionSpec candidate
            , bool referencesUnityCollections
        )
        {
            ExtensionsName = candidate.extensionsName;
            StructName = candidate.structName;
            ParentIsNamespace = candidate.parentIsNamespace;
            Name = candidate.enumName;
            FullyQualifiedName = candidate.fullyQualifiedName;
            UnderlyingTypeName = candidate.underlyingTypeName;
            Accessibility = candidate.accessibility;
            HasFlags = candidate.hasFlags;
            ReferenceUnityCollections = referencesUnityCollections;
            IsDisplayAttributeUsed = candidate.isDisplayAttributeUsed;
            FixedStringBytes = candidate.fixedStringBytes;

            var members = Members = new List<EnumMemberSpec>(candidate.members.Count);

            foreach (var m in candidate.members)
            {
                members.Add(m);
            }

            ContainingTypes = candidate.containingTypes;
            NamespaceName = candidate.namespaceName;

            if (referencesUnityCollections)
            {
                FixedStringTypeName = GeneratorHelpers.GetFixedStringTypeName(candidate.fixedStringBytes);
                PrintFixedStringTypeName = $"UC.{FixedStringTypeName}";
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
                PrintFixedStringTypeName = $"UC.{FixedStringTypeName}";
            }
        }

        public static string GetNameExtensionsClass(string enumName)
        {
            return $"{enumName}Extensions";
        }

        public static string GetNameExtendedStruct(string enumName)
        {
            return $"{enumName}Extended";
        }
    }
}
