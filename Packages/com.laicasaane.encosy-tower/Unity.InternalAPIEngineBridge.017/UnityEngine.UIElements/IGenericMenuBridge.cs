#if !UNITY_6000_3_OR_NEWER

using System.Runtime.CompilerServices;

namespace UnityEngine.UIElements.Internal
{
    internal interface IGenericMenuBridge : IGenericMenu
    {
        void DropDown(Rect position, VisualElement targetElement, int mode);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void IGenericMenu.DropDown(Rect position, VisualElement targetElement, bool anchored)
            => DropDown(position, targetElement, anchored ? 1 : 0);
    }
}

#endif
