using System;
using EncosyTower.Data;

namespace EncosyTower.Databases.Authoring
{
    /// <summary>
    /// Marks a member as using a horizontal BakingSheet layout instead of
    /// <see cref="Cathei.BakingSheet.VerticalList{T}"/>.
    /// </summary>
    /// <remarks>
    /// Use this attribute for authoring members that should be imported as horizontal data.
    /// This is typically applied within types that participate in the authoring pipeline through
    /// <see cref="AuthorDatabaseAttribute"/>.
    /// </remarks>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = true)]
    public sealed class HorizontalAttribute : Attribute
    {
        /// <summary>
        /// Gets the target data type that defines the horizontal mapping.
        /// The type must implement <see cref="IData"/>.
        /// </summary>
        public Type TargetType { get; }

        /// <summary>
        /// Gets the name of the target property on <see cref="TargetType"/>
        /// used for horizontal mapping.
        /// </summary>
        public string PropertyName { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="HorizontalAttribute"/> class.
        /// </summary>
        /// <param name="targetType">A non-abstract type that implements <see cref="IData"/>.</param>
        /// <param name="propertyName">The name of an existing property on <paramref name="targetType"/>.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="targetType"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="InvalidCastException">
        /// Thrown when <paramref name="targetType"/> does not implement <see cref="IData"/>.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown when <paramref name="targetType"/> is abstract.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="propertyName"/> is null, empty, whitespace, or does not exist on
        /// <paramref name="targetType"/>.
        /// </exception>
        public HorizontalAttribute(Type targetType, string propertyName)
        {
            if (targetType == null)
            {
                throw new ArgumentNullException(nameof(targetType));
            }

            if (typeof(IData).IsAssignableFrom(targetType) == false)
            {
                throw new InvalidCastException($"{targetType} does not implement {typeof(IData)}");
            }

            if (targetType.IsAbstract)
            {
                throw new InvalidOperationException($"{targetType} cannot be abstract");
            }

            if (string.IsNullOrWhiteSpace(propertyName))
            {
                throw new ArgumentException($"Property name `{propertyName}` is invalid", nameof(propertyName));
            }

            if (targetType.GetProperty(propertyName) == null)
            {
                throw new ArgumentException($"Target type {targetType} does not contain any property named `{propertyName}`", nameof(propertyName));
            }

            this.TargetType = targetType;
            this.PropertyName = propertyName;
        }
    }
}
