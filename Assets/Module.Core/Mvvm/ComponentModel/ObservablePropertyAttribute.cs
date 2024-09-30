using System;

namespace Module.Core.Mvvm.ComponentModel
{
    /// <summary>
    /// An attribute that indicates that a given field should be wrapped by a generated observable property.
    /// In order to use this attribute, the containing type has to implement <see cref="IObservableObject"/>.
    /// <para>
    /// This attribute can be used as follows:
    /// <code>
    /// partial class MyViewModel : IObservableObject
    /// {
    ///     [ObservableProperty]
    ///     private string _name;
    ///
    ///     [ObservableProperty("IsEnabled")]
    ///     private bool m_isEnabled;
    /// }
    /// </code>
    /// </para>
    /// </summary>
    /// <remarks>
    /// The generated properties will automatically use the <c>UpperCamelCase</c> format for their names,
    /// which will be derived from the field names. The generator can also recognize fields using either
    /// the <c>_lowerCamel</c> or <c>m_lowerCamel</c> naming scheme. Otherwise, the first character in the
    /// source field name will be converted to uppercase (eg. <c>isEnabled</c> to <c>IsEnabled</c>).
    /// </remarks>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public sealed class ObservablePropertyAttribute : Attribute
    {
        public string Name { get; }

        public ObservablePropertyAttribute()
            : this("")
        { }

        public ObservablePropertyAttribute(string name)
        {
            Name = name ?? string.Empty;
        }
    }
}
