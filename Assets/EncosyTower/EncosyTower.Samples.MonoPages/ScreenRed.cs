using EncosyTower.PageFlows;
using EncosyTower.PageFlows.MonoPages;
using EncosyTower.PubSub;
using UnityEngine;
using UnityEngine.UI;

namespace EncosyTower.Samples.MonoPages
{
    public class ScreenRed : MonoPageBase<GamePageFlowScopes>
    {
        [SerializeField] private Button _buttonOpenScreen;
        [SerializeField] private Button _buttonOpenPopup;

        private void Awake()
        {
            _buttonOpenScreen.onClick.AddListener(OnOpenScreenClick);
            _buttonOpenPopup.onClick.AddListener(OnOpenPopupClick);
        }

        private void OnOpenScreenClick()
        {
            if (FlowScopes.TryGetValue(out var scopes) == false)
            {
                return;
            }

            var publisher = GlobalMessenger.Publisher.Scope(scopes.Screen);
            publisher.Publish(new ShowPageAsyncMessage("prefab-screen-blue", default));
        }

        private void OnOpenPopupClick()
        {
            if (FlowScopes.TryGetValue(out var scopes) == false)
            {
                return;
            }

            var publisher = GlobalMessenger.Publisher.Scope(scopes.Popup);
            publisher.Publish(new ShowPageAsyncMessage("prefab-popup-gray", new PageContext()));
        }
    }
}
