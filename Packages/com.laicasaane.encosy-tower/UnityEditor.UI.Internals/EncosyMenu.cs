// https://discussions.unity.com/t/its-possible-to-create-menuitems-with-out-attributes/171587/3
// https://gist.github.com/JVinceW/1db8eaa93c7e8daf7bf21455a6d569af

using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using UnityEditor;

namespace EncosyTower.Editor
{
    public static class EncosyMenu
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void RemoveMenuItem(string name)
            => Menu.RemoveMenuItem(name);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool MenuItemExists(string menuPath)
            => Menu.MenuItemExists(menuPath);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AddSeparator(string menuPath, int priority)
            => Menu.AddSeparator(menuPath, priority);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AddMenuItem(
              string name
            , string shortcut
            , bool @checked
            , int priority
            , Action execute
            , Func<bool> validate
        )
        {
            if (string.IsNullOrEmpty(name))
            {
                return;
            }

            Menu.AddMenuItem(
                  name
                , shortcut ?? string.Empty
                , @checked
                , priority
                , execute
                , validate
            );
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AddMenuItem([NotNull] EditorMenuItem item)
        {
            if (item.Separator)
            {
                AddSeparator(item.Name, item.Priority);
                return;
            }

            AddMenuItem(
                  item.Name
                , item.Shortcut
                , item.Checked
                , item.Priority
                , item.Execute
                , item.Validate
            );
        }
    }
}
