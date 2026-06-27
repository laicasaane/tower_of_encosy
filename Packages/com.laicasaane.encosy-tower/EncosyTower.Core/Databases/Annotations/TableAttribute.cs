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
        public NameCasing NameCasing { get; }

        /// <remarks>
        /// Converter precedences (smaller index means higher precedence):
        /// <list type="number">
        /// <item><see cref="EncosyTower.Databases.Authoring.ConverterForDataPropertyAttribute"/></item>
        /// <item><see cref="EncosyTower.Databases.Authoring.ConverterForTableAttribute"/></item>
        /// <item><see cref="EncosyTower.Databases.Authoring.AuthorDatabaseAttribute"/></item>
        /// <item><see cref="EncosyTower.Data.Authoring.DataAuthoringConverterAttribute"/></item>
        /// <item><see cref="EncosyTower.Databases.TableAttribute"/></item>
        /// <item><see cref="EncosyTower.Databases.DatabaseAttribute"/></item>
        /// <item>Local <c>Convert</c> method within IData type</item>
        /// </list>
        /// </remarks>
        public Type[] Converters { get; }

        public TableAttribute(params Type[] converters)
        {
            Converters = converters;
        }

        public TableAttribute(NameCasing nameCasing, params Type[] converters)
        {
            NameCasing = nameCasing;
            Converters = converters;
        }
    }
}
