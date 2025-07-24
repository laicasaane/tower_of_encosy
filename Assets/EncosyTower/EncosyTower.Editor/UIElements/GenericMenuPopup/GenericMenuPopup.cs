#if UNITY_EDITOR

using System.Collections;
using System.Reflection;
using EncosyTower.Common;
using UnityEditor;
using UnityEngine;

namespace EncosyTower.Editor.UIElements
{
    public class GenericMenuPopup
    {
        public string title;
        public MenuItemNode rootNode;

        public int width = 200;
        public int height = 200;
        public int maxHeight = 300;
        public bool resizeToContent = false;
        public bool showOnStatus = true;
        public bool showSearch = true;
        public bool showTooltip = false;
        public bool showTitle = false;

        public GenericMenuPopup(MenuItemNode rootNode, string title)
        {
            this.title = title;
            this.showTitle = title.IsNotEmpty();
            this.rootNode = rootNode;
        }

        public GenericMenuPopup(GenericMenu menu, string title)
        {
            this.title = title;
            this.showTitle = title.IsNotEmpty();
            this.rootNode = GenerateMenuItemNodeTree(menu);
        }

        public void Show(Vector2 position)
        {
            var window = ScriptableObject.CreateInstance<GenericMenuPopupWindow>();
            window.Initialize(this);

            var mousePosition = GUIUtility.GUIToScreenPoint(position);
            window.position = new Rect(mousePosition.x, mousePosition.y, width, height);

            window.ShowPopup();
        }

        public static MenuItemNode GenerateMenuItemNodeTree(GenericMenu menu)
        {
            var rootNode = new MenuItemNode();

            if (menu == null)
            {
                return rootNode;
            }

            var menuItems = TryGetMenuItems(menu, "menuItems") ?? TryGetMenuItems(menu, "m_MenuItems");

            foreach (var menuItem in menuItems)
            {
                var menuItemType = menuItem.GetType();
                var content = (GUIContent)menuItemType.GetField("content").GetValue(menuItem);

                var separator = (bool)menuItemType.GetField("separator").GetValue(menuItem);
                var path = content.text;
                var splitPath = path.Split('/');
                var currentNode = rootNode;

                for (var i = 0; i < splitPath.Length; i++)
                {
                    currentNode = (i < splitPath.Length - 1)
                        ? currentNode.GetOrCreateNode(splitPath[i])
                        : currentNode.CreateNode(splitPath[i]);
                }

                if (separator)
                {
                    currentNode.separator = true;
                }
                else
                {
                    currentNode.content = content;
                    currentNode.func = (GenericMenu.MenuFunction)menuItemType.GetField("func").GetValue(menuItem);
                    currentNode.func2 = (GenericMenu.MenuFunction2)menuItemType.GetField("func2").GetValue(menuItem);
                    currentNode.userData = menuItemType.GetField("userData").GetValue(menuItem);
                    currentNode.on = (bool)menuItemType.GetField("on").GetValue(menuItem);
                }
            }

            return rootNode;

            static IEnumerable TryGetMenuItems(GenericMenu menu, string fieldName)
            {
                var menuItemsField = menu.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
                return menuItemsField?.GetValue(menu) as IEnumerable;
            }
        }
    }
}

#endif
