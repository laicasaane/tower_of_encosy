#if UNITY_COLLECTIONS || SYSTEM_RUNTIME_COMPILER_SERVICES_UNSAFE

namespace Module.Core.Pooling
{
    public struct GameObjectInfo
    {
        public int instanceId;
        public int transformId;
        public int transformArrayIndex;
    }
}

#endif
