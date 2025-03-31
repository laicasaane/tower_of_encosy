using System;
using EncosyTower.PageFlows;
using EncosyTower.PageFlows.MonoPages;
using EncosyTower.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace EncosyTower.VisualCommands
{
    [RequireComponent(typeof(UIDocument))]
    public partial class VisualCommanderPage : MonoPageBase, IPageAfterShow, IPageBeforeHide
    {
        public event Action Close;

        [SerializeField] private float _directoryListWidth = 200f;
        [SerializeField] private bool _showOnAwake;
        [SerializeField] private bool _disableDocumentOnClose;

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
            if (_disableDocumentOnClose)
            {
                ToggleDocument(true);
            }

            _view = VisualCommanderAPI.CreateView(_document.rootVisualElement, _directoryListWidth);
            _view.OnClose += OnClose;
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

        private void OnClose()
        {
            if (_disableDocumentOnClose)
            {
                ToggleDocument(false);
            }

            Close?.Invoke();
        }

        private void ToggleDocument(bool value)
        {
            _document.rootVisualElement?.SetDisplay(value ? DisplayStyle.Flex : DisplayStyle.None);
        }
    }
}
