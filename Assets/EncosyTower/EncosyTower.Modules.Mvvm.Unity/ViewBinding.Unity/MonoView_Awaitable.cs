#if !UNITASK && UNITY_6000_0_OR_NEWER

using System.Threading;
using EncosyTower.Modules.Mvvm.ComponentModel;
using UnityEngine;

namespace EncosyTower.Modules.Mvvm.ViewBinding.Unity
{
    public partial class MonoView
    {
        public async Awaitable InitializeAsync(bool alsoStartListening, CancellationToken token = default)
        {
            var context = await GetContextAsync(token);
            InitializeInternal(context, alsoStartListening);
            Initialized = true;
        }

        private async Awaitable<IObservableObject> GetContextAsync(CancellationToken token)
        {
            await Awaitables.WaitUntil(
                  this
                , static state => state._context != null && state._context.TryGetContext(out _)
                , token
            );

            return _context.TryGetContext(out var context) ? context : null;
        }
    }
}

#endif
