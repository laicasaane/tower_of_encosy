namespace EncosyTower.SourceGen.Tests.TypeWraps;

internal static class TypeWrapsAnalyzerStubs
{
    public const string ATTRIBUTES = """
        namespace EncosyTower.TypeWraps
        {
            [System.AttributeUsage(System.AttributeTargets.Struct | System.AttributeTargets.Class, Inherited=false, AllowMultiple=false)]
            public sealed class WrapTypeAttribute : System.Attribute
            {
                public const string DEFAULT_MEMBER_NAME = "value";

                public System.Type Type { get; }

                public string MemberName { get; }

                public bool ExcludeConverter { get; set; }

                public WrapTypeAttribute(System.Type type) : this(type, DEFAULT_MEMBER_NAME) { }

                public WrapTypeAttribute(System.Type type, string memberName)
                {
                    Type = type;
                    MemberName = memberName;
                }
            }

            [System.AttributeUsage(System.AttributeTargets.Struct | System.AttributeTargets.Class, Inherited=false, AllowMultiple=false)]
            public sealed class WrapRecordAttribute : System.Attribute
            {
                public bool ExcludeConverter { get; set; }
            }

            public interface IWrapper { }

            public interface IWrap<T> : IWrapper { }
        }

        namespace System.Runtime.CompilerServices
        {
            internal static class IsExternalInit { }
        }
        """;
}
