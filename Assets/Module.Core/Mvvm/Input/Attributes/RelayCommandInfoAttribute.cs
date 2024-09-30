using System;

namespace Module.Core.Mvvm.Input
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
    public sealed class RelayCommandInfoAttribute : Attribute
    {
        public string CommandName { get; }

        public Type ParameterType { get; }

        public RelayCommandInfoAttribute(string commandName, Type parameterType)
        {
            this.CommandName = commandName;
            this.ParameterType = parameterType;
        }
    }
}
