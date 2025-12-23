using System;
using System.Diagnostics.CodeAnalysis;

namespace EncosyTower.PolyEnumStructs
{
    /// <summary>
    /// Annotates any struct to generate functionality related to Polymorphic Enum Structs.
    /// </summary>
    /// <remarks>
    /// <list type="bullet">
    /// <item>The idea of Polymorphic Enum Struct is based on both 'discriminated union' and 'interface polymorphism'.</item>
    /// <item>Any nested struct will be automatically recognized as a case for the outer struct.</item>
    /// <item>In polymorphism terms, the outer struct acts as the base type, where nested structs act as derived types.</item>
    /// <item>
    /// A common interface named <c>IEnumCase</c> will be generated into the annotated outer struct.
    /// Its members are aggregated from the identical members of all nested structs:
    /// They must have the same name, same return type, same parameters.
    /// </item>
    /// <item>Both the outer and the nested structs will automatically derive from <c>IEnumCase</c>.</item>
    /// <item>Members from <c>IEnumCase</c> will be automatically implemented for the outer struct.</item>
    /// <item>Additional facilities to convert the outer struct to a nested struct, and vice versa will be generated.</item>
    /// <item>Additional facilities to help the polymorphism usage will be generated.</item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <code>
    /// [PolyEnumStruct]
    /// public partial struct Task
    /// {
    ///     public partial struct Shopping
    ///     {
    ///         public void Execute() { }
    ///     }
    ///
    ///     public partial struct Working
    ///     {
    ///         public void Execute() { }
    ///     }
    ///
    ///     // The interface 'IEnumCase' is generated.
    ///     // The enum type 'EnumCase' is generated.
    ///     // Additional facilities for 'discriminated union' and 'polymorphism' are generated.
    ///     // User can now assign a 'Shopping' or 'Working' value to a 'Task' variable, and vice versa.
    ///     // User can now invoke the method 'Task.Execute()' polymorhically.
    /// }
    /// </code>
    /// </example>
    [AttributeUsage(AttributeTargets.Struct, AllowMultiple = false)]
    public sealed class PolyEnumStructAttribute : Attribute
    {
        /// <summary>
        /// Determines whether to sort the generated fields by their size.
        /// </summary>
        /// <remarks>
        /// By default, the fields are laid out in the order found in the nested structs and no sorting is performed.
        /// </remarks>
        public bool SortFieldsBySize { get; set; }

        /// <summary>
        /// Determines whether the implementation for <see cref="IEquatable{T}"/> should be generated
        /// for both the outer and nested structs.
        /// <br/>
        /// Additionally, an implementation for <c>GetHashCode</c> using <see cref="EncosyTower.Common.HashValue"/>
        /// will also be generated.
        /// </summary>
        /// <remarks>
        /// For performance reason, this feature will assume that <see cref="IEquatable{T}"/> and <c>GetHashCode</c>
        /// have already been implemented on the type of each field.
        /// </remarks>
        public bool AutoEquatable { get; set;  }
    }

    /// <summary>
    /// Specifies a value to be associated with a struct for use in Polymorphic Enum Struct scenarios.
    /// </summary>
    /// <remarks>
    /// Apply this attribute to a nested struct to indicate that it should be constructed when a
    /// specific value is provided.
    /// Multiple instances of this attribute can be applied to a nested struct to associate it with multiple values.
    /// </remarks>
    [AttributeUsage(AttributeTargets.Struct, AllowMultiple = true)]
    public sealed class ConstructEnumCaseFromAttribute : Attribute
    {
        public ConstructEnumCaseFromAttribute([NotNull] object value)
        {
            Value = value;
        }

        public object Value { get; }
    }
}
