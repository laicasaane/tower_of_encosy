using System;

namespace EncosyTower.UserDataVaults
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public sealed class UserDataVaultAttribute : Attribute
    {
        public string Prefix { get; set; }

        public string Suffix { get; set; }
    }
}
