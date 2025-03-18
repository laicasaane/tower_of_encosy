#if UNITY_EDITOR

using System;

namespace EncosyTower.Editor
{
    /// <summary>
    /// Determines which static method can create custom MenuItem for the Editor.
    /// </summary>
    /// <remarks>
    /// This attribute is only applicable to methods that are:
    /// <list type="bullet">
    /// <item>Static</item>
    /// <item>Non-generic</item>
    /// <item>Parameterless</item>
    /// <item>Return an <see cref="EditorMenuItem"/></item>
    /// <item>Return a collection of <see cref="EditorMenuItem"/></item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <code>
    /// using EncosyTower.Editor;
    ///
    /// [MenuItemProvider]
    /// private static MenuItem CustomMenuItem()
    /// {
    ///     return new MenuItem { ... };
    /// }
    /// </code>
    /// <code>
    /// using System.Collections.Generic;
    /// using EncosyTower.Editor;
    ///
    /// [MenuItemProvider]
    /// private static List&lt;MenuItem&gt; CustomMenuItems()
    /// {
    ///     return new List&lt;MenuItem&gt; {
    ///         new MenuItem { ... },
    ///         new MenuItem { ... },
    ///     };
    /// }
    /// </code>
    /// </example>
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class MenuItemProviderAttribute : Attribute
    {
    }
}

#endif
