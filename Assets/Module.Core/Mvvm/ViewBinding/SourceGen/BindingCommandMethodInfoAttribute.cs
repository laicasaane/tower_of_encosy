using System;

namespace Module.Core.Mvvm.ViewBinding.SourceGen
{
    /// <summary>
    /// An attribute that indicates that a given method can be binded to any relay command.
    /// </summary>
    /// <remarks>
    /// This attribute is not intended to be used directly by user code to decorate user-defined types.
    /// <br/>
    /// However, it can be used in other contexts, such as reflection.
    /// </remarks>
    /// <seealso cref="BindingCommandAttribute"/>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
    public sealed class BindingCommandMethodInfoAttribute : Attribute
    {
        public string MethodName { get; }

        public Type ParameterType { get; }

        public BindingCommandMethodInfoAttribute(string methodName, Type parameterType)
        {
            this.MethodName = methodName;
            this.ParameterType = parameterType;
        }
    }
}
