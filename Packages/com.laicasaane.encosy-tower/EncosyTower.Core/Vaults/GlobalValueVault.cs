namespace EncosyTower.Vaults
{
    using System.Runtime.CompilerServices;
    using EncosyTower.Ids;
    using EncosyTower.StringIds;
    using EncosyTower.Types;

    public static partial class GlobalValueVault<TValue>
        where TValue : struct
    {
        private readonly static ValueVault<Id2, TValue> s_vaultIdT = new();
        private readonly static ValueVault<Id2, TValue> s_vaultId2 = new();
        private readonly static ValueVault<MetaStringId, TValue> s_vaultStringId = new();

        #region    ID<T>
        #endregion =====

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Contains<T>(Id<T> id)
            => s_vaultIdT.Contains(ToId2(id));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryRemove<T>(Id<T> id, out TValue value)
            => s_vaultIdT.TryRemove(ToId2(id), out value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryGet<T>(Id<T> id, out TValue value)
            => s_vaultIdT.TryGet(ToId2(id), out value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TrySet<T>(Id<T> id, TValue value)
            => s_vaultIdT.TrySet(ToId2(id), value);

        #region    ID2
        #endregion ===

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Contains(Id2 id)
            => s_vaultId2.Contains(id);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryRemove(Id2 id, out TValue value)
            => s_vaultId2.TryRemove(id, out value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryGet(Id2 id, out TValue value)
            => s_vaultId2.TryGet(id, out value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TrySet(Id2 id, TValue value)
            => s_vaultId2.TrySet(id, value);

        #region    STRINGID<T>
        #endregion ===========

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Contains<T>(StringId<T> id)
            => s_vaultStringId.Contains(ToMetaStringId(id));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryRemove<T>(StringId<T> id, out TValue value)
            => s_vaultStringId.TryRemove(ToMetaStringId(id), out value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryGet<T>(StringId<T> id, out TValue value)
            => s_vaultStringId.TryGet(ToMetaStringId(id), out value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TrySet<T>(StringId<T> id, TValue value)
            => s_vaultStringId.TrySet(ToMetaStringId(id), value);

        #region    HELPERS
        #endregion =======

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Id2 ToId2<T>(Id<T> id)
            => Type<T>.Id.ToId2(id);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static MetaStringId ToMetaStringId<T>(StringId<T> id)
            => new(id, (Id<T>)Type<T>.Id);
    }
}

#if UNITY_EDITOR

namespace EncosyTower.Vaults
{
    using System;
    using System.Collections.Concurrent;
    using EncosyTower.Collections;
    using EncosyTower.Ids;
    using EncosyTower.Types;
    using UnityEditor;
    using UnityEngine.Scripting;

    partial class GlobalValueVault<TValue>
    {
        static GlobalValueVault()
        {
            GlobalValueVaultEditor.Register(s_vaultIdT);
            GlobalValueVaultEditor.Register(s_vaultId2);
            GlobalValueVaultEditor.Register(s_vaultStringId);
        }
    }

    internal static partial class GlobalValueVaultEditor
    {
        private readonly static ConcurrentDictionary<Id2, IClearable> s_vaults = new();

        [InitializeOnEnterPlayMode, Preserve]
        private static void InitWhenDomainReloadDisabled()
        {
            var vaults = s_vaults;

            foreach (var (_, disposable) in vaults)
            {
                disposable?.Clear();
            }
        }

        public static void Register<TId, TValue>(ValueVault<TId, TValue> vault)
            where TId : unmanaged, IEquatable<TId>
            where TValue : struct
        {
            var x = (Id<TId>)Type<TId>.Id;
            var y = (Id<TValue>)Type<TValue>.Id;

            s_vaults.TryAdd(new(x, y), vault);
        }
    }
}

#endif
