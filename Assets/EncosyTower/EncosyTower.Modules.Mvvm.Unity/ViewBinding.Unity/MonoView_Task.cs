#if !(UNITASK || UNITY_6000_0_OR_NEWER)

using System;
using System.Threading;
using System.Threading.Tasks;
using EncosyTower.Modules.Mvvm.ComponentModel;
using UnityEngine;

namespace EncosyTower.Modules.Mvvm.ViewBinding.Unity
{
    public partial class MonoView
    {
        public async Task InitializeAsync(bool alsoStartListening, CancellationToken token = default)
        {
            var context = await GetContextAsync(token);
            InitializeInternal(context, alsoStartListening);
            Initialized = true;
        }

        private async ValueTask<IObservableObject> GetContextAsync(CancellationToken token)
        {
            await WaitForContextAsync(token);
            return _context.TryGetContext(out var context) ? context : null;
        }

        private async ValueTask WaitForContextAsync(CancellationToken token)
        {
            while (_context == null || _context.TryGetContext(out _) == false)
            {
                if (token.IsCancellationRequested)
                {
                    return;
                }

                await Task.Delay(TimeSpan.FromSeconds(Time.deltaTime), token);
            }

            return;
        }
    }
}

#endif
