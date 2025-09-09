#if UNITY_EDITOR

using System.Collections.Generic;
using UnityEditor;

namespace EncosyTower.Editor
{
    internal static class EditorMenuItemPopulator
    {
        [InitializeOnLoadMethod]
        private static void Init()
        {
            // Usage of EditorApplication.delayCall += VerifyCreateMenu;
            // is required to let unity finish all its native menu population process,
            // else your new menu can get lost.
            // https://gist.github.com/JVinceW/1db8eaa93c7e8daf7bf21455a6d569af
            EditorApplication.delayCall -= VerifyCreateMenu;
            EditorApplication.delayCall += VerifyCreateMenu;
        }

        private static void VerifyCreateMenu()
        {
            var providers = TypeCache.GetMethodsWithAttribute<MenuItemProviderAttribute>();
            var typeOfMenuItem = typeof(EditorMenuItem);
            var typeOfEnumerable = typeof(IEnumerable<EditorMenuItem>);

            foreach (var provider in providers)
            {
                if (provider.IsStatic == false
                    || provider.IsConstructor
                    || provider.ContainsGenericParameters
                    || provider.GetParameters().Length != 0
                )
                {
                    continue;
                }

                if (provider.ReturnType == typeOfMenuItem)
                {
                    EncosyMenu.AddMenuItem(provider.Invoke(null, null) as EditorMenuItem);
                    continue;
                }

                if (typeOfEnumerable.IsAssignableFrom(provider.ReturnType))
                {
                    var items = provider.Invoke(null, null) as IEnumerable<EditorMenuItem>;

                    foreach (var item in items)
                    {
                        if (item is not null)
                        {
                            EncosyMenu.AddMenuItem(item);
                        }
                    }

                    continue;
                }
            }
        }
    }
}

#endif
