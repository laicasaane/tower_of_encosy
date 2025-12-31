#if !UNITY_6000_3_OR_NEWER

using System;
using System.Runtime.CompilerServices;
using UnityEngine.UIElements.Internal;

namespace UnityEngine.UIElements
{
    /// <summary>
    /// Base class for menu functionality.
    /// </summary>
    public abstract class AbstractGenericMenu : IGenericMenuBridge
    {
        /// <summary>
        /// Adds an item to this menu.
        /// </summary>
        /// <param name="itemName">The text to display to the user.</param>
        /// <param name="isChecked">Indicates whether a checkmark next to the item is displayed.</param>
        /// <param name="action">The callback to invoke when the item is selected by the user.</param>
        public abstract void AddItem(string itemName, bool isChecked, Action action);

        /// <summary>
        /// Adds an item to this menu.
        /// </summary>
        /// <param name="itemName">The text to display to the user.</param>
        /// <param name="isChecked">Indicates whether a checkmark next to the item is displayed.</param>
        /// <param name="action">The callback to invoke when the item is selected by the user.</param>
        /// <param name="data">The object to pass to the callback as a parameter.</param>
        public abstract void AddItem(string itemName, bool isChecked, Action<object> action, object data);

        /// <summary>
        /// Adds a disabled item to this menu.
        /// </summary>
        /// <param name="itemName">The text to display to the user.</param>
        /// <param name="isChecked">Indicates whether a checkmark next to the item is displayed.</param>
        public abstract void AddDisabledItem(string itemName, bool isChecked);

        /// <summary>
        /// Adds a visual separator to this menu.
        /// </summary>
        /// <param name="path">The text to display to the user.</param>
        public abstract void AddSeparator(string path);

        /// <summary>
        /// Displays the menu at the specified position.
        /// </summary>
        /// <param name="position">The position in the coordinate space of the panel.</param>
        /// <param name="targetElement">The element determines which root to use as the menu's parent.</param>
        /// <param name="dropdownMenuSizeMode">Indicates how to format the menu size.</param>
        public abstract void DropDown(Rect position, VisualElement targetElement, DropdownMenuSizeMode dropdownMenuSizeMode = DropdownMenuSizeMode.Auto);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void IGenericMenuBridge.DropDown(Rect position, VisualElement targetElement, int mode)
            => DropDown(position, targetElement, (DropdownMenuSizeMode)mode);
    }
}

#endif
