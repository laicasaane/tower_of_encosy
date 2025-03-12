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
        private static MonoPageCodex.FlowScopeRecord s_screenRecord;
        private static MonoPageCodex.FlowScopeRecord s_popupRecord;
        private static MonoPageCodex.FlowScopeRecord s_freeTopRecord;

        private MonoPageCodex _codex;

        public static bool IsInitialized { get; private set; }

        public static PageFlowScope ScreenScope => s_screenRecord.Scope;

        public static PageFlowScope PopupScope => s_popupRecord.Scope;

        public static PageFlowScope FreeTopScope => s_freeTopRecord.Scope;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void Init()
        {
            IsInitialized = false;
            s_screenRecord = default;
            s_popupRecord = default;
            s_freeTopRecord = default;
        }

        private async void Awake()
        {
            _codex = GetComponent<MonoPageCodex>();

            await UniTask.WaitUntil(this, static state => state._codex.IsInitialized);

            s_screenRecord = GetFlowScopeRecord("Screen");
            s_popupRecord = GetFlowScopeRecord("Popup");
            s_freeTopRecord = GetFlowScopeRecord("FreeTop");

            IsInitialized = true;
        }

        private MonoPageCodex.FlowScopeRecord GetFlowScopeRecord(string identifier)
            => _codex.TryGetFlowScopeRecord(identifier, out var result)
            ? result
            : throw new InvalidOperationException($"Cannot find PageFlowScope for identifier '{identifier}'");
    }
}
