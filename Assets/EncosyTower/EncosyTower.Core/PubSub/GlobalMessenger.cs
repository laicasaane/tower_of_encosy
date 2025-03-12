#if UNITASK || UNITY_6000_0_OR_NEWER

using System.Buffers;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace EncosyTower.PubSub
{
    public static class GlobalMessenger
    {
        private static Messenger s_instance;

        private static Messenger Instance => s_instance ??= new(ArrayPool<UniTask>.Shared);

        public static MessagePublisher Publisher => Instance.Publisher;

        public static MessageSubscriber Subscriber => Instance.Subscriber;

#if UNITY_EDITOR
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        static void Init()
        {
            s_instance?.Dispose();
            s_instance = null;
        }
#endif
    }
}

#endif
