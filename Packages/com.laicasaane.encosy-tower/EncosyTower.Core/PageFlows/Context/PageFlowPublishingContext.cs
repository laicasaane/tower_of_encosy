#if UNITASK || UNITY_6000_0_OR_NEWER

#if UNITY_EDITOR || DEVELOPMENT_BUILD
#if PAGE_FLOW_PUBSUB_INCLUDE_CALLER_INFO_DEV
#define __PUBSUB_INCLUDE_CALLER_INFO_DEV__
#endif
#endif

namespace EncosyTower.PageFlows
{
    public readonly struct PageFlowPublishingContext
    {
        public PubSubCallerInfoOption CallerInfoOption
        {
            get
            {
#if __PUBSUB_INCLUDE_CALLER_INFO_DEV__
                return PubSubCallerInfoOption.ForDevelopment;
#elif PAGE_FLOW_PUBSUB_INCLUDE_CALLER_INFO
                return PubSubCallerInfoOption.Always;
#else
                return PubSubCallerInfoOption.Never;
#endif
            }
        }
    }

    public enum PubSubCallerInfoOption : byte
    {
        /// <summary>
        /// Do not include caller info.
        /// </summary>
        Never = 0,

        /// <summary>
        /// Include caller info only for Editor and Development builds.
        /// </summary>
        ForDevelopment,

        /// <summary>
        /// Include caller info for all environments.
        /// </summary>
        Always,
    }
}

#endif
