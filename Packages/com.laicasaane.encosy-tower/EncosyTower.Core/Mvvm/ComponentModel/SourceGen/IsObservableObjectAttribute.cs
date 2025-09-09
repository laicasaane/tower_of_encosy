using System;

namespace EncosyTower.Mvvm.ComponentModel.SourceGen
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public sealed class IsObservableObjectAttribute : Attribute
    {
        public Type Type { get; }

        public IsObservableObjectAttribute(Type type)
        {
            this.Type = type;
        }
    }
}
