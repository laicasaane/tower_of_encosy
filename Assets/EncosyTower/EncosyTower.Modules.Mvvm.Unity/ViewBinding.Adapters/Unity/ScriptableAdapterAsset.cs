using EncosyTower.Modules.Unions;
using UnityEngine;

namespace EncosyTower.Modules.Mvvm.ViewBinding.Adapters.Unity
{
    public abstract class ScriptableAdapterAsset : ScriptableObject, IAdapter
    {
        public abstract Union Convert(in Union union);
    }
}
