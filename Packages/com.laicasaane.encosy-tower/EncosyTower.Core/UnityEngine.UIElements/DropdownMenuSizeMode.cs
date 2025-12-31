#if !UNITY_6000_3_OR_NEWER

namespace UnityEngine.UIElements
{
    /// <summary>
    /// Mode used to calculate the width of a dropdown.
    /// </summary>
    public enum DropdownMenuSizeMode
    {
        /// <summary>
        /// The width of the dropdown matches the width of the provided Rect, but if the
        /// content is wider, it expands to fit the content. This is the only supported mode
        /// for OS menus.
        /// </summary>
        Auto,

        /// <summary>
        /// The width of the dropdown matches the width the provided Rect.
        /// </summary>
        Fixed,

        /// <summary>
        /// The width of the dropdown menu matches the width of the content.
        /// </summary>
        Content
    }
}

#endif
