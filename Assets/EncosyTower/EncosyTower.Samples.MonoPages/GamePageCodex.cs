using System;
using Cysharp.Threading.Tasks;
using EncosyTower.PageFlows;
using EncosyTower.PageFlows.MonoPages;
using UnityEngine;

namespace EncosyTower.Samples.MonoPages
{
    [RequireComponent(typeof(MonoPageCodex))]
    public class GamePageCodex : MonoBehaviour
    {
        public static bool IsInitialized { get; private set; }

        public static PageFlowScope ScreenScope { get; private set; }

        public static PageFlowScope PopupScope { get; private set; }

        public static PageFlowScope FreeTopScope { get; private set; }

        private MonoPageCodex _codex;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void Init()
        {
            IsInitialized = false;
            ScreenScope = default;
            PopupScope = default;
            FreeTopScope = default;
        }

        private async void Awake()
        {
            _codex = GetComponent<MonoPageCodex>();

            await UniTask.WaitUntil(this, static state => state._codex.IsInitialized);

            ScreenScope = GetFlowScope("Screen");
            PopupScope = GetFlowScope("Popup");
            FreeTopScope = GetFlowScope("FreeTop");

            IsInitialized = true;
        }

        private PageFlowScope GetFlowScope(string identifier)
            => _codex.TryGetFlowScope(identifier, out var result)
            ? result
            : throw new InvalidOperationException($"Cannot find PageFlowScope for identifier '{identifier}'");
    }
}
