using EncosyTower.Variants;
using EncosyTower.Variants.Converters;
using UnityEngine;

namespace EncosyTower.Mvvm.ViewBinding.Adapters.Unity
{
    public abstract class ResourcesAdapter<T> : IAdapter
       where T : UnityEngine.Object
    {
        private readonly CachedVariantConverter<T> _converter;

        protected ResourcesAdapter(CachedVariantConverter<T> converter)
        {
            _converter = converter;
        }

        public Variant Convert(in Variant variant)
        {
            if (variant.TryGetValue(out string assetPath) == false)
            {
                return variant;
            }

            var asset = Resources.Load<T>(assetPath);
            return asset == false ? variant : _converter.ToVariantT(asset);
        }
    }
}
