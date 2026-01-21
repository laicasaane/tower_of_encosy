#if UNITASK || UNITY_6000_0_OR_NEWER

using System.Collections.Generic;

namespace EncosyTower.Processing
{
    public static class RegistryCollectionExtensions
    {
        public static void Unregister<TCollection>(this TCollection self)
            where TCollection : ICollection<ProcessRegistry>
        {
            if (self == null)
            {
                return;
            }

            foreach (var registry in self)
            {
                registry.Unregister();
            }

            self.Clear();
        }
    }
}

#endif
