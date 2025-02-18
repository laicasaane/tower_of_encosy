#pragma warning disable CA1040 // Avoid empty interfaces

using UnityEngine;

namespace EncosyTower.Databases
{
    public abstract class DataTableAssetBase : ScriptableObject, IDataTableAsset
    {
        internal abstract void SetEntries(object obj);

        internal protected virtual void Initialize() { }

        internal protected virtual void Deinitialize() { }
    }
}
