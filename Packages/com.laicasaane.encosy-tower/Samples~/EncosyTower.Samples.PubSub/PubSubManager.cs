using System;
using System.Collections.Generic;
using EncosyTower.Common;
using EncosyTower.Logging;
using EncosyTower.PubSub;
using EncosyTower.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace EncosyTower.Samples.PubSub
{
#if UNITASK
    using UnityTask = Cysharp.Threading.Tasks.UniTask;
#else
    using UnityTask = UnityEngine.Awaitable;
#endif

    internal class PubSubManager : MonoBehaviour
    {
        [SerializeField] private Button _buttonClearLog;
        [SerializeField] private Toggle _toggleSubscribe;
        [SerializeField] private Button _buttonPublish;
        [SerializeField] private Button _buttonPublishAsync;
        [SerializeField] private Toggle _toggleWaitForSubscriber;
        [SerializeField] private Toggle _toggleEnableInterceptor;
        [SerializeField] private TMP_InputField _inputMessageToPublish;
        [SerializeField] private TMP_InputField _inputInterceptedFormat;
        [SerializeField] private TMP_InputField _output;
        [SerializeField] private GameObject _popupRoot;
        [SerializeField] private GameObject _asyncIndicatorPrefab;

        private readonly StringBuilderLogger _logger = new();
        private readonly LogInterceptor _logInterceptor = new();
        private readonly AsyncLogInterceptor _asyncLogInterceptor = new();
        private readonly List<ISubscription> _subscriptions = new();

        private bool _enabledInterceptor;
        private bool _waitForSubscriber;
        private string _messageToPublish;
        private GameObject _asyncIndicator;

        private void Awake()
        {
            _buttonClearLog.onClick.AddListener(Button_ClearLog);
            _toggleSubscribe.onValueChanged.AddListener(Toggle_Subscribe);
            _buttonPublish.onClick.AddListener(Button_Publish);
            _buttonPublishAsync.onClick.AddListener(Button_PublishAsync);
            _toggleWaitForSubscriber.onValueChanged.AddListener(Toggle_WaitForSubscriber);
            _toggleEnableInterceptor.onValueChanged.AddListener(Toggle_EnableInterceptor);
            _inputMessageToPublish.onValueChanged.AddListener(TextField_SetMessageToPublish);
            _inputInterceptedFormat.onValueChanged.AddListener(TextField_SetInterceptedFormat);

            _asyncIndicator = Instantiate(_asyncIndicatorPrefab, _popupRoot.transform, false);
            _asyncIndicator.SetActive(false);

            _messageToPublish = _inputMessageToPublish.text;
            _logInterceptor.Format = _inputInterceptedFormat.text;
            _asyncLogInterceptor.Format = _inputInterceptedFormat.text;

            _logger.OnLogEntryWritten += UpdateOutput;
            _logger.LogInfo("Pub/Sub Manager initialized.");
        }

        private void Subscribe()
        {
            var subscriber = GlobalMessenger.Subscriber.UnityScope(this)
                .WithState(this)
                .WithSubscriptions(_subscriptions);

            subscriber.Subscribe<LogMsg>(Handle);
            subscriber.Subscribe<AsyncLogMsg>(HandleAsync);
        }

        private void Publish()
        {
            _asyncIndicator.SetActive(_waitForSubscriber);

            var token = destroyCancellationToken;
            var context = _waitForSubscriber
                ? PublishingContext.WaitForSubscriber(logger: _logger, token: token)
                : PublishingContext.DropIfNoSubscriber(logger: _logger, token: token);

            GlobalMessenger.Publisher.UnityScope(this)
                .Publish(new LogMsg(_messageToPublish), context);
        }

        private async UnityTask PublishAsync()
        {
            _asyncIndicator.SetActive(_waitForSubscriber);

            var token = destroyCancellationToken;
            var context = _waitForSubscriber
                ? PublishingContext.WaitForSubscriber(logger: _logger, token: token)
                : PublishingContext.DropIfNoSubscriber(logger: _logger, token: token);

            await GlobalMessenger.Publisher.UnityScope(this)
                .PublishAsync(new AsyncLogMsg(_messageToPublish), context);

            _asyncIndicator.SetActive(false);
        }

        private void Log(string text)
        {
            _logger.LogLine($"[{DateTime.Now.TimeOfDay:hh\\:mm\\:ss}] Received: {text}");
        }

        private void UpdateOutput()
        {
            _output.text = _logger.ToString();
        }

        private static void Handle(PubSubManager state, LogMsg msg, PublishingContext ctx)
        {
            state._asyncIndicator.SetActive(false);
            state.Log($"Sync: {msg.Text}");
        }

        private static async UnityTask HandleAsync(PubSubManager state, AsyncLogMsg msg, PublishingContext ctx)
        {
            // Simulate some async work
            await UnityTasks.NextFrameAsync(ctx.Token);

            state.Log($"Async: {msg.Text}");
        }

        private readonly record struct LogMsg(string Text) : IMessage;

        private readonly record struct AsyncLogMsg(string Text) : IMessage;

        private class LogInterceptor : IMessageInterceptor<LogMsg>
        {
            public string Format { get; set; }

            public UnityTask InterceptAsync(
                  LogMsg msg
                , PublishingContext context
                , PublishContinuation<LogMsg> continuation
            )
            {
                var text = msg.Text;

                if (Format.IsNotEmpty())
                {
                    text = string.Format(Format, msg.Text);
                }

                return continuation(new LogMsg(text), context);
            }
        }

        private class AsyncLogInterceptor : IMessageInterceptor<AsyncLogMsg>
        {
            public string Format { get; set; }

            public UnityTask InterceptAsync(
                  AsyncLogMsg msg
                , PublishingContext context
                , PublishContinuation<AsyncLogMsg> continuation
            )
            {
                var text = msg.Text;

                if (Format.IsNotEmpty())
                {
                    text = string.Format(Format, msg.Text);
                }

                return continuation(new AsyncLogMsg(text), context);
            }
        }

        #region    UI EVENTS
        #endregion =========

        private void Button_ClearLog()
        {
            _logger.Clear();
            _output.text = string.Empty;
        }

        private void Toggle_Subscribe(bool enabled)
        {
            _subscriptions.Unsubscribe();

            if (enabled)
            {
                Subscribe();
            }
        }

        private void Button_Publish()
        {
            Publish();
        }

        private void Button_PublishAsync()
        {
            UnityTasks.Forget(PublishAsync());
        }

        private void Toggle_WaitForSubscriber(bool enabled)
        {
            _waitForSubscriber = enabled;
        }

        private void Toggle_EnableInterceptor(bool enabled)
        {
            if (_enabledInterceptor)
            {
                GlobalMessenger.Interceptors.RemoveInterceptor(_logInterceptor);
                GlobalMessenger.Interceptors.RemoveInterceptor(_asyncLogInterceptor);
            }

            if (enabled)
            {
                GlobalMessenger.Interceptors.AddInterceptor(_logInterceptor);
                GlobalMessenger.Interceptors.AddInterceptor(_asyncLogInterceptor);
            }

            _enabledInterceptor = enabled;
            _inputInterceptedFormat.interactable = enabled;
        }

        private void TextField_SetMessageToPublish(string value)
        {
            _messageToPublish = value;
        }

        private void TextField_SetInterceptedFormat(string value)
        {
            _logInterceptor.Format = value;
            _asyncLogInterceptor.Format = value;
        }
    }
}
