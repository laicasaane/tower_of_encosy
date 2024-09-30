using Module.Core.Mvvm.ViewBinding;
using Module.Core.Unions;
using UnityEngine;

namespace Module.Core.Extended.Mvvm.ViewBinding.Adapters.Unity
{
    public abstract class ScriptableAdapterAsset : ScriptableObject, IAdapter
    {
        public abstract Union Convert(in Union union);
    }
}
