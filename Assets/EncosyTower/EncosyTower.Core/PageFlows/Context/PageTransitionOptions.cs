#if UNITASK || UNITY_6000_0_OR_NEWER

using System;
using EncosyTower.EnumExtensions;

namespace EncosyTower.PageFlows
{
    public enum PageTransitionOperation
    {
        Show,
        Hide,
    }

    [Flags]
    public enum PageTransitionOptions
    {
        Default = 0,
        NoTransition = 1 << 0,
        ZeroDuration = 1 << 1,
        OnlyFirstPageHasDuration = 1 << 2,
        OnlyLastPageHasDuration = 1 << 3,
    }

    [EnumExtensionsFor(typeof(PageTransitionOptions))]
    public static partial class PageTransitionOptionsExtensions { }
}

#endif
