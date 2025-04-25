using EncosyTower.PageFlows;
using EncosyTower.PageFlows.MonoPages;
using EncosyTower.PubSub;
using UnityEngine;
using UnityEngine.UI;

namespace EncosyTower.Samples.MonoPages
{
    public class ScreenBlue : MonoPageBase
    {
        [SerializeField] private Button _buttonOpenScreen;
        [SerializeField] private Button _buttonOpenPopup;
        [SerializeField] private Button _buttonCloseScreen;

        private void Awake()
        {
            _buttonOpenScreen.onClick.AddListener(OnOpenSceneClick);
            _buttonOpenPopup.onClick.AddListener(OnOpenPopupClick);
            _buttonCloseScreen.onClick.AddListener(OnCloseScreenClick);
        }

        private void OnOpenSceneClick()
        {
            var publisher = GlobalMessenger.Publisher.Scope(GamePageCodex.ScreenScope);
            publisher.Publish(new ShowPageAsyncMessage("prefab-screen-red", default));
        }

        private void OnOpenPopupClick()
        {
            var publisher = GlobalMessenger.Publisher.Scope(GamePageCodex.PopupScope);
            publisher.Publish(new ShowPageAsyncMessage("prefab-popup-green", new PageContext {
                ShowOptions = PageTransitionOptions.OnlyFirstPageHasDuration,
                HideOptions = PageTransitionOptions.NoTransition,
            }));
        }

        private void OnCloseScreenClick()
        {
            var publisher = GlobalMessenger.Publisher.Scope(GamePageCodex.ScreenScope);
            publisher.Publish(new HideActivePageAsyncMessage(default));
        }
    }
}
