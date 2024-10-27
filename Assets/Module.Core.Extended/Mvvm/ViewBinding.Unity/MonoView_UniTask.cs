#if UNITASK

using System.Threading;
using Cysharp.Threading.Tasks;
using Module.Core.Mvvm.ComponentModel;

namespace Module.Core.Extended.Mvvm.ViewBinding.Unity
{
    public partial class MonoView
    {
        public async UniTask InitializeAsync(bool alsoStartListening, CancellationToken token = default)
        {
            var context = await GetContextAsync(token);
            InitializeInternal(context, alsoStartListening);
            Initialized = true;
        }

        private async UniTask<IObservableObject> GetContextAsync(CancellationToken token)
        {
            await UniTask.WaitUntil(
                  this
                , static state => state._context != null && state._context.TryGetContext(out _)
                , cancellationToken: token
            );

            return _context.TryGetContext(out var context) ? context : null;
        }
    }
}

#endif
