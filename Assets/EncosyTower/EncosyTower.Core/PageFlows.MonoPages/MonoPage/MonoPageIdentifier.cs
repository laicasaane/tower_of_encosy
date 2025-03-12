#if UNITASK || UNITY_6000_0_OR_NEWER

using System.Diagnostics.CodeAnalysis;
using EncosyTower.StringIds;
using EncosyTower.UnityExtensions;
using UnityEngine;

namespace EncosyTower.PageFlows.MonoPages
{
    public class MonoPageIdentifier : MonoBehaviour
    {
        public string AssetKey { get; internal set; }

        public StringId KeyId { get; internal set; }

        public UnityInstanceId<GameObject> GameObjectId { get; internal set; }

        [NotNull]
        public Transform Transform { get; internal set; }

        [NotNull]
        public GameObject GameObject { get; internal set; }

        [AllowNull]
        public CanvasGroup CanvasGroup { get; internal set; }

        public IMonoPage Page { get; internal set; }
    }
}

#endif
