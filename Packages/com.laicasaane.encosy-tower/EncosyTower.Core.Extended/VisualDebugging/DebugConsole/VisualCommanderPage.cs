using EncosyTower.PageFlows;
using EncosyTower.PageFlows.MonoPages;
using UnityEngine;
using UnityEngine.UIElements;

namespace EncosyTower.VisualDebugging.Commands
{
    [RequireComponent(typeof(UIDocument))]
    public partial class VisualCommanderPage : MonoPageBase, IPageOnAfterShow, IPageOnBeforeHide
    {
        [SerializeField] private float _directoryListWidth = 200f;
        [SerializeField] private bool _showOnAwake;

        private UIDocument _document;
        private VisualCommanderView _view;

        private void Awake()
        {
            _document = GetComponent<UIDocument>();

            if (_showOnAwake)
            {
                OnAfterShow(default);
            }
        }

        private void OnDestroy()
        {
            OnBeforeHide(default);
        }

        public void OnAfterShow(PageContext context)
        {
            _view = VisualCommanderAPI.CreateView(_document.rootVisualElement, _directoryListWidth);
        }

        public void OnBeforeHide(PageContext context)
        {
            if (_view?.userData is VisualCommanderViewController controller)
            {
                controller.Dispose();
            }

            _view = null;
            _document.rootVisualElement?.Clear();
        }
    }
}
