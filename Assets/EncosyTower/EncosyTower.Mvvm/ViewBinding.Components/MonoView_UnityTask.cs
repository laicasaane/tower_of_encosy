#if UNITASK || UNITY_6000_0_OR_NEWER

using System.Threading;
using EncosyTower.Mvvm.ComponentModel;
using EncosyTower.Tasks;

namespace EncosyTower.Mvvm.ViewBinding.Components
{
#if UNITASK
    using UnityTask = Cysharp.Threading.Tasks.UniTask<IObservableObject>;
#else
    using UnityTask = UnityEngine.Awaitable<IObservableObject>;
#endif

    public partial class MonoView
    {
        private async UnityTask GetContextAsync(CancellationToken token)
        {
            await UnityTasks.WaitUntil(
                  this
                , static state => state._context != null && state._context.TryGetContext(out _)
                , token
            );

            if (token.IsCancellationRequested)
            {
                return null;
            }

            return _context.TryGetContext(out var context) ? context : null;
        }
    }
}

#endif
