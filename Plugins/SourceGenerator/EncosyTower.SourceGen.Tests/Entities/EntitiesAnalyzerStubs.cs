namespace EncosyTower.SourceGen.Tests.Entities;

internal static class EntitiesAnalyzerStubs
{
    public const string ATTRIBUTES = """
        namespace Unity.Entities
        {
            public interface IComponentData { }

            public interface IBufferElementData { }

            public interface ISharedComponentData { }

            public interface IEnableableComponent { }
        }

        namespace EncosyTower.Entities
        {
            [System.AttributeUsage(System.AttributeTargets.Struct, AllowMultiple = true)]
            public sealed class TypeHandleAttribute : System.Attribute
            {
                public TypeHandleAttribute(System.Type type) { }
                public TypeHandleAttribute(System.Type type, bool isReadOnly) { }
            }

            public interface IBufferLookups { }

            public interface IComponentLookups { }

            public interface IEnableableBufferLookups { }

            public interface IEnableableComponentLookups { }

            public interface IPhysicsBufferLookups { }

            public interface IPhysicsComponentLookups { }

            public interface IPhysicsEnableableComponentLookups { }

            [System.AttributeUsage(System.AttributeTargets.Struct, AllowMultiple = true)]
            public sealed class LookupAttribute : System.Attribute
            {
                public System.Type Type { get; }
                public bool IsReadOnly { get; }

                public LookupAttribute(System.Type type)
                {
                    Type = type;
                    IsReadOnly = false;
                }

                public LookupAttribute(System.Type type, bool isReadOnly)
                {
                    Type = type;
                    IsReadOnly = isReadOnly;
                }
            }
        }
        """;
}
