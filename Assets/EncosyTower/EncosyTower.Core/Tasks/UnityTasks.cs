#if UNITASK || UNITY_6000_0_OR_NEWER

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
