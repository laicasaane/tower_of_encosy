using Module.Core.Mvvm.ViewBinding;
using Module.Core.Unions;
using Module.Core.Unions.Converters;
using UnityEngine;

namespace Module.Core.Extended.Mvvm.ViewBinding.Adapters.Unity
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