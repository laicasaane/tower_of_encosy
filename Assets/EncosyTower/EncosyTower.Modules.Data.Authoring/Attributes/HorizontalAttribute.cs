using System;

namespace EncosyTower.Modules.Data.Authoring
{
    /// <summary>
    /// Specify a property as a horizontal list, as opposed to <see cref="Cathei.BakingSheet.VerticalList{T}"/>.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = true)]
    public sealed class HorizontalAttribute : Attribute
    {
        /// <summary>
        /// Type implements <see cref="IData"/>
        /// </summary>
        public Type TargetType { get; }

        /// <summary>
        /// Name of a property of <see cref="TargetType"/>
        /// </summary>
        public string PropertyName { get; }

        /// <summary>
        /// Specify a property as a horizontal list, as opposed to <see cref="Cathei.BakingSheet.VerticalList{T}"/>.
        /// </summary>
        /// <param name="targetType">Type implements <see cref="IData"/></param>
        /// <param name="propertyName">Name of a property of <paramref name="targetType"/></param>
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
