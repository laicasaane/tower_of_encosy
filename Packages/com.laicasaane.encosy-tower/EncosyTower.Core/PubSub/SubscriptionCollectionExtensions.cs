using System.Collections.Generic;

namespace EncosyTower.PubSub
{
    public static class SubscriptionCollectionExtensions
    {
        public static void Unsubscribe<TCollection>(this TCollection self)
            where TCollection : ICollection<ISubscription>
        {
            if (self == null)
            {
                return;
            }

            foreach (var disposable in self)
            {
                disposable?.Unsubscribe();
            }

            self.Clear();
        }
    }
}
