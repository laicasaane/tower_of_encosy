using System;

namespace EncosyTower.Data
{
    /// <summary>
    /// Instructs the source generator to use a custom converter for the annotated property or field
    /// in the authoring pipeline.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <see cref="Type"/> must define a public <c>Convert</c> method that takes a single parameter
    /// and returns a value whose type matches the annotated member.
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
    ///     [DataProperty, DataConverter(typeof(IntToFloatConverter))]
    ///     public float Hp { get; set; }
    ///
    ///     [JsonProperty, DataConverter(typeof(IntToFloatConverter))]
    ///     private float _atk;
    /// }
    /// </code>
    /// </example>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public sealed class DataConverterAttribute : Attribute
    {
        public Type Type { get; }

        public DataConverterAttribute(Type type)
        {
            this.Type = type;
        }
    }
}
