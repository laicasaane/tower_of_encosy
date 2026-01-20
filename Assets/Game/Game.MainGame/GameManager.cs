using Cysharp.Threading.Tasks;
using EncosyTower.PubSub;
using UnityEngine;

namespace Module.MainGame
{
    public sealed class GameManager : MonoBehaviour
    {
        [SerializeField] private float _subscriptionDelay = 1f;

        private float _subscriptionDelayElapsedTime = 0f;

        private void Awake()
        {
            Debug.Log("[GameManager] Awake");
            Debug.Log("[GameManager] Add an interceptor...");

            GlobalMessenger.Interceptors.AddInterceptor(new Interceptor());

            Debug.Log("[GameManager] Publishing a Test Message with content 'From Awake'...");

            GlobalMessenger.Publisher.UnityScope(this).Publish(
                  new TestMsg("From Awake")
                , PublishingContext.WaitForSubscriber(token: destroyCancellationToken)
            );

            Debug.Log("[GameManager] Test Message published. Wait for response...");
        }

        private void Update()
        {
            _subscriptionDelayElapsedTime += Time.deltaTime;

            if (_subscriptionDelayElapsedTime >= _subscriptionDelay)
            {
                GlobalMessenger.Subscriber.UnityScope(this)
                    .WithState(this)
                    .Subscribe<TestMsg>(Handle, destroyCancellationToken);
            }
        }

        private static void Handle(GameManager state, TestMsg msg)
        {
            Debug.Log($"[GameManager] Test Message received: {msg.Text}");
        }

        private readonly record struct TestMsg(string Text) : IMessage;

        private class Interceptor : IMessageInterceptor<TestMsg>
        {
            public UniTask InterceptAsync(TestMsg msg, PublishingContext context, PublishContinuation<TestMsg> continuation)
            {
                Debug.Log("[GameManager+Interceptor] Intercepting Test Message, prefixed with 'Intercepted: '.");

                return continuation(msg with { Text = $"Intercepted: {msg.Text}" }, context);
            }
        }
    }
}
