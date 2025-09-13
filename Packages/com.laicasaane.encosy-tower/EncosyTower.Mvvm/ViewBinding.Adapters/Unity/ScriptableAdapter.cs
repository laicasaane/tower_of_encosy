using System;
using EncosyTower.Annotations;
using EncosyTower.Variants;
using UnityEngine;

namespace EncosyTower.Mvvm.ViewBinding.Adapters.Unity
{
    [Serializable]
    [Label("Scriptable Adapter", "Default")]
    public sealed class ScriptableAdapter : IAdapter
    {
        [SerializeField, HideInInspector]
        private ScriptableAdapterAsset _asset;

        public Variant Convert(in Variant variant)
        {
            return _asset ? _asset.Convert(variant) : variant;
        }
    }
}
