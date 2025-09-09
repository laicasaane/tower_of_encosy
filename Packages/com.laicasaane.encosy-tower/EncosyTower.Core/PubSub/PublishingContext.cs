#if UNITASK || UNITY_6000_0_OR_NEWER

using System.Runtime.CompilerServices;
using EncosyTower.Logging;

namespace EncosyTower.PubSub
{
    public readonly struct PublishingContext
    {
        private readonly bool _ignoreEmptySubscriber;
        private readonly ILogger _logger;
        private readonly CallerInfo _callerInfo;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private PublishingContext(bool ignoreEmptySubscriber, ILogger logger, CallerInfo callerInfo)
        {
            _ignoreEmptySubscriber = ignoreEmptySubscriber;
            _logger = logger;
            _callerInfo = callerInfo;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static PublishingContext Get(
              bool ignoreEmptySubscriber = false
            , ILogger logger = default
            , [CallerLineNumber] int lineNumber = 0
            , [CallerMemberName] string memberName = ""
            , [CallerFilePath] string filePath = ""
        )
        {
            var callerInfo = new CallerInfo(lineNumber, memberName, filePath);
            return new(ignoreEmptySubscriber, logger, callerInfo);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static PublishingContext GetSlim(
              bool ignoreEmptySubscriber = false
            , ILogger logger = default
        )
        {
            return new(ignoreEmptySubscriber, logger, default);
        }

        public ILogger Logger => _logger ?? DevLogger.Default;

        public CallerInfo CallerInfo => _callerInfo;

        public bool IgnoreEmptySubscriber => _ignoreEmptySubscriber;
    }
}

#endif
