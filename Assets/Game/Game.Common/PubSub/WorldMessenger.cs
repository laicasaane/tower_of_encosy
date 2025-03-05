using System.Buffers;
using Cysharp.Threading.Tasks;
using EncosyTower.PubSub;
using UnityEngine;

namespace Module.GameCommon.PubSub
{
    public static class WorldMessenger
    {
        public static MessagePublisher Publisher => Instance.Publisher;

        public static MessageSubscriber Subscriber => Instance.Subscriber;

        private static Messenger Instance => s_instance ??= new(ArrayPool<UniTask>.Shared);

        private static Messenger s_instance;

#if UNITY_EDITOR
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void Init()
        {
            s_instance?.Dispose();
            s_instance = null;
        }
#endif
    }
}
