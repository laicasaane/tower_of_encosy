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

            GlobalMessenger.Publisher.UnityScope(this).Publish(
                  new TestMsg("From Awake")
                , PublishingContext.WaitForSubscriber()
                , destroyCancellationToken
            );
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
            Debug.Log($"[GameManager] TestMsg received: {msg.Text}");
        }

        private readonly record struct TestMsg(string Text) : IMessage;
    }
}
