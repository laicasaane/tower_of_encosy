using Module.Core.Unions;
using UnityEngine;

namespace Module.Core.Mvvm.ViewBinding.Adapters.Unity
{
    public abstract class ScriptableAdapterAsset : ScriptableObject, IAdapter
    {
        public abstract Union Convert(in Union union);
    }
}
