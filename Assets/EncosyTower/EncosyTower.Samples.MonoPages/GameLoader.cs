using Cysharp.Threading.Tasks;
using EncosyTower.PageFlows;
using EncosyTower.PageFlows.MonoPages;
using EncosyTower.PubSub;
using UnityEngine;

namespace EncosyTower.Samples.MonoPages
{
    public class GameLoader : MonoBehaviour
    {
        private async void Start()
        {
            await UniTask.WaitUntil(static () => GamePageCodex.IsInitialized);

            var publisher = GlobalMessenger.Publisher.Scope(GamePageCodex.ScreenScope);
            publisher.Publish(new ShowPageAsyncMessage("prefab-screen-red", new PageContext {
                ShowOptions = PageTransitionOptions.NoTransition,
            }));
        }
    }
}
