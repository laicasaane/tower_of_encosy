using EncosyTower.Settings;
using EncosyTower.Variants;
using UnityEngine;

namespace EncosyTower.Editor.Variants.Settings
{
    [Settings(SettingsUsage.EditorProject, "Encosy Tower/Variant Type")]
    public sealed class VariantTypeSettings : Settings<VariantTypeSettings>
    {
        public const string LONG_SYMBOL_FORMAT = "VARIANT_{0}_LONGS";
        public const string INT_SYMBOL_FORMAT = "VARIANT_{0}_INTS";
        public const string BYTE_SYMBOL_FORMAT = "VARIANT_{0}_BYTES";

        [SerializeField] internal int _variantSize = VariantData.DEFAULT_LONG_COUNT;

        public void ApplyVariantSize()
        {
            var currLongCount = VariantData.BYTE_COUNT / VariantData.SIZE_OF_LONG;
            var currIntCount = currLongCount * 2;
            var currByteCount = currLongCount * 8;

            var minLongCount = VariantData.DEFAULT_LONG_COUNT;
            var maxLongCount = VariantData.MAX_LONG_COUNT;
            var nextLongCount = Mathf.Clamp(_variantSize, minLongCount, maxLongCount);

            var currLongSymbol = string.Format(LONG_SYMBOL_FORMAT, currLongCount);
            var currIntSymbol = string.Format(INT_SYMBOL_FORMAT, currIntCount);
            var currByteSymbol = string.Format(BYTE_SYMBOL_FORMAT, currByteCount);

            var nextLongSymbol = string.Format(LONG_SYMBOL_FORMAT, nextLongCount);

            UserBuildAPI.RemoveScriptingDefineSymbols(
                  UserBuildAPI.CurrentBuildTarget
                , currLongSymbol
                , currIntSymbol
                , currByteSymbol
            );

            if (nextLongCount > minLongCount)
            {
                UserBuildAPI.AddScriptingDefineSymbols(
                      UserBuildAPI.CurrentBuildTarget
                    , nextLongSymbol
                );
            }
        }
    }
}
