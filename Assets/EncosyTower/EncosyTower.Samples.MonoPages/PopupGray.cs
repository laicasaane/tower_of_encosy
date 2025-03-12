using EncosyTower.PageFlows;
using EncosyTower.PageFlows.MonoPages;
using EncosyTower.PubSub;
using UnityEngine;
using UnityEngine.UI;

namespace EncosyTower.Samples.MonoPages
{
    public class PopupGray : MonoPageBase
    {
        [SerializeField] private Button _buttonOpen;
        [SerializeField] private Button _buttonClose;

        [SerializeField] private PageTransitionOptions _pageToShowOptions;
        [SerializeField] private PageTransitionOptions _pageToHideOptions;

        private void Awake()
        {
            _buttonOpen.onClick.AddListener(OnOpenClick);
            _buttonClose.onClick.AddListener(OnCloseClick);
        }

        private void OnOpenClick()
        {
            var publisher = GlobalMessenger.Publisher.Scope(GamePageCodex.PopupScope);
            publisher.Publish(new ShowPageAsyncMessage("prefab-popup-green", new PageContext {
                ShowOptions = PageTransitionOptions.OnlyFirstPageHasDuration,
                HideOptions = PageTransitionOptions.NoTransition | PageTransitionOptions.ZeroDuration,
            }));
        }

        private void OnCloseClick()
        {
            var publisher = GlobalMessenger.Publisher.Scope(GamePageCodex.PopupScope);
            publisher.Publish(new HideActivePageAsyncMessage(new PageContext {
                ShowOptions = PageTransitionOptions.NoTransition | PageTransitionOptions.ZeroDuration,
                HideOptions = PageTransitionOptions.OnlyFirstPageHasDuration,
            }));
        }
    }
}
