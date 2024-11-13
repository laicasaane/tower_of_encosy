using System;

namespace EncosyTower.Modules.Data
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, Inherited = false)]
    public sealed class DataFieldPolicyAttribute : Attribute
    {
        public DataFieldPolicy Policy { get; }

        public DataFieldPolicyAttribute(DataFieldPolicy policy)
        {
            this.Policy = policy;
        }
    }

    public enum DataFieldPolicy
    {
        Private = 0,
        Internal,
        Public,
    }
}
