using System;
using System.Diagnostics.CodeAnalysis;

namespace EncosyTower.VisualDebugging.Commands
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public sealed class VisualOrderAttribute : Attribute
    {
        public int Order { get; }

        public VisualOrderAttribute(int order)
        {
            Order = order;
        }

        private string[] Provider() => default;
    }

    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public sealed class VisualIgnoredAttribute : Attribute { }

    /// <summary>
    /// Attribute to specify the options for a visual property.
    /// <br/>
    /// The <see cref="OptionsGetter"/> should be a method that returns
    /// an <see cref="System.Collections.Generic.IReadOnlyList{string}"/>.
    /// <br/>
    /// The <see cref="IsDataStatic"/> should be <c>true</c> if the returned options never change.
    /// </summary>
    /// <remarks>
    /// The <see cref="IVisualCommand"/> must contains a method to set the selected option for the property.
    /// The method must satisfy these requirements:
    /// <list type="bullet">
    /// <item>Its name must be prefixed with <c>SetOptionFor</c> and suffixed with the property name.</item>
    /// <item>It must have a single parameter of type <see cref="VisualOption"/>.</item>
    /// <item>It must be annotated with <c>[RelayCommand]</c>.</item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <code>
    /// [ObservableProperty]
    /// [VisualOptions(nameof(GetResourceTypeOptions), true)]
    /// private ResourceType ResourceType
    /// {
    ///     get => Get_ResourceType();
    ///     set => Set_ResourceType(value);
    /// }
    ///
    /// private string[] GetResourceTypeOptions => default;
    ///
    /// [RelayCommand]
    /// private void SetOptionForResourceType(VisualOption option) { }
    /// </code>
    /// </example>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public sealed class VisualOptionsAttribute : Attribute
    {
        /// <summary>
        /// The method that returns the options.
        /// </summary>
        public string OptionsGetter { get; }

        /// <summary>
        /// Indicates if the returned options never change.
        /// </summary>
        public bool IsDataStatic { get; }

        public VisualOptionsAttribute([NotNull] string optionsGetter, bool isDataStatic = false)
        {
            OptionsGetter = optionsGetter;
            IsDataStatic = isDataStatic;
        }
    }
}
