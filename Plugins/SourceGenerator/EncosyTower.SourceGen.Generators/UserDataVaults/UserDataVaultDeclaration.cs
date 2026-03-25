using System.Collections.Generic;

namespace EncosyTower.SourceGen.Generators.UserDataVaults
{
    internal partial class UserDataVaultDeclaration
    {
        public string ClassName { get; }

        public bool IsStatic { get; }

        public List<UserDataAccessorDefinition> AccessorDefs { get; }

        public UserDataVaultDeclaration(
              string className
            , bool isStatic
            , List<UserDataAccessorDefinition> accessorDefs
        )
        {
            ClassName = className;
            IsStatic = isStatic;
            AccessorDefs = accessorDefs;
        }
    }
}
