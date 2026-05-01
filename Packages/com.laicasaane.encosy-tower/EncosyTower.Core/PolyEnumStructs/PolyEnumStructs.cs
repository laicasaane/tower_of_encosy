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
    /// <item>A common interface named <c>IEnumCase</c> will be generated into the enum-struct.</item>
    /// <item>The members of <c>IEnumCase</c> are aggregated from the identical members of all case-structs.
    /// They must have the same name, same return type, same parameters.</item>
    /// <item>Both the enum-struct and case-structs will automatically derive <c>IEnumCase</c>.</item>
    /// <item>The members of <c>IEnumCase</c> will be automatically implemented on the enum-struct.</item>
    /// <item>Additional facilities to convert the enum-struct to a case-struct, and vice versa, will be generated.</item>
    /// <item>Additional facilities to help the polymorphism usage will be generated.</item>
    /// <item>If <see cref="SortFieldsBySize"/> is enabled, the fields of the enum-struct will be sorted by size (in bytes).</item>
    /// <item>If <see cref="AutoEquatable"/> is enabled, <see cref="IEquatable{T}"/> will be implemented for all structs.</item>
    /// <item>If <see cref="WithEnumExtensions"/> is enabled, the <c>EnumCase</c> will have its extensions generated.</item>
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
        public bool AutoEquatable { get; set; }

        /// <summary>
        /// Determines whether enum extensions should be generated for the nested <c>EnumCase</c> type.
        /// </summary>
        public bool WithEnumExtensions { get; set; }
    }

    /// <summary>
    /// Specifies one or more values to be associated with a case-struct.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Apply this attribute to a case-struct to indicate that it should be constructed when a
    /// specific value is provided.
    /// </para>
    /// <para>
    /// Multiple instances of this attribute can be applied to a case-struct to associate it with multiple values.
    /// </para>
    /// <para>
    /// Each case-struct will receive generated methods:
    /// <list type="bullet">
    /// <item><c>TryGetConstructionValue</c> to get the associated value from the case-struct instance.</item>
    /// </list>
    /// </para>
    /// <para>
    /// The enum-struct will receive generated methods:
    /// <list type="bullet">
    /// <item><c>ConstructFrom</c> to construct a case-struct from the associated value.</item>
    /// <item><c>TryGetConstructionValue</c> to get the associated value from a case-struct instance.</item>
    /// </list>
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// [PolyEnumStruct]
    /// public partial struct Task
    /// {
    ///     [EnumCaseValue(1)]
    ///     public partial struct Shopping
    ///     {
    ///         public void Execute() { }
    ///     }
    ///
    ///     [EnumCaseValue(2)]
    ///     [EnumCaseValue(DataType.TaskWorking)]
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
    /// <seealso cref="PolyEnumStructAttribute"/>
    [AttributeUsage(AttributeTargets.Struct, AllowMultiple = true)]
    public sealed class EnumCaseValueAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of <see cref="EnumCaseValueAttribute"/> with the specified value.
        /// </summary>
        /// <param name="value">
        /// The value to associate with the case-struct. Can be a primitive, enum, or any constant value.
        /// </param>
        /// <inheritdoc cref="EnumCaseValueAttribute" />
        public EnumCaseValueAttribute([NotNull] object value)
        {
            Value = value;
        }

        /// <summary>
        /// Gets the value associated with the case-struct.
        /// </summary>
        public object Value { get; }
    }

    /// <summary>
    /// Annotates a nested struct to exclude it from being recognized as a case-struct
    /// for the outer enum-struct.
    /// </summary>
    [AttributeUsage(AttributeTargets.Struct, AllowMultiple = false)]
    public sealed class EnumCaseIgnoreAttribute : Attribute
    {
    }

    /// <summary>
    /// Specifies a factory type for a poly enum struct.
    /// </summary>
    /// <example>
    /// <code>
    /// [PolyEnumStruct]
    /// public partial struct Task
    /// {
    ///     public partial record struct Shopping(int Allowance);
    ///
    ///     public partial record struct Working(float Duration);
    /// }
    ///
    /// [PolyEnumFactoryFor(typeof(Task))]
    /// public partial class TaskFactory
    /// {
    ///     private Task _task;
    ///
    ///     // User should define a constructor that accepts the enum-struct type as a single parameter.
    ///     // When undefined, the source generator will generate a private one, along with a private field
    ///     // to hold the enum-struct instance.
    ///
    ///     private TaskFactory(Task task)
    ///     {
    ///         _task = task;
    ///     }
    ///
    ///     // The static factory methods will be generated for each case-struct of the enum-struct.
    ///
    ///     public static TaskFactory Shopping(int allowance)
    ///         => new TaskFactory(new Task.Shopping(allowance));
    ///
    ///     public static TaskFactory Working(float duration)
    ///         => new TaskFactory(new Task.Working(duration));
    /// }
    /// </code>
    /// </example>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false, Inherited = false)]
    public sealed class PolyEnumFactoryForAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the PolyEnumFactoryForAttribute class with the specified type to be
        /// wrapped.
        /// </summary>
        /// <param name="type">The poly enum struct type to be wrapped.</param>
        /// <inheritdoc cref="PolyEnumFactoryForAttribute" />
        public PolyEnumFactoryForAttribute(Type type)
        {
            Type = type;
        }

        /// <summary>
        /// The poly enum struct type to be wrapped by the annotated wrapper type.
        /// </summary>
        public Type Type { get; }
    }
}
