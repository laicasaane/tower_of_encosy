using System.Collections.Generic;
using EncosyTower.PubSub;
using EncosyTower.Samples.UserDataVault.Shared;
using EncosyTower.Samples.UserDataVault.Vaults;
using UnityEngine;
using UnityEngine.UI;

namespace EncosyTower.Samples.UserDataVault.SimpleUsage
{
    [RequireComponent(typeof(ScreenController))]
    internal class LobbyScreen : MonoBehaviour
    {
        [SerializeField] private ItemCurrencyController _currencyGold;
        [SerializeField] private ItemCurrencyController _currencySilver;
        [SerializeField] private ItemCurrencyController _currencyBronze;
        [SerializeField] private Button _buttonSave;
        [SerializeField] private Button _buttonOpenSaveFolder;
        [SerializeField] private Button _buttonExit;

        private readonly List<ISubscription> _subscriptions = new();

        private ScreenController _screenController;

        private void Awake()
        {
            _screenController = GetComponent<ScreenController>();
            _buttonSave.onClick.AddListener(OnSaveClicked);
            _buttonOpenSaveFolder.onClick.AddListener(OnOpenSaveFolderClicked);
            _buttonExit.onClick.AddListener(OnExitClicked);

            var subscriber = GlobalMessenger.Subscriber.Scope<ScreenScope>()
                .WithState(this)
                .WithSubscriptions(_subscriptions);

            subscriber.Subscribe<ShowLobbyScreenMsg>(Handle);
        }

        private void OnDestroy()
        {
            _subscriptions.Unsubscribe();
        }

        private static void Handle(LobbyScreen state, ShowLobbyScreenMsg _)
        {
            var playerAccess = PlayerVault.Accessors.Player;

            state._currencyGold.Value = playerAccess.GetItemAmount(CurrencyType.Gold).GetValueOrDefault();
            state._currencySilver.Value = playerAccess.GetItemAmount(CurrencyType.Silver).GetValueOrDefault();
            state._currencyBronze.Value = playerAccess.GetItemAmount(CurrencyType.Bronze).GetValueOrDefault();

            state._screenController.Show();
        }

        private void OnSaveClicked()
        {
            var playerAccess = PlayerVault.Accessors.Player;
            playerAccess.SetItemAmount(CurrencyType.Gold, _currencyGold.Value).AssertError(log: true);
            playerAccess.SetItemAmount(CurrencyType.Silver, _currencySilver.Value).AssertError(log: true);
            playerAccess.SetItemAmount(CurrencyType.Bronze, _currencyBronze.Value).AssertError(log: true);

            PlayerVault.Save(destroyCancellationToken);
        }

        private void OnOpenSaveFolderClicked()
        {
            VaultAPI.OpenDeviceSaveFolder();
        }

        private void OnExitClicked()
        {
            _screenController.Hide();

            GlobalMessenger.Publisher.Scope<ScreenScope>()
                .Publish(new ShowMainMenuScreenMsg());
        }
    }
}
