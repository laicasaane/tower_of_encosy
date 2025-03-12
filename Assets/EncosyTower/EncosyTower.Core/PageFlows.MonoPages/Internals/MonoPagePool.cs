#if UNITASK || UNITY_6000_0_OR_NEWER

using System;
using EncosyTower.Collections;
using EncosyTower.Common;
using EncosyTower.Logging;
using EncosyTower.Pooling;
using EncosyTower.StringIds;
using EncosyTower.UnityExtensions;
using UnityEngine;

namespace EncosyTower.PageFlows.MonoPages
{
    internal class MonoPagePool : IDisposable
    {
        private readonly GameObjectPool _pool = new();
        private readonly ArrayMap<UnityInstanceId<GameObject>, int> _idToPageIndex = new();
        private readonly FasterList<MonoPageIdentifier> _pageIds;

        public MonoPagePool(Transform poolParent, Transform activeParent, FasterList<MonoPageIdentifier> pageIds)
        {
            PoolParent = poolParent;
            ActiveParent = activeParent;
            _pageIds = pageIds;
        }

        public Transform PoolParent { get; }

        public Transform ActiveParent { get; }

        public bool IsInitialized => _pool.Prefab != null;

        public int PoolingCount => _pool.UnusedCount;

        public void Initialize(GameObject source)
        {
            if (IsInitialized)
            {
                return;
            }

            _pool.Prefab = new GameObjectPrefab {
                Parent = PoolParent,
                Source = source,
                InstantiateInWorldSpace = false,
            };
        }

        public void Prepool(int amount)
        {
            if (_pool.Prepool(amount))
            {
                _idToPageIndex.IncreaseCapacityBy(amount);
                _pageIds.IncreaseCapacityBy(amount);
            }
        }

        public Option<MonoPageIdentifier> Rent(
              StringId keyId
            , string pageAssetKey
            , UnityObjectLogger logger
        )
        {
            var obj = _pool.RentGameObject(false);
            var findResult = obj.TryGetComponent<IMonoPage>(out var page);

            if (findResult == false)
            {
                UnityEngine.Object.Destroy(obj);
                ErrorIfFoundNoPage(pageAssetKey, logger);
                return default;
            }

            if (page is not Component component)
            {
                UnityEngine.Object.Destroy(obj);
                ErrorIfPageIsNotComponent(page, pageAssetKey, logger);
                return default;
            }

            var gameObject = component.gameObject;
            var transform = component.transform;
            var identifier = gameObject.GetOrAddComponent<MonoPageIdentifier>();

            component.TryGetComponent<CanvasGroup>(out var canvasGroup);

            identifier.KeyId = keyId;
            identifier.AssetKey = pageAssetKey;
            identifier.Transform = transform;
            identifier.GameObject = gameObject;
            identifier.CanvasGroup = canvasGroup;
            identifier.Transform.SetParent(ActiveParent, false);
            identifier.GameObjectId = gameObject;
            identifier.Page = page;

            if (_idToPageIndex.TryAdd(identifier.GameObjectId, _pageIds.Count))
            {
                _pageIds.Add(identifier);
            }
            else
            {
                WarningIfAlreadyInActive(page, pageAssetKey, logger);
            }

            return identifier;
        }

        public void Return(MonoPageIdentifier identifier)
        {
            if (_idToPageIndex.Remove(identifier.GameObjectId, out _, out var index))
            {
                _pageIds.RemoveAt(index);
            }

            identifier.Transform.SetParent(_pool.Prefab.Parent, false);
            _pool.Return(identifier.GameObject);
        }

        public void Dispose()
        {
            ReturnActives();
            _pool.ReleaseInstances(0);
            _pool.Dispose();
        }

        public void Destroy(int amountToDestroy)
        {
            _pool.ReleaseInstances(_pool.UnusedCount - amountToDestroy);
        }

        private void ReturnActives()
        {
            var length = _idToPageIndex.Count;
            var array = NativeArray.CreateFast<int>(length, Unity.Collections.Allocator.Temp);
            var index = 0;

            foreach (var instanceId in _idToPageIndex.Keys)
            {
                array[index] = (int)instanceId;
                index++;
            }

            _idToPageIndex.Clear();
            _pageIds.Clear();
            _pool.ReturnInstanceIds(array);
        }

        [HideInCallstack]
        private static void ErrorIfFoundNoPage(string key, UnityObjectLogger logger)
        {
            logger.LogError(
                $"Cannot find any page component derived from '{nameof(IMonoPage)}' on the asset '{key}'."
            );
        }

        [HideInCallstack]
        private static void ErrorIfPageIsNotComponent(IMonoPage page, string key, UnityObjectLogger logger)
        {
            logger.LogError(
                $"The page '{page.GetType()}' on the asset '{key}' is not derived from 'UnityEngine.Component'."
            );
        }

        [HideInCallstack]
        private static void WarningIfAlreadyInActive(IMonoPage page, string assetKey, UnityObjectLogger logger)
        {
            logger.LogWarning(
                $"The page '{page.GetType()}' on the asset '{assetKey}' is already in active. " +
                $"This might indicate an unexpected bug in the pooling mechanism."
            );
        }
    }
}

#endif
