using EncosyTower.PubSub;
using UnityEngine;

namespace EncosyTower.Samples.UserDataVault.SimpleUsage
{
    internal class GameManager : MonoBehaviour
    {
        private void Start()
        {
            GlobalMessenger.Publisher.Scope<ScreenScope>()
                .Publish(new ShowMainMenuScreenMsg());
        }
    }
}
