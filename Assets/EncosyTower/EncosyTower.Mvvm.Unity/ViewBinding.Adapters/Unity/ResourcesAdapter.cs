using EncosyTower.Unions;
using EncosyTower.Unions.Converters;
using UnityEngine;

namespace EncosyTower.Mvvm.ViewBinding.Adapters.Unity
{
    public abstract class ResourcesAdapter<T> : IAdapter
       where T : UnityEngine.Object
    {
        private readonly CachedUnionConverter<T> _converter;

        protected ResourcesAdapter(CachedUnionConverter<T> converter)
        {
            _converter = converter;
        }

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
