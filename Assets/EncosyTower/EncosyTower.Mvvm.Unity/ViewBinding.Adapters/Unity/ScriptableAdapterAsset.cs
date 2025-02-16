using EncosyTower.Unions;
using UnityEngine;

namespace EncosyTower.Mvvm.ViewBinding.Adapters.Unity
{
    public abstract class ScriptableAdapterAsset : ScriptableObject, IAdapter
    {
        public abstract Union Convert(in Union union);
    }
}
