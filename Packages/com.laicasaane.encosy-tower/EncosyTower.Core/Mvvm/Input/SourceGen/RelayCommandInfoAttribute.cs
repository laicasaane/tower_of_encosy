using System;

namespace EncosyTower.Mvvm.Input.SourceGen
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
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
