using System;

namespace EncosyTower.Modules.Mvvm.ComponentModel.SourceGen
{
    /// <summary>
    /// An attribute that indicates that a given property will notify clients when its value is changed.
    /// </summary>
    /// <remarks>
    /// This attribute is not intended to be used directly by user code to decorate user-defined types.
    /// <br/>
    /// However, it can be used in other contexts, such as reflection.
    /// </remarks>
    /// <seealso cref="INotifyPropertyChanged"/>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public sealed class NotifyPropertyChangedInfoAttribute : Attribute
    {
        public string PropertyName { get; }

        public Type PropertyType { get; }

        public NotifyPropertyChangedInfoAttribute(string propertyName, Type propertyType)
        {
            this.PropertyName = propertyName;
            this.PropertyType = propertyType;
        }
    }
}
