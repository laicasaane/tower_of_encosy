using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using UnityEngine.UIElements.Internal;

namespace UnityEngine.UIElements
{
    public static class EncosyBasePopupFieldExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetCreateMenuCallback<TValueType, TValueChoice>(
              [NotNull] this BasePopupField<TValueType, TValueChoice> self
            , Func<AbstractGenericMenu> createMenuCallback
        )
        {
            BasePopupFieldInternalAccessor.SetCreateMenuCallback(self, createMenuCallback);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetGenericMenu<TValueType, TValueChoice>(
              [NotNull] this BasePopupField<TValueType, TValueChoice> self
            , AbstractGenericMenu genericMenu
        )
        {
            BasePopupFieldInternalAccessor.SetGenericMenu(self, genericMenu);
        }
    }
}
