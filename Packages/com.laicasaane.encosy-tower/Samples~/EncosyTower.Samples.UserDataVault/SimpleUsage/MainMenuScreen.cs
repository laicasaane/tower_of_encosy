using System.Collections.Generic;
using System.Threading;
using EncosyTower.PubSub;
using EncosyTower.Samples.UserDataVault.Shared;
using EncosyTower.Samples.UserDataVault.Vaults;
using EncosyTower.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace EncosyTower.Samples.UserDataVault.SimpleUsage
{
#if UNITASK
    using UnityTask = Cysharp.Threading.Tasks.UniTask;
#else
    using UnityTask = UnityEngine.Awaitable;
#endif

    [RequireComponent(typeof(ScreenController))]
    internal class MainMenuScreen : MonoBehaviour
    {
        [SerializeField] private Button _buttonNewGame;
        [SerializeField] private Button _buttonContinue;
        [SerializeField] private Button _buttonOpenSaveFolder;

        private readonly List<ISubscription> _subscriptions = new();

        private ScreenController _screenController;

        private void Awake()
        {
            _screenController = GetComponent<ScreenController>();
            _buttonNewGame.onClick.AddListener(OnNewGameClicked);
            _buttonContinue.onClick.AddListener(OnContinueClicked);
            _buttonOpenSaveFolder.onClick.AddListener(OnOpenSaveFolderClicked);

            var subscriber = GlobalMessenger.Subscriber.Scope<ScreenScope>()
                .WithState(this)
                .WithSubscriptions(_subscriptions);

            subscriber.Subscribe<ShowMainMenuScreenMsg>(Handle);
        }

        private void OnDestroy()
        {
            _subscriptions.Unsubscribe();
        }

        private static void Handle(MainMenuScreen state, ShowMainMenuScreenMsg _)
        {
            state.Init();
            state._screenController.Show();
        }

        private void Init()
        {
            var playerSaveExists = PlayerVault.API.DoesSaveFileExist();
            _buttonContinue.gameObject.SetActive(playerSaveExists);
            _buttonOpenSaveFolder.gameObject.SetActive(playerSaveExists);
        }

        private void OnNewGameClicked()
        {
            EnterLobby(true, destroyCancellationToken).Forget();
        }

        private void OnContinueClicked()
        {
            EnterLobby(false, destroyCancellationToken).Forget();
        }

        private void OnOpenSaveFolderClicked()
        {
            VaultAPI.OpenDeviceSaveFolder();
        }

        private async UnityTask EnterLobby(bool newGame, CancellationToken token)
        {
            var vault = await PlayerVault.InitializeAsync(newGame, token);

            if (newGame)
            {
                Debug.Log($"Player {vault.UserId} has been created.");
                OnPlayerCreated(vault);
            }
            else
            {
                Debug.Log($"Player {vault.UserId} is loaded.");
            }

            _screenController.Hide();

            GlobalMessenger.Publisher.Scope<ScreenScope>()
                .Publish(new ShowLobbyScreenMsg());
        }

        private void OnPlayerCreated(PlayerVault.ReadOnlyVault vault)
        {
            var playerAccess = vault.Accessors.Player;
            playerAccess.AddItemAmount(new ItemId(CurrencyType.Bronze), 15);

            PlayerVault.Save(destroyCancellationToken);
        }
    }
}
