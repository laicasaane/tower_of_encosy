using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using EncosyTower.Common;
using EncosyTower.Ids;
using EncosyTower.StringIds;
using EncosyTower.Types;

namespace EncosyTower.Vaults
{
    public static partial class GlobalObjectVault
    {
        private static ObjectVault<Id2> s_vaultIdT = new();
        private static ObjectVault<Id2> s_vaultId2 = new();
        private static ObjectVault<MetaStringId> s_vaultStringId = new();

#if UNITY_EDITOR
        [UnityEditor.InitializeOnEnterPlayMode, UnityEngine.Scripting.Preserve]
        private static void InitWhenDomainReloadDisabled()
        {
            s_vaultIdT?.Dispose();
            s_vaultIdT = new();

            s_vaultId2?.Dispose();
            s_vaultId2 = new();

            s_vaultStringId?.Dispose();
            s_vaultStringId = new();
        }
#endif

        #region    ID<T>
        #endregion =====

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Contains<T>(Id<T> id)
            => s_vaultIdT.Contains(ToId2(id));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryAdd<T>(Id<T> id, [NotNull] T obj)
            => s_vaultIdT.TryAdd(ToId2(id), obj);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryRemove<T>(Id<T> id, out Option<T> obj)
            => s_vaultIdT.TryRemove(ToId2(id), out obj);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryGet<T>(Id<T> id, out Option<T> obj)
            => s_vaultIdT.TryGet<T>(ToId2(id), out obj);

        #region    ID2
        #endregion ===

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Contains<T>(Id2 id)
            => s_vaultId2.Contains(id);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryAdd<T>(Id2 id, [NotNull] T obj)
            => s_vaultId2.TryAdd(id, obj);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryRemove<T>(Id2 id, out Option<T> obj)
            => s_vaultId2.TryRemove(id, out obj);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryGet<T>(Id2 id, out Option<T> obj)
            => s_vaultId2.TryGet<T>(id, out obj);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryGet(Id2 id, out Option<object> obj)
            => s_vaultId2.TryGet(id, out obj);

        #region    STRINGID
        #endregion ========

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Contains<T>(StringId<T> id)
            => s_vaultStringId.Contains(ToMetaStringId(id));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryAdd<T>(StringId<T> id, [NotNull] T obj)
            => s_vaultStringId.TryAdd(ToMetaStringId(id), obj);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryRemove<T>(StringId<T> id, out Option<T> obj)
            => s_vaultStringId.TryRemove(ToMetaStringId(id), out obj);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryGet<T>(StringId<T> id, out Option<T> obj)
            => s_vaultStringId.TryGet(ToMetaStringId(id), out obj);

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
