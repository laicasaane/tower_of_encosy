#pragma warning disable CA1040 // Avoid empty interfaces

using UnityEngine;

namespace EncosyTower.Modules.Data
{
    public interface IDataTableAsset { }

    public abstract class DataTableAsset : ScriptableObject, IDataTableAsset
    {
        internal abstract void SetEntries(object obj);

        internal protected virtual void Initialize() { }

        internal protected virtual void Deinitialize() { }
    }
}
