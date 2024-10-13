#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;

namespace Module.Core.Extended.Editor.Mvvm.ViewBinding.Unity
{
    partial class MonoViewInspector
    {
        private static GUIStyle s_rootTabViewStyle;
        private static GUIStyle s_rootTabLabelStyle;
        private static GUIStyle s_toolbarLeftButtonStyle;
        private static GUIStyle s_toolbarMidButtonStyle;
        private static GUIStyle s_toolbarRightButtonStyle;
        private static GUIStyle s_toolbarMenuButtonStyle;
        private static GUIStyle s_noBinderStyle;
        private static GUIStyle s_binderButtonStyle;
        private static GUIStyle s_binderIndexLabelStyle;
        private static GUIStyle s_binderSelectedButtonStyle;
        private static GUIStyle s_bindersHeaderStyle;
        private static GUIStyle s_detailsHeaderStyle;
        private static GUIStyle s_removeButtonStyle;
        private static GUIStyle s_indexLabelStyle;
        private static GUIStyle s_headerLabelStyle;
        private static GUIStyle s_subHeaderLabelStyle;
        private static GUIStyle s_itemLabelStyle;
        private static GUIStyle s_chooseContextButtonStyle;
        private static GUIStyle s_popupStyle;
        private static GUIContent s_addLabel;
        private static GUIContent s_removeLabel;
        private static GUIContent s_removeSelectedLabel;
        private static GUIContent s_menuLabel;
        private static GUIContent s_iconWarning;
        private static GUIContent s_iconBinding;
        private static GUIContent s_applyLabel;
        private static GUIContent s_cancelLabel;
        private static GUIContent s_chooseLabel;
        private static Color s_headerColor;
        private static Color s_contentColor;
        private static Color s_selectedColor;
        private static Color s_menuColor;
        private static Color s_altContentColor;

        private static readonly GUIContent s_bindersLabel = new("Binders");
        private static readonly GUIContent[] s_detailsTabLabels = new GUIContent[] { new("Bindings"), new("Targets"), };
        private static readonly GUIContent s_propertyBindingLabel = new("Property");
        private static readonly GUIContent s_commandBindingLabel = new("Command");
        private static readonly GUIContent s_converterLabel = new("Converter");
        private static readonly GUIContent s_clearAllLabel = new("Clear All");
        private static readonly GUIContent s_copyAllLabel = new("Copy All");
        private static readonly GUIContent s_pasteAllLabel = new("Paste All");
        private static readonly GUIContent s_editSubtitleLabel = new("Edit Subtitle");
        private static readonly GUIContent s_copyItemLabel = new("Copy");
        private static readonly GUIContent s_pasteItemLabel = new("Paste");
        private static readonly GUIContent s_deleteItemLabel = new("Delete");

        private void InitStyles()
        {
            if (s_rootTabViewStyle != null)
            {
                return;
            }

            s_rootTabViewStyle = new(EditorStyles.helpBox) {
                padding = new(0, 0, 0, 0),
                margin = new(0, 0, 0, 0),
                border = new(0, 0, 0, 0),
            };

            s_rootTabLabelStyle = new(EditorStyles.boldLabel) {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 13,
            };

            s_toolbarLeftButtonStyle = new(EditorStyles.miniButtonMid) {
                padding = new(0, 0, 0, 0),
                margin = new(2, 0, 0, 0),
                border = new(0, 0, 0, 0),
                fixedHeight = 0,
            };

            s_toolbarMidButtonStyle = new(EditorStyles.miniButtonMid) {
                padding = new(0, 0, 0, 0),
                margin = new(0, 0, 0, 0),
                border = new(0, 0, 0, 0),
                fixedHeight = 0,
            };

            s_toolbarRightButtonStyle = new(EditorStyles.miniButtonMid) {
                padding = new(0, 0, 0, 0),
                margin = new(0, 1, 0, 0),
                border = new(0, 0, 0, 0),
                fixedHeight = 0,
            };

            s_toolbarMenuButtonStyle = new(s_toolbarRightButtonStyle) {
                padding = new(2, 2, 2, 2),
            };

            s_noBinderStyle = new(EditorStyles.miniLabel) {
                padding = new(0, 0, 0, 0),
                margin = new(1, 0, 0, 0),
                fixedHeight = 0,
                fixedWidth = 0,
                stretchWidth = true,
                stretchHeight = true,
                fontSize = EditorStyles.miniLabel.fontSize,
                fontStyle = FontStyle.Italic,
                alignment = TextAnchor.MiddleCenter,
            };

            s_binderButtonStyle = new(EditorStyles.toolbarButton) {
                padding = new(30, 30, 0, 0),
                margin = new(1, 0, 0, 0),
                fixedHeight = 0,
                fontSize = GUI.skin.button.fontSize,
                richText = true,
            };

            s_binderSelectedButtonStyle = new(s_binderButtonStyle) {
                fontStyle = FontStyle.Bold,
                margin = new(1, 2, 0, 0),
            };

            s_binderIndexLabelStyle = new(EditorStyles.miniLabel) {
                alignment = TextAnchor.MiddleLeft,
            };

            {
                var style = s_binderSelectedButtonStyle;

                style.normal.scaledBackgrounds
                    = style.onNormal.scaledBackgrounds
                    = style.active.scaledBackgrounds
                    = style.onActive.scaledBackgrounds
                    = new Texture2D[] { Texture2D.whiteTexture };

                style.normal.background
                    = style.onNormal.background
                    = style.active.background
                    = style.onActive.background
                    = Texture2D.whiteTexture;
            }

            s_bindersHeaderStyle = new() {
                padding = new(0, 0, 0, 0),
                margin = new(0, 0, 0, 0),
                border = new(0, 0, 0, 0),
                stretchHeight = true,
                stretchWidth = true,
            };

            s_detailsHeaderStyle = new() {
                padding = new(0, 0, 0, 0),
                margin = new(0, 0, 0, 0),
                border = new(0, 0, 0, 0),
                stretchHeight = true,
                stretchWidth = true,
            };

            s_removeButtonStyle = new(EditorStyles.iconButton);

            s_indexLabelStyle = new(EditorStyles.miniLabel) {
                padding = new(3, 0, 0, 0),
            };

            s_headerLabelStyle = new(EditorStyles.boldLabel) {
                padding = new(0, 0, 0, 0),
                margin = new(0, 0, 0, 0),
                alignment = TextAnchor.MiddleCenter,
            };

            s_subHeaderLabelStyle = new(EditorStyles.label) {
                padding = new(0, 0, 0, 0),
                margin = new(0, 0, 0, 0),
                alignment = TextAnchor.MiddleCenter,
                fontSize = 10,
            };

            s_itemLabelStyle = new(EditorStyles.label) {
                stretchWidth = false,
            };

            s_chooseContextButtonStyle = new(EditorStyles.miniButton) {
                stretchWidth = true,
                stretchHeight = true,
                fixedHeight = 0,
                fixedWidth = 0,
                alignment = TextAnchor.MiddleCenter,
            };

            s_popupStyle = new(EditorStyles.popup) {
                richText = true,
            };

            {
                var icon = EditorGUIUtility.isProSkin
                    ? EditorGUIUtility.IconContent("d_Toolbar Plus More")
                    : EditorGUIUtility.IconContent("Toolbar Plus More");

                s_addLabel = new(icon.image, "Add");
            }

            {
                var icon = EditorGUIUtility.isProSkin
                    ? EditorGUIUtility.IconContent("d_Toolbar Minus")
                    : EditorGUIUtility.IconContent("Toolbar Minus");

                s_removeLabel = new(icon.image, "Remove");
                s_removeSelectedLabel = new(icon.image, "Remove Selected Item");
            }

            {
                var icon = EditorGUIUtility.IconContent("pane options@2x");
                s_menuLabel = new(icon.image, "Menu");
                s_menuColor = EditorGUIUtility.isProSkin ? Color.white : Color.black;
            }

            {
                var icon = EditorGUIUtility.isProSkin
                    ? EditorGUIUtility.IconContent("d_console.warnicon")
                    : EditorGUIUtility.IconContent("console.warnicon");

                s_iconWarning = new GUIContent(icon.image);
            }

            s_iconBinding = EditorGUIUtility.isProSkin
                ? EditorGUIUtility.IconContent("d_BlendTree Icon")
                : EditorGUIUtility.IconContent("BlendTree Icon");

            {
                var icon = EditorGUIUtility.isProSkin
                    ? EditorGUIUtility.IconContent("d_P4_CheckOutRemote")
                    : EditorGUIUtility.IconContent("P4_CheckOutRemote");

                s_applyLabel = new GUIContent(icon.image, "Apply");
            }

            {
                var icon = EditorGUIUtility.isProSkin
                    ? EditorGUIUtility.IconContent("d_P4_DeletedLocal")
                    : EditorGUIUtility.IconContent("P4_DeletedLocal");

                s_cancelLabel = new GUIContent(icon.image, "Cancel");
            }

            {
                var icon = EditorGUIUtility.isProSkin
                    ? EditorGUIUtility.IconContent("d_Grid.PickingTool")
                    : EditorGUIUtility.IconContent("Grid.PickingTool");

                s_chooseLabel = new GUIContent("Choose", icon.image);
            }

            {
                ColorUtility.TryParseHtmlString("#2C5D87", out var darkColor);
                ColorUtility.TryParseHtmlString("#3A72B0", out var lightColor);
                s_selectedColor = EditorGUIUtility.isProSkin ? darkColor : lightColor;
            }

            {
                ColorUtility.TryParseHtmlString("#474747", out var darkColor);
                ColorUtility.TryParseHtmlString("#D6D6D6", out var lightColor);
                s_headerColor = EditorGUIUtility.isProSkin ? darkColor : lightColor;
            }

            {
                ColorUtility.TryParseHtmlString("#383838", out var darkColor);
                ColorUtility.TryParseHtmlString("#C8C8C8", out var lightColor);
                s_contentColor = EditorGUIUtility.isProSkin ? darkColor : lightColor;
            }

            {
                ColorUtility.TryParseHtmlString("#404040", out var darkColor);
                ColorUtility.TryParseHtmlString("#ABABAB", out var lightColor);
                s_altContentColor = EditorGUIUtility.isProSkin ? darkColor : lightColor;
            }
        }
    }
}

#endif
