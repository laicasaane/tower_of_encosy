#if UNITASK || UNITY_6000_0_OR_NEWER

using System;
using System.Collections.Generic;
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
        private readonly HashSet<UnityInstanceId<GameObject>> _instanceIds = new();
        private readonly FasterList<MonoPageIdentifier> _pageIds;
        private readonly Transform _poolParent;
        private readonly Transform _activeParent;
        private readonly int _poolLayer;
        private readonly int _activeLayer;

        private Option<MonoPageOptions> _options;

        public MonoPagePool(
              Transform poolParent
            , Transform activeParent
            , FasterList<MonoPageIdentifier> pageIds
            , PooledGameObjectStrategy pooledStrategy
        )
        {
            _poolParent = poolParent;
            _activeParent = activeParent;
            _pageIds = pageIds;
            _poolLayer = poolParent.gameObject.layer;
            _activeLayer = activeParent.gameObject.layer;
            _pool.PooledStrategy = pooledStrategy;
        }

        public bool IsInitialized => _pool.Prefab != null;

        public int PoolingCount => _pool.UnusedCount;

        public void Initialize(GameObject source)
        {
            if (IsInitialized)
            {
                return;
            }

            _pool.Prefab = new GameObjectPrefab {
                Parent = _poolParent,
                Source = source,
                InstantiateInWorldSpace = false,
            };

            _options = source.TryGetComponent<MonoPageOptions>(out var options)
                ? options
                : Option.None;
        }

        public void Prepool(int amount)
        {
            if (_pool.Prepool(amount, GetPooledStrategy()))
            {
                _instanceIds.EnsureCapacity(amount);
                _pageIds.IncreaseCapacityBy(amount);
            }
        }

        public Option<MonoPageIdentifier> Rent(
              StringId keyId
            , string pageAssetKey
            , UnityObjectLogger logger
        )
        {
            var gameObject = _pool.RentGameObject(false);
            var findResult = gameObject.TryGetComponent<IMonoPage>(out var page);

            if (findResult == false)
            {
                UnityEngine.Object.Destroy(gameObject);
                ErrorIfFoundNoPage(pageAssetKey, logger);
                return default;
            }

            if (page is not Component component)
            {
                UnityEngine.Object.Destroy(gameObject);
                ErrorIfPageIsNotComponent(page, pageAssetKey, logger);
                return default;
            }

            gameObject.layer = _activeLayer;

            var transform = component.transform;
            var identifier = gameObject.GetOrAddComponent<MonoPageIdentifier>();

            component.TryGetComponent<CanvasGroup>(out var canvasGroup);

            identifier.KeyId = keyId;
            identifier.AssetKey = pageAssetKey;
            identifier.Transform = transform;
            identifier.GameObject = gameObject;
            identifier.CanvasGroup = canvasGroup;
            identifier.Transform.SetParent(_activeParent, false);
            identifier.GameObjectId = gameObject;
            identifier.Page = page;

            if (_instanceIds.Add(identifier.GameObjectId))
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
            if (_instanceIds.Remove(identifier.GameObjectId))
            {
                _pageIds.Remove(identifier);
            }

            identifier.GameObject.layer = _poolLayer;
            identifier.Transform.SetParent(_poolParent, false);

            _pool.Return(identifier.GameObject, GetPooledStrategy());
        }

        public void Dispose()
        {
            ReturnActiveObjects();

            _pool.ReleaseInstances(0);
            _pool.Dispose();
            _options = Option.None;
        }

        public void Destroy(int amountToDestroy)
        {
            _pool.ReleaseInstances(_pool.UnusedCount - amountToDestroy);
        }

        private void ReturnActiveObjects()
        {
            var length = _instanceIds.Count;
            var array = NativeArray.CreateFast<int>(length, Unity.Collections.Allocator.Temp);
            var index = 0;

            foreach (var instanceId in _instanceIds)
            {
                array[index] = (int)instanceId;
                index++;
            }

            _instanceIds.Clear();
            _pageIds.Clear();
            _pool.ReturnInstanceIds(array);
        }

        private PooledGameObjectStrategy GetPooledStrategy()
            => _options.TryGetValue(out var options)
                ? options.PooledStrategy
                : PooledGameObjectStrategy.Default;

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
