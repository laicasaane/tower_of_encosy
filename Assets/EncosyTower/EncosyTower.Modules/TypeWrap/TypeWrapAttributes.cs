using System;

namespace EncosyTower.Modules.TypeWrap
{
    [AttributeUsage(AttributeTargets.Struct | AttributeTargets.Class)]
    public sealed class WrapTypeAttribute : Attribute
    {
        public const string DEFAULT_MEMBER_NAME = "value";

        public Type Type { get; }

        public string MemberName { get; }

        public bool ExcludeConverter { get; set; }

        public WrapTypeAttribute(Type type) : this(type, DEFAULT_MEMBER_NAME)
        { }

        public WrapTypeAttribute(Type type, string memberName)
        {
            Type = type;
            MemberName = memberName;
        }
    }

    [AttributeUsage(AttributeTargets.Struct | AttributeTargets.Class)]
    public sealed class WrapRecordAttribute : Attribute
    {
        public bool ExcludeConverter { get; set; }
    }

    public interface IWrapper { }

    public interface IWrap<T> : IWrapper { }
}
