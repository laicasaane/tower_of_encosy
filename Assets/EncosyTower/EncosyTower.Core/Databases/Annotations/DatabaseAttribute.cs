using System;
using EncosyTower.Naming;

namespace EncosyTower.Databases
{
    /// <summary>
    /// Defines a database which utilizes BakingSheet mechanism to import data from sources.
    /// </summary>
    /// <remarks>
    /// By default, collections are automatically typed <see cref="Cathei.BakingSheet.VerticalList{T}"/>.
    /// <br/>
    /// In case this behavior is not desired, use <see cref="Authoring.HorizontalAttribute"/>
    /// to signify that the property should be a horizontal list.
    /// </remarks>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, Inherited = false)]
    public sealed class DatabaseAttribute : Attribute
    {
        public NamingStrategy NamingStrategy { get; }

        public Type[] Converters { get; }

        public string AssetName { get; set; }

        public DatabaseAttribute(params Type[] converters)
        {
            Converters = converters ?? Array.Empty<Type>();
        }

        public DatabaseAttribute(NamingStrategy namingStrategy, params Type[] converters)
        {
            NamingStrategy = namingStrategy;
            Converters = converters ?? Array.Empty<Type>();
        }
    }
}
