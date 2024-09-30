using System;

namespace Module.Core.Mvvm.ViewBinding.SourceGen
{
    /// <summary>
    /// An attribute that indicates that a given method can be binded to any observable property.
    /// </summary>
    /// <remarks>
    /// This attribute is not intended to be used directly by user code to decorate user-defined types.
    /// <br/>
    /// However, it can be used in other contexts, such as reflection.
    /// </remarks>
    /// <seealso cref="BindingPropertyAttribute"/>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
    public sealed class BindingPropertyMethodInfoAttribute : Attribute
    {
        public string MethodName { get; }

        public Type ParameterType { get; }

        public BindingPropertyMethodInfoAttribute(string methodName, Type parameterType)
        {
            this.MethodName = methodName;
            this.ParameterType = parameterType;
        }
    }
}
