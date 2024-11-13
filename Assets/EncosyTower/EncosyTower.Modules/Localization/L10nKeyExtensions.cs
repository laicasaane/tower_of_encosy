#if UNITY_LOCALIZATION

namespace EncosyTower.Modules.Localization
{
    using System.Runtime.CompilerServices;
    using UnityEngine.Localization.Settings;

    // ReSharper disable once InconsistentNaming
    public static partial class L10nKeyExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static L10nKey Format<T0>(this L10nKey key, T0 arg0)
            => new(key.Table.TableCollectionName, string.Format(key.Entry.Key, arg0));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static L10nKey Format<T0, T1>(this L10nKey key, T0 arg0, T1 arg1)
            => new(key.Table.TableCollectionName, string.Format(key.Entry.Key, arg0, arg1));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static L10nKey Format<T0, T1, T2>(this L10nKey key, T0 arg0, T1 arg1, T2 arg2)
            => new(key.Table.TableCollectionName, string.Format(key.Entry.Key, arg0, arg1, arg2));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static L10nKey<T> Format<T, T0>(this L10nKey<T> key, T0 arg0)
            => new(key.Table.TableCollectionName, string.Format(key.Entry.Key, arg0));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static L10nKey<T> Format<T, T0, T1>(this L10nKey<T> key, T0 arg0, T1 arg1)
            => new(key.Table.TableCollectionName, string.Format(key.Entry.Key, arg0, arg1));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static L10nKey<T> Format<T, T0, T1, T2>(this L10nKey<T> key, T0 arg0, T1 arg1, T2 arg2)
            => new(key.Table.TableCollectionName, string.Format(key.Entry.Key, arg0, arg1, arg2));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string Localize(this L10nKey<string> key)
            => LocalizeInternal(key, null);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string Localize(this L10nKey<string> key, params object[] arguments)
            => LocalizeInternal(key, arguments);

        private static string LocalizeInternal(L10nKey<string> key, object[] arguments)
        {
            if (key.IsValid == false) return string.Empty;

            var handle = arguments == null || arguments.Length < 1
                ? LocalizationSettings.StringDatabase.GetLocalizedStringAsync(key.Table, key.Entry)
                : LocalizationSettings.StringDatabase.GetLocalizedStringAsync(key.Table, key.Entry, arguments)
                ;

            handle.WaitForCompletion();

            return handle.Result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TObject LocalizeAsset<TObject>(this L10nKey<TObject> key)
            where TObject : UnityEngine.Object
            => LocalizationSettings.AssetDatabase.GetLocalizedAsset<TObject>(key.Table, key.Entry);

        #region SERIALIZABLE
        #endregion =========

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static L10nKey.Serializable Format<T0>(this L10nKey.Serializable key, T0 arg0)
            => new(key.Table.TableCollectionName, string.Format(key.Entry.Key, arg0));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static L10nKey.Serializable Format<T0, T1>(this L10nKey.Serializable key, T0 arg0, T1 arg1)
            => new(key.Table.TableCollectionName, string.Format(key.Entry.Key, arg0, arg1));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static L10nKey.Serializable Format<T0, T1, T2>(this L10nKey.Serializable key, T0 arg0, T1 arg1, T2 arg2)
            => new(key.Table.TableCollectionName, string.Format(key.Entry.Key, arg0, arg1, arg2));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static L10nKey.Serializable<T> Format<T, T0>(this L10nKey.Serializable<T> key, T0 arg0)
            => new(key.Table.TableCollectionName, string.Format(key.Entry.Key, arg0));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static L10nKey.Serializable<T> Format<T, T0, T1>(this L10nKey.Serializable<T> key, T0 arg0, T1 arg1)
            => new(key.Table.TableCollectionName, string.Format(key.Entry.Key, arg0, arg1));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static L10nKey.Serializable<T> Format<T, T0, T1, T2>(this L10nKey.Serializable<T> key, T0 arg0, T1 arg1, T2 arg2)
            => new(key.Table.TableCollectionName, string.Format(key.Entry.Key, arg0, arg1, arg2));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string Localize(this L10nKey.Serializable<string> key)
            => Localize((L10nKey<string>)key);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string Localize(this L10nKey.Serializable<string> key, params object[] arguments)
            => Localize((L10nKey<string>)key, arguments);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TObject LocalizeAsset<TObject>(this L10nKey.Serializable<TObject> key)
            where TObject : UnityEngine.Object
            => LocalizeAsset((L10nKey<TObject>)key);
    }
}

#endif
