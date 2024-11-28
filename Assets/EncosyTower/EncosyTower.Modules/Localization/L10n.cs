#if UNITY_LOCALIZATION

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using EncosyTower.Modules.Logging;
using EncosyTower.Modules.Vaults;
using Unity.Logging;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

namespace EncosyTower.Modules.Localization
{
    // ReSharper disable once InconsistentNaming

    /// <summary>
    /// L10n = L(ocalizatio)n
    /// </summary>
    public static partial class L10n
    {
        public static readonly Id<Localization> PresetId = TypeId<Localization>.Value;

        private static readonly Dictionary<string, Locale> s_codeToLocaleMap = new();
        private static Func<ReadOnlyMemory<L10nLanguage>> s_getLanguages;
        private static Func<SystemLanguage, L10nLanguage> s_toLanguage;

        public static IReadOnlyDictionary<string, Locale> LocaleMap => s_codeToLocaleMap;

#if UNITY_EDITOR
        // Hỗ trợ khi tắt Domain Reload trên editor
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void Init()
        {
            GlobalValueVault<bool>.TrySet(PresetId, false);

            s_getLanguages = null;
            s_toLanguage = null;
            s_codeToLocaleMap.Clear();
        }
#endif

        public static void Initialize(
              [NotNull] Func<ReadOnlyMemory<L10nLanguage>> getLanguages
            , [NotNull] Func<SystemLanguage, L10nLanguage> toLanguage
        )
        {
            GlobalValueVault<bool>.TrySet(PresetId, false);

            s_getLanguages = getLanguages;
            s_toLanguage = toLanguage;
            s_codeToLocaleMap.Clear();

            var locales = LocalizationSettings.AvailableLocales.Locales;

            foreach (var locale in locales)
            {
                s_codeToLocaleMap.TryAdd(locale.Identifier.Code, locale);
            }

            GlobalValueVault<bool>.TrySet(PresetId, true);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsReady()
            => GlobalValueVault<bool>.TryGet(PresetId, out var value) && value;

        public static bool TryGetDefaultLocaleCode(out string localeCode)
        {
            if (IsReady() == false)
            {
                ErrorNotReady();
                localeCode = default;
                return false;
            }

            localeCode = s_toLanguage(Application.systemLanguage).ToLocaleCode();

            var localeOpt = FindLocale(localeCode);

            if (localeOpt.HasValue == false)
            {
                ErrorCannotFindLanguage(localeCode);
                localeCode = L10nLanguage.Default.ToLocaleCode();
                localeOpt = FindLocale(localeCode);
            }

            return localeOpt.HasValue;
        }

        public static string SetLocale(string localeCode)
        {
            if (IsReady() == false)
            {
                ErrorNotReady();
                return L10nLanguage.Default.ToLocaleCode();
            }

            if (string.IsNullOrWhiteSpace(localeCode))
            {
                localeCode = s_toLanguage(Application.systemLanguage).ToLocaleCode();
            }

            var localeOpt = FindLocale(localeCode);

            if (localeOpt.HasValue == false)
            {
                ErrorCannotFindLanguage(localeCode);
                localeCode = s_toLanguage(Application.systemLanguage).ToLocaleCode();
                localeOpt = FindLocale(localeCode);
            }

            if (localeOpt.HasValue == false)
            {
                ErrorCannotFindLanguage(localeCode);
                localeCode = L10nLanguage.Default.ToLocaleCode();
                localeOpt = FindLocale(localeCode);
            }

            if (localeOpt.HasValue == false)
            {
                ErrorCannotFindLanguage(localeCode);
            }
            else
            {
                var locale = localeOpt.Value();
                InfoChangeLanguage(locale.LocaleName);
                LocalizationSettings.SelectedLocale = locale;
            }

            return localeCode;
        }

        public static void GetActiveLocales(ICollection<string> locales)
        {
            if (locales == null)
            {
                return;
            }

            locales.Clear();

            var languages = s_getLanguages().Span;

            foreach (var locale in languages)
            {
                locales.Add(locale.ToLocaleCode());
            }
        }

        public static void SaveSelectedLanguage(Action<string> onSave)
        {
            if (IsReady() == false)
            {
                ErrorNotReady();
                return;
            }

            var locale = LocalizationSettings.SelectedLocale;
            onSave?.Invoke(locale.Identifier.Code);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Option<Locale> FindLocale(string localeCode)
            => s_codeToLocaleMap.TryGetValue(localeCode, out var locale) && locale ? (Option<Locale>)locale : default;

        [HideInStackTrace, HideInCallstack, Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        private static void ErrorNotReady()
        {
            DevLoggerAPI.LogError("Must call \"L10n.Initialize()\" first.");
        }

        [HideInStackTrace, HideInCallstack, Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        private static void ErrorCannotFindLanguage(string value)
        {
            DevLoggerAPI.LogError($"Cannot find any language by locale code {value}");
        }

        [HideInStackTrace, HideInCallstack, Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        private static void InfoChangeLanguage(string value)
        {
            DevLoggerAPI.LogInfo($"Change language to {value}");
        }

        public readonly struct Localization { }
    }
}

#endif
