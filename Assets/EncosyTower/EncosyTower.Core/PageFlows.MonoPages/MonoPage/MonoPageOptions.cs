#if UNITASK || UNITY_6000_0_OR_NEWER

using UnityEngine;

namespace EncosyTower.PageFlows.MonoPages
{
    public class MonoPageOptions : MonoBehaviour
    {
        [SerializeField] private PageOptions _pageOptions;

        public PageOptions PageOptions
        {
            get => _pageOptions;
            set => _pageOptions = value;
        }
    }
}

#endif
