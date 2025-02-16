using System;
using EncosyTower.Annotations;
using EncosyTower.Unions;
using UnityEngine;

namespace EncosyTower.Mvvm.ViewBinding.Adapters.Unity
{
    [Serializable]
    [Label("Scriptable Adapter", "Default")]
    public sealed class ScriptableAdapter : IAdapter
    {
        [SerializeField, HideInInspector]
        private ScriptableAdapterAsset _asset;

        public Union Convert(in Union union)
        {
            return _asset ? _asset.Convert(union) : union;
        }
    }
}
