#if UNITASK || UNITY_6000_0_OR_NEWER

using EncosyTower.Pooling;
using UnityEngine;

namespace EncosyTower.PageFlows.MonoPages
{
    public class MonoPageOptions : MonoBehaviour
    {
        [SerializeField] private RentingStrategy _rentingStrategy;
        [SerializeField] private ReturningStrategy _returningStrategy;
        [SerializeField] private PageOptions _pageOptions;

        public RentingStrategy RentingStrategy
        {
            get => _rentingStrategy;
            set => _rentingStrategy = value;
        }

        public ReturningStrategy ReturningStrategy
        {
            get => _returningStrategy;
            set => _returningStrategy = value;
        }

        public PageOptions PageOptions
        {
            get => _pageOptions;
            set => _pageOptions = value;
        }
    }
}

#endif
