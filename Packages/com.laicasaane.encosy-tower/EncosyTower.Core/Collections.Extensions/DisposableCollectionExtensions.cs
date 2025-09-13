using System;
using System.Collections.Generic;

namespace EncosyTower.Collections.Extensions
{
    public static class DisposableCollectionExtensions
    {
        public static void Dispose<TCollection>(this TCollection self)
            where TCollection : ICollection<IDisposable>
        {
            if (self == null)
            {
                return;
            }

            foreach (var disposable in self)
            {
                disposable?.Dispose();
            }

            self.Clear();
        }
    }
}
