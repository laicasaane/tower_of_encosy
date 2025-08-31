using EncosyTower.PageFlows;
using EncosyTower.PageFlows.MonoPages;
using UnityEngine;
using UnityEngine.UI;

namespace EncosyTower.Samples.MonoPages
{
    public class PopupGray : MonoPageBase<GamePageFlowScopes>
    {
        [SerializeField] private Button _buttonOpen;
        [SerializeField] private Button _buttonClose;

        private void Awake()
        {
            _buttonOpen.onClick.AddListener(OnOpenClick);
            _buttonClose.onClick.AddListener(OnCloseClick);
        }

        private void OnOpenClick()
        {
            if (FlowScopeCollection.TryGetValue(out var scopes) == false)
            {
                return;
            }

            var publisher = Publisher.Scope(scopes.Popup);
            publisher.Publish(new ShowPageAsyncMessage("prefab-popup-green", new PageContext()));
        }

        private void OnCloseClick()
        {
            if (FlowScopeCollection.TryGetValue(out var scopes) == false)
            {
                return;
            }

            var publisher = Publisher.Scope(scopes.Popup);
            publisher.Publish(new HideActivePageAsyncMessage(new PageContext()));
        }
    }
}
