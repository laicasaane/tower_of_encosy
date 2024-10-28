using EncosyTower.Modules.Unions;
using EncosyTower.Modules.Unions.Converters;
using UnityEngine;

namespace EncosyTower.Modules.Mvvm.ViewBinding.Adapters.Unity
{
    public abstract class ResourcesAdapter<T> : IAdapter
       where T : UnityEngine.Object
    {
        private readonly CachedUnionConverter<T> _converter = new CachedUnionConverter<T>();

        public Union Convert(in Union union)
        {
            if (union.TryGetValue(out string assetPath))
            {
                var asset = Resources.Load<T>(assetPath);

                if (asset == false)
                {
                    return union;
                }

                return _converter.ToUnionT(asset);
            }

            return union;
        }
    }
}