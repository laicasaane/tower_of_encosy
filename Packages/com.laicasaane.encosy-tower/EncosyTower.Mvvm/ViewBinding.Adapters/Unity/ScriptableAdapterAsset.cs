using EncosyTower.Variants;
using UnityEngine;

namespace EncosyTower.Mvvm.ViewBinding.Adapters.Unity
{
    public abstract class ScriptableAdapterAsset : ScriptableObject, IAdapter
    {
        public abstract Variant Convert(in Variant variant);
    }
}
