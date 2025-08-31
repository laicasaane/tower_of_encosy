using EncosyTower.PageFlows;
using EncosyTower.PageFlows.MonoPages;
using UnityEngine;
using UnityEngine.UI;

namespace EncosyTower.Samples.MonoPages
{
    public class PopupGreen : MonoPageBase<GamePageFlowScopes>
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

        }

        private void OnCloseClick()
        {
            if (FlowScopes.TryGetValue(out var scopes) == false)
            {
                return;
            }

            var publisher = Publisher.Scope(scopes.Popup);
            publisher.Publish(new HideActivePageAsyncMessage(new PageContext()));
        }
    }
}
