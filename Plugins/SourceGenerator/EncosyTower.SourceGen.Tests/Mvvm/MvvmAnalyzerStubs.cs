namespace EncosyTower.SourceGen.Tests.Mvvm;

internal static class MvvmAnalyzerStubs
{
    public const string ATTRIBUTES = """
        namespace EncosyTower.Mvvm.ComponentModel
        {
            [System.AttributeUsage(System.AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
            public class ObservableObjectAttribute : System.Attribute { }

            [System.AttributeUsage(System.AttributeTargets.Field | System.AttributeTargets.Property)]
            public class ObservablePropertyAttribute : System.Attribute
            {
                public ObservablePropertyAttribute() { }
                public ObservablePropertyAttribute(string name) { }
            }

            [System.AttributeUsage(System.AttributeTargets.Field | System.AttributeTargets.Property, AllowMultiple = true)]
            public class NotifyPropertyChangedForAttribute : System.Attribute
            {
                public NotifyPropertyChangedForAttribute(string propertyName) { }
                public NotifyPropertyChangedForAttribute(string propertyName, params string[] otherPropertyNames) { }
            }

            [System.AttributeUsage(System.AttributeTargets.Field | System.AttributeTargets.Property, AllowMultiple = true)]
            public class NotifyCanExecuteChangedForAttribute : System.Attribute
            {
                public NotifyCanExecuteChangedForAttribute(string commandName) { }
                public NotifyCanExecuteChangedForAttribute(string commandName, params string[] otherCommandNames) { }
            }
        }
        namespace EncosyTower.Mvvm.Input
        {
            [System.AttributeUsage(System.AttributeTargets.Method, Inherited = false)]
            public class RelayCommandAttribute : System.Attribute
            {
                public string CanExecute { get; set; }
            }
        }

        namespace EncosyTower.Mvvm.ViewBinding.Components
        {
            [System.AttributeUsage(System.AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
            public sealed class MonoBinderAttribute : System.Attribute
            {
                public MonoBinderAttribute(System.Type type) { }
                public System.Type Type { get; }
                public string PreprocessorGuard { get; set; }
                public bool ExcludeObsolete { get; set; }
            }

            [System.AttributeUsage(System.AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
            public sealed class MonoBinderExcludeParentAttribute : System.Attribute
            {
                public MonoBinderExcludeParentAttribute(System.Type parentType) { }
                public System.Type ParentType { get; }
            }

            [System.AttributeUsage(System.AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
            public sealed class MonoBindingExcludeAttribute : System.Attribute
            {
                public MonoBindingExcludeAttribute(string memberName) { }
                public string MemberName { get; }
            }

            [System.AttributeUsage(System.AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
            public sealed class MonoBindingPropertyAttribute : System.Attribute
            {
                public MonoBindingPropertyAttribute(string memberName) { }
                public string MemberName { get; }
                public bool UseCustomSetter { get; set; }
                public string Label { get; set; }
                public string SetterMethod { get; set; }
            }

            [System.AttributeUsage(System.AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
            public sealed class MonoBindingCommandAttribute : System.Attribute
            {
                public MonoBindingCommandAttribute(string memberName) { }
                public string MemberName { get; }
                public System.Type WrapperType { get; set; }
                public string Label { get; set; }
            }
        }

        namespace UnityEngine
        {
            public class Object { }
        }
        """;
}
