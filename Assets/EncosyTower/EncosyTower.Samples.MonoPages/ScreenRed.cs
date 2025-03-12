using EncosyTower.PageFlows;
using EncosyTower.PageFlows.MonoPages;
using EncosyTower.PubSub;
using UnityEngine;
using UnityEngine.UI;

namespace EncosyTower.Samples.MonoPages
{
    public class ScreenRed : MonoPageBase
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
            var publisher = GlobalMessenger.Publisher.Scope(GamePageCodex.ScreenScope);
            publisher.Publish(new ShowPageAsyncMessage("prefab-screen-blue", default));
        }

        private void OnOpenPopupClick()
        {
            var publisher = GlobalMessenger.Publisher.Scope(GamePageCodex.PopupScope);
            publisher.Publish(new ShowPageAsyncMessage("prefab-popup-gray", new PageContext {
                ShowOptions = PageTransitionOptions.OnlyFirstPageHasDuration,
                HideOptions = PageTransitionOptions.NoTransition | PageTransitionOptions.ZeroDuration,
            }));
        }
    }
}
