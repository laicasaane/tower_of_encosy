#if UNITASK || UNITY_6000_0_OR_NEWER

using System.Threading;
using EncosyTower.Initialization;

namespace EncosyTower.UserDataVaults
{
#if UNITASK
    using UnityTask = Cysharp.Threading.Tasks.UniTask;
#else
    using UnityTask = UnityEngine.Awaitable;
#endif

    public interface IUserDataDirectory : IInitializable, IDeinitializable
    {
        string UserId { get; set; }

        void CreateDataIfNotExist();

        void MarkDirty(bool isDirty);

        UnityTask LoadEntireDirectoryAsync(SourcePriority priority, CancellationToken token);

        UnityTask SaveEntireDirectoryAsync(SaveDestination destination, CancellationToken token);
    }
}

#endif
