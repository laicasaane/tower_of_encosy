#if !(UNITASK || UNITY_6000_0_OR_NEWER)

using System;
using System.Threading;
using System.Threading.Tasks;
using EncosyTower.Mvvm.ComponentModel;
using UnityEngine;

namespace EncosyTower.Mvvm.ViewBinding.Components
{
    public partial class MonoView
    {
        private async ValueTask<IObservableObject> GetContextAsync(CancellationToken token)
        {
            await WaitForContextAsync(token);
            return _context.TryGetContext(out var context) ? context : null;

            async ValueTask WaitForContextAsync(CancellationToken token)
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
}

#endif
