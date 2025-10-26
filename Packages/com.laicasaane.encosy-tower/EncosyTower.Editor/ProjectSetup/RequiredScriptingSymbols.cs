#if UNITY_EDITOR

using EncosyTower.Logging;
using EncosyTower.UnityExtensions;
using UnityEngine;

namespace EncosyTower.Editor.ProjectSetup
{
    [ExecuteInEditMode]
    internal sealed class RequiredScriptingSymbols : MonoBehaviour
    {
        [SerializeField] internal string[] _symbols;

        private void Awake()
        {
            gameObject.tag = "EditorOnly";
            gameObject.hideFlags = HideFlags.DontSaveInBuild;
        }

        private void Update()
        {
            if (transform.parent.IsValid())
            {
                transform.SetParent(null, false);

                StaticDevLogger.LogError(
                    this, $"{nameof(RequiredScriptingSymbols)} cannot be a child of any GameObject."
                );
            }

            if (transform.childCount > 0)
            {
                transform.DetachChildren();

                StaticDevLogger.LogError(
                    this, $"{nameof(RequiredScriptingSymbols)} cannot have any child GameObject."
                );
            }
        }
    }
}

#endif
