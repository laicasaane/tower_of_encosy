using System.Collections.Generic;

namespace EncosyTower.SourceGen.Generators.UserDataVaults
{
    internal partial class UserDataVaultDeclaration
    {
        public string ClassName { get; }

        public bool IsStatic { get; }

        public List<UserDataAccessorDeclaration> AccessorDefs { get; }

        public UserDataVaultDeclaration(
              string className
            , bool isStatic
            , List<UserDataAccessorDeclaration> accessorDefs
        )
        {
            ClassName = className;
            IsStatic = isStatic;
            AccessorDefs = accessorDefs;
        }
    }
}
