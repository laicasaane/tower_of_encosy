using EncosyTower.PageFlows;
using EncosyTower.PageFlows.MonoPages;
using UnityEngine;
using UnityEngine.UI;

namespace EncosyTower.Samples.MonoPages
{
    public class ScreenBlue : MonoPageBase<GamePageFlowScopes>
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
            if (FlowScopes.TryGetValue(out var scopes) == false)
            {
                return;
            }

            var publisher = Publisher.Scope(scopes.Screen);
            publisher.Publish(new ShowPageAsyncMessage("prefab-screen-red", default));
        }

        private void OnOpenPopupClick()
        {
            if (FlowScopes.TryGetValue(out var scopes) == false)
            {
                return;
            }

            var publisher = Publisher.Scope(scopes.Popup);
            publisher.Publish(new ShowPageAsyncMessage("prefab-popup-green", new PageContext {
                ShowOptions = PageTransitionOptions.OnlyFirstPageHasDuration,
                HideOptions = PageTransitionOptions.NoTransition,
            }));
        }

        private void OnCloseScreenClick()
        {
            if (FlowScopes.TryGetValue(out var scopes) == false)
            {
                return;
            }

            var publisher = Publisher.Scope(scopes.Screen);
            publisher.Publish(new HideActivePageAsyncMessage(default));
        }
    }
}
