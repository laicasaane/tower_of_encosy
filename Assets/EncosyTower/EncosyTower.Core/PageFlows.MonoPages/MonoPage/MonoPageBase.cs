#if UNITASK || UNITY_6000_0_OR_NEWER

using EncosyTower.Collections;
using UnityEngine;

namespace EncosyTower.PageFlows.MonoPages
{
    public abstract class MonoPageBase : MonoBehaviour, IMonoPage, ITryGet<IPageTransition>
    {
        public bool TryGet(out IPageTransition result)
        {
            if (TryGetComponent<MonoPageTransitionCollection>(out var collection))
            {
                result = collection;
                return true;
            }

            if (TryGetComponent<MonoPageTransition>(out var transition))
            {
                result = transition;
                return true;
            }

            result = default;
            return false;
        }
    }
}

#endif
