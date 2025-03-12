#if UNITASK || UNITY_6000_0_OR_NEWER

#pragma warning disable IDE0005

namespace EncosyTower.Tasks
{
#if UNITASK
    using UnityTask = Cysharp.Threading.Tasks.UniTask;
#else
    using UnityTask = UnityEngine.Awaitable;
#endif

    public static partial class UnityTasks
    {

    }
}

#endif
