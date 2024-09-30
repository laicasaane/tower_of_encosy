using System;
using System.Linq;

namespace Module.Core.Mvvm.ComponentModel
{
    /// <summary>
    /// An attribute that can be used to support <see cref="ObservablePropertyAttribute"/> in generated properties.
    /// When this attribute is used, the generated property setter will also call OnPropertyChanged
    /// (or the equivalent method in the target class) for the properties specified in the attribute data.
    /// This can be useful to keep the code compact when there are one or more dependent properties
    /// that should also be reported as updated when the value of the annotated observable property is changed.
    /// If this attribute is used in a field without <see cref="ObservablePropertyAttribute"/>, it is ignored.
    /// <para>
    /// In order to use this attribute, the containing type has to implement the <see cref="IObservableObject"/> interface.
    /// </para>
    /// <para>
    /// This attribute can be used as follows:
    /// <code>
    /// partial class MyViewModel : IObservableObject
    /// {
    ///     [ObservableProperty]
    ///     [NotifyPropertyChangedFor(nameof(FullName))]
    ///     private string name;
    ///
    ///     [ObservableProperty]
    ///     [NotifyPropertyChangedFor(nameof(FullName))]
    ///     private string surname;
    ///
    ///     public string FullName => $"{Name} {Surname}";
    /// }
    /// </code>
    /// </para>
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = true, Inherited = false)]
    public sealed class NotifyPropertyChangedForAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NotifyPropertyChangedForAttribute"/> class.
        /// </summary>
        /// <param name="propertyName">The name of the property to also notify when the annotated property changes.</param>
        public NotifyPropertyChangedForAttribute(string propertyName)
        {
            PropertyNames = new[] { propertyName };
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NotifyPropertyChangedForAttribute"/> class.
        /// </summary>
        /// <param name="propertyName">The name of the property to also notify when the annotated property changes.</param>
        /// <param name="otherPropertyNames">
        /// The other property names to also notify when the annotated property changes. This parameter can optionally
        /// be used to indicate a series of dependent properties from the same attribute, to keep the code more compact.
        /// </param>
        public NotifyPropertyChangedForAttribute(string propertyName, params string[] otherPropertyNames)
        {
            PropertyNames = new[] { propertyName }.Concat(otherPropertyNames).ToArray();
        }

        /// <summary>
        /// Gets the property names to also notify when the annotated property changes.
        /// </summary>
        public string[] PropertyNames { get; }
    }
}
