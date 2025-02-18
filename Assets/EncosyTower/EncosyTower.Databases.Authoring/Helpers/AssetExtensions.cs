using System;
using System.Collections.Generic;
using EncosyTower.Databases;
using UnityEngine;

namespace EncosyTower.Databases.Authoring
{
    using LazyTableLoadReference = LazyLoadReference<DataTableAssetBase>;

    public static class AssetExtensions
    {
        public static void AddRange<TDatabaseAsset>(
              this TDatabaseAsset target
            , DataTableAssetBase[] assets
        )
            where TDatabaseAsset : DatabaseAsset
        {
            target._assetRefs = assets.ToRefArray();
        }

        public static void AddRange<TDatabaseAsset>(
              this TDatabaseAsset target
            , DataTableAssetBase[] assets
            , DataTableAssetBase[] redundantAssets
        )
            where TDatabaseAsset : DatabaseAsset
        {
            target._assetRefs = assets.ToRefArray();
            target._redundantAssetRefs = redundantAssets.ToRefArray();
        }

        public static void Clear<TDatabaseAsset>(this TDatabaseAsset target)
            where TDatabaseAsset : DatabaseAsset
        {
            target._assetRefs = new LazyTableLoadReference[0];
            target._redundantAssetRefs = new LazyTableLoadReference[0];
        }

        public static LazyTableLoadReference[] ToRefArray(this DataTableAssetBase[] assets)
        {
            var span = assets.AsSpan();
            var list = new List<LazyTableLoadReference>(span.Length);

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
