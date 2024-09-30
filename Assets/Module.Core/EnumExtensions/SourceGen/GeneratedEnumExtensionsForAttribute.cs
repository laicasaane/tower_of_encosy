using System;

namespace Module.Core.EnumExtensions.SourceGen
{
    [AttributeUsage(AttributeTargets.Interface | AttributeTargets.Struct | AttributeTargets.Class, AllowMultiple = false)]
    public sealed class GeneratedEnumExtensionsForAttribute : Attribute
    {
        public Type EnumType { get; }

        public Type InterfaceType { get; }

        public Type ExtensionsType { get; }

        public Type WrapperType { get; }

        public GeneratedEnumExtensionsForAttribute(
              Type enumType
            , Type interfaceType
            , Type extensionsType
            , Type wrapperType
        )
        {
            EnumType = enumType;
            InterfaceType = interfaceType;
            ExtensionsType = extensionsType;
            WrapperType = wrapperType;
        }
    }
}
