#if UNITASK || UNITY_6000_0_OR_NEWER

using EncosyTower.Pooling;
using UnityEngine;

namespace EncosyTower.PageFlows.MonoPages
{
    public class MonoPageOptions : MonoBehaviour
    {
        [SerializeField] private PooledGameObjectStrategy _pooledStrategy;
        [SerializeField] private PageOptions _pageOptions;

        public PooledGameObjectStrategy PooledStrategy
        {
            get => _pooledStrategy;
            set => _pooledStrategy = value;
        }

        public PageOptions PageOptions
        {
            get => _pageOptions;
            set => _pageOptions = value;
        }
    }
}

#endif
