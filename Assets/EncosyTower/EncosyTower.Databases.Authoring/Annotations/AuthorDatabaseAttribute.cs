using System;

namespace EncosyTower.Databases.Authoring
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, Inherited = false, AllowMultiple = false)]
    public sealed class AuthorDatabaseAttribute : Attribute
    {
        public Type DatabaseType { get; }

        public AuthorDatabaseAttribute(Type databaseType)
        {
            DatabaseType = databaseType;
        }
    }
}
