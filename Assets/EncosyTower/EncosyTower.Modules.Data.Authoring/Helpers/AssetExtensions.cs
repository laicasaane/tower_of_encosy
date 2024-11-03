using System;
using System.Collections.Generic;
using UnityEngine;

namespace EncosyTower.Modules.Data.Authoring
{
    using TableAssetRef = LazyLoadReference<DataTableAsset>;

    public static class AssetExtensions
    {
        public static void AddRange<TDatabaseAsset>(
              this TDatabaseAsset target
            , DataTableAsset[] assets
        )
            where TDatabaseAsset : DatabaseAsset
        {
            target._assetRefs = assets.ToRefArray();
        }

        public static void AddRange<TDatabaseAsset>(
              this TDatabaseAsset target
            , DataTableAsset[] assets
            , DataTableAsset[] redundantAssets
        )
            where TDatabaseAsset : DatabaseAsset
        {
            target._assetRefs = assets.ToRefArray();
            target._redundantAssetRefs = redundantAssets.ToRefArray();
        }

        public static void Clear<TDatabaseAsset>(this TDatabaseAsset target)
            where TDatabaseAsset : DatabaseAsset
        {
            target._assetRefs = Array.Empty<TableAssetRef>();
            target._redundantAssetRefs = Array.Empty<TableAssetRef>();
        }

        public static TableAssetRef[] ToRefArray(this DataTableAsset[] assets)
        {
            var span = assets.AsSpan();
            var list = new List<TableAssetRef>(span.Length);

            foreach (var asset in span)
            {
                if (asset)
                {
                    list.Add(asset);
                }
            }

            return list.ToArray();
        }
    }
}
