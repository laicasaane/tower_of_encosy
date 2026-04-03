using UnityEngine;

namespace EncosyTower.Samples.UserDataVault.SimpleUsage
{
    [RequireComponent(typeof(CanvasGroup))]
    internal class ScreenController : MonoBehaviour
    {
        [SerializeField] private bool _showOnAwake;

        private CanvasGroup _canvasGroup;

        private void Awake()
        {
            _canvasGroup = GetComponent<CanvasGroup>();

            if (_showOnAwake)
            {
                Show();
            }
            else
            {
                Hide();
            }
        }

        public void Show()
        {
            _canvasGroup.alpha = 1f;
            _canvasGroup.interactable = true;
            _canvasGroup.blocksRaycasts = true;
        }

        public void Hide()
        {
            _canvasGroup.alpha = 0f;
            _canvasGroup.interactable = false;
            _canvasGroup.blocksRaycasts = false;
        }
    }
}
