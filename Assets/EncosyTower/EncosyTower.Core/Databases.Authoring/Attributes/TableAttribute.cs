using System;
using EncosyTower.Naming;

namespace EncosyTower.Databases.Authoring
{
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class TableAttribute : Attribute
    {
        /// <summary>
        /// Determines the naming case for each sheet and its properties.
        /// </summary>
        public NamingStrategy NamingStrategy { get; }

        public Type[] Converters { get; }

        public TableAttribute(params Type[] converters)
        {
            Converters = converters;
        }

        public TableAttribute(NamingStrategy namingStrategy, params Type[] converters)
        {
            NamingStrategy = namingStrategy;
            Converters = converters;
        }
    }
}
