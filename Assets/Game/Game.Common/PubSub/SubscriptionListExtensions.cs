using System;
using System.Collections.Generic;
using EncosyTower.Modules.PubSub;

namespace Module.GameCommon.PubSub
{
    public static class SubscriptionListExtensions
    {
        public static void Dispose(this ICollection<IDisposable> items)
        {
            if (items == null)
            {
                return;
            }

            foreach (var disposable in items)
            {
                disposable?.Dispose();
            }

            items.Clear();
        }

        public static void Unsubscribe(this ICollection<ISubscription> items)
        {
            if (items == null)
            {
                return;
            }

            foreach (var disposable in items)
            {
                disposable?.Unsubscribe();
            }

            items.Clear();
        }

        public static void Unsubscribe<TCollection>(this TCollection items)
            where TCollection : ICollection<ISubscription>
        {
            if (items == null)
            {
                return;
            }

            foreach (var disposable in items)
            {
                disposable?.Unsubscribe();
            }

            items.Clear();
        }
    }
}
