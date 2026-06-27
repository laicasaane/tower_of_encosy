using System;
using System.Diagnostics;
using EncosyTower.Core;

namespace EncosyTower.Data.Authoring
{
    /// <summary>
    /// Instructs the source generator to use a custom converter for the annotated property or field
    /// in the authoring pipeline via Baking Sheet.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <see cref="Type"/> must define a public <c>Convert</c> method that takes a single parameter
    /// and returns a value whose type matches the annotated member.
    /// </para>
    /// <para>
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
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// public struct IntToFloatConverter
    /// {
    ///     public float Convert(int value) => value;
    /// }
    ///
    /// [Data]
    /// public partial class MyData
    /// {
    ///     [DataProperty, DataAuthoringConverter(typeof(IntToFloatConverter))]
    ///     public float Hp { get; set; }
    ///
    ///     [JsonProperty, DataAuthoringConverter(typeof(IntToFloatConverter))]
    ///     private float _atk;
    /// }
    /// </code>
    /// </example>
    [ApiForAuthoring]
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    [Conditional("UNITY_EDITOR"), Conditional("ENCOSY_INCLUDE_AUTHORING")]
    public sealed class DataAuthoringConverterAttribute : Attribute
    {
        /// <inheritdoc cref="DataAuthoringConverterAttribute" />
        public DataAuthoringConverterAttribute(Type type)
        {
            Type = type;
        }

        public Type Type { get; }
    }
}
