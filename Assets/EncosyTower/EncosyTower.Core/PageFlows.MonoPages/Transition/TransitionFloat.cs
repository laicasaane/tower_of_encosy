#if UNITASK || UNITY_6000_0_OR_NEWER

using System;

namespace EncosyTower.PageFlows.MonoPages
{
    [Serializable]
    public struct TransitionFloat
    {
        public float start;
        public float end;

        public TransitionFloat(float start, float end)
        {
            this.start = start;
            this.end = end;
        }
    }
}

#endif
