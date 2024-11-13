using EncosyTower.Modules.Unions;
using EncosyTower.Modules.Unions.Converters;
using UnityEngine;

namespace EncosyTower.Modules.Mvvm.ViewBinding.Adapters.Unity
{
    public abstract class ResourcesAdapter<T> : IAdapter
       where T : UnityEngine.Object
    {
        private readonly CachedUnionConverter<T> _converter = new();

        public Union Convert(in Union union)
        {
            if (union.TryGetValue(out string assetPath) == false)
            {
                return union;
            }

            var asset = Resources.Load<T>(assetPath);
            return asset == false ? union : _converter.ToUnionT(asset);
        }
    }
}