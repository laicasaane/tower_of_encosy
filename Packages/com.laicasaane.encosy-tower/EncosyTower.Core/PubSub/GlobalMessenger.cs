#if UNITASK || UNITY_6000_0_OR_NEWER

namespace EncosyTower.PubSub
{
    using System.Buffers;

#if UNITASK
    using UnityTask = Cysharp.Threading.Tasks.UniTask;
#else
    using UnityTask = UnityEngine.Awaitable;
#endif

    public static partial class GlobalMessenger
    {
        private static Messenger s_instance;

        private static Messenger Instance => s_instance ??= new(ArrayPool<UnityTask>.Shared);

        public static MessagePublisher Publisher => Instance.Publisher;

        public static MessageSubscriber Subscriber => Instance.Subscriber;
    }
}

#if UNITY_EDITOR

namespace EncosyTower.PubSub
{
    using UnityEditor;
    using UnityEngine.Scripting;

    partial class GlobalMessenger
    {
        [InitializeOnEnterPlayMode, Preserve]
        private static void InitWhenDomainReloadDisabled()
        {
            s_instance?.Dispose();
            s_instance = null;
        }
    }
}

#endif

#endif
