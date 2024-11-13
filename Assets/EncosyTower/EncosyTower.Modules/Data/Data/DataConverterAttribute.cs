using System;

namespace EncosyTower.Modules.Data
{
    /// <summary>
    /// Instructs the source generator to use a custom converter for the annotated property or field
    /// in the authoring mechanism.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <see cref="Type"/> must include a public <c>Convert</c> method
    /// that accepts a single parameter of any type
    /// and must return a value of the same type as the annotated member.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// public struct IntToFloatConverter
    /// {
    ///     public float Convert(int value) => value;
    /// }
    /// 
    /// public class MyData : IData
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
