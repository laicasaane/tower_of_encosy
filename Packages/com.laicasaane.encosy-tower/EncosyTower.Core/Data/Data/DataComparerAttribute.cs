using System;

namespace EncosyTower.Data
{
    /// <summary>
    /// Instructs the source generator to use a custom comparer for the annotated property or field
    /// in the authoring mechanism.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <see cref="Type"/> must include a public <c>Equals</c> method that accepts 2 parameters of the same type
    /// and must return a boolean.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// public struct IntArrayComparer
    /// {
    ///     public static bool Equals(int[] a, int[] b) => a.AsSpan().SequenceEqual(b.AsSpan());
    /// }
    ///
    /// public class MyData : IData
    /// {
    ///     [DataProperty, DataComparer(typeof(IntArrayComparer))]
    ///     public int[] Values { get; set; }
    /// }
    /// </code>
    /// </example>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public sealed class DataComparerAttribute : Attribute
    {
        public Type Type { get; }

        public DataComparerAttribute(Type type)
        {
            this.Type = type;
        }
    }
}
