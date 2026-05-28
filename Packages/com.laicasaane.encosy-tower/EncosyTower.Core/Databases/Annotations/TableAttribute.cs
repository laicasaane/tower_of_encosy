using System;
using EncosyTower.Naming;

namespace EncosyTower.Databases
{
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class TableAttribute : Attribute
    {
        /// <summary>
        /// Determines the naming case for each sheet and its properties.
        /// </summary>
        public NamingStrategy NamingStrategy { get; }

        /// <remarks>
        /// Converter precedences (smaller index means higher precedence):
        /// <list type="number">
        /// <item><see cref="EncosyTower.Data.Authoring.DataConverterAttribute"/></item>
        /// <item><see cref="EncosyTower.Databases.Authoring.AuthorDatabaseAttribute"/></item>
        /// <item><see cref="EncosyTower.Databases.DatabaseAttribute"/></item>
        /// <item><see cref="EncosyTower.Databases.TableAttribute"/></item>
        /// <item>Local <c>Convert</c> method within IData type</item>
        /// </list>
        /// </remarks>
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
