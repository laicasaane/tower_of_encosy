#if UNITASK || UNITY_6000_0_OR_NEWER

using EncosyTower.UnityExtensions;
using UnityEngine;

namespace EncosyTower.PageFlows.MonoPages
{
    public abstract class MonoPageBase : MonoBehaviour
        , IMonoPage
        , IPageHasOptions
        , IPageHasTransition
    {
        [SerializeField] private PageOptions _pageOptions;

        private IPageTransition _transition;

        public PageOptions PageOptions
        {
            get => _pageOptions;
            set => _pageOptions = value;
        }

        public IPageTransition PageTransition
        {
            get
            {
                if (_transition is null)
                {
                    var transitions = GetComponents<MonoPageTransition>();

                    if (transitions.Length > 1)
                    {
                        _transition = this.GetOrAddComponent<MonoPageTransitionCollection>();
                    }
                    else if (transitions.Length == 1)
                    {
                        _transition = transitions[0];
                    }
                }

                return _transition;
            }
        }
    }
}

#endif
