using System;
using System.Diagnostics.CodeAnalysis;

namespace EncosyTower.PolyEnumStructs
{
    /// <summary>
    /// Annotates any struct to mark it as a root enum-struct.
    /// The functionality related to Poly Enum Struct will be generated into itself.
    /// </summary>
    /// <remarks>
    /// <list type="bullet">
    /// <item>The idea of Polymorphic Enum Struct is based on both 'discriminated union' and 'interface polymorphism'.</item>
    /// <item>Any nested struct will be automatically recognized as a case-struct for the outer enum-struct.</item>
    /// <item>In polymorphism terms, the enum-struct acts as the base type, where case-structs act as derived types.</item>
    /// <item>
    /// A common interface named <c>IEnumCase</c> will be generated into the enum-struct.
    /// The members of the enum-struct are aggregated from the identical members of all case-structs,
    /// given that they have the same name, same return type, same parameters.
    /// </item>
    /// <item>Both the enum-struct and case-structs will automatically derive <c>IEnumCase</c>.</item>
    /// <item>The members of <c>IEnumCase</c> will be automatically implemented on the enum-struct.</item>
    /// <item>Additional facilities to convert the enum-struct to a case-struct, and vice versa, will be generated.</item>
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
        /// Determines whether to sort the generated fields of the enum-struct by their size.
        /// </summary>
        /// <remarks>
        /// By default, the fields are laid out in the order found in all case-structs and no sorting is performed.
        /// </remarks>
        public bool SortFieldsBySize { get; set; }

        /// <summary>
        /// Determines whether the implementation for <see cref="IEquatable{T}"/> should be generated
        /// for both the enum-struct and case-structs.
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
    /// Specifies a value to be associated with a case-struct.
    /// </summary>
    /// <remarks>
    /// Apply this attribute to a case-struct to indicate that it should be constructed when a
    /// specific value is provided.
    /// <br/>
    /// Multiple instances of this attribute can be applied to a case-struct to associate it with multiple values.
    /// </remarks>
    /// <example>
    /// <code>
    /// [PolyEnumStruct]
    /// public partial struct Task
    /// {
    ///     [ConstructEnumCaseFrom(1)]
    ///     public partial struct Shopping
    ///     {
    ///         public void Execute() { }
    ///     }
    ///
    ///     [ConstructEnumCaseFrom(2)]
    ///     [ConstructEnumCaseFrom(DataType.TaskWorking)]
    ///     public partial struct Working
    ///     {
    ///         public void Execute() { }
    ///     }
    ///
    ///     // The 'Task.ConstructFrom(...)' methods are generated.
    ///     // The 'TryGetConstructionValue(out value)' methods for each struct are generated.
    /// }
    /// </code>
    /// </example>
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
