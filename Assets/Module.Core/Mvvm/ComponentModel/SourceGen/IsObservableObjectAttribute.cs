using System;

namespace Module.Core.Mvvm.ComponentModel.SourceGen
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public sealed class IsObservableObjectAttribute : Attribute
    {
        public Type Type { get; }

        public IsObservableObjectAttribute(Type type)
        {
            this.Type = type;
        }
    }
}
