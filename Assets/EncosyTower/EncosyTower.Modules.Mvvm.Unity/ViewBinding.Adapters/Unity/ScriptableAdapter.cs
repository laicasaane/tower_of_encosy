using System;
using EncosyTower.Modules.Unions;
using UnityEngine;

namespace EncosyTower.Modules.Mvvm.ViewBinding.Adapters.Unity
{
    [Serializable]
    [Label("Scriptable Adapter", "Default")]
    public sealed class ScriptableAdapter : IAdapter
    {
        [SerializeField, HideInInspector]
        private ScriptableAdapterAsset _asset;

        public Union Convert(in Union union)
        {
            if (_asset)
            {
                return _asset.Convert(union);
            }

            return union;
        }
    }
}