#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;

namespace EncosyTower.Modules.Editor.Mvvm.ViewBinding.Unity
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
        private static GUIStyle s_panelHeaderStyle;
        private static GUIStyle s_indexLabelStyle;
        private static GUIStyle s_headerLabelStyle;
        private static GUIStyle s_subHeaderLabelStyle;
        private static GUIStyle s_itemLabelStyle;
        private static GUIStyle s_chooseContextButtonStyle;
        private static GUIStyle s_popupStyle;
        private static GUIStyle s_iconButtonStyle;
        private static GUIContent s_addMoreIconLabel;
        private static GUIContent s_addIconLabel;
        private static GUIContent s_removeSelectedLabel;
        private static GUIContent s_menuIconLabel;
        private static GUIContent s_iconWarning;
        private static GUIContent s_iconBinding;
        private static GUIContent s_applyLabel;
        private static GUIContent s_cancelLabel;
        private static GUIContent s_chooseIconLabel;
        private static GUIContent s_foldoutExpandedIconLabel;
        private static GUIContent s_foldoutCollapsedIconLabel;
        private static GUIContent s_resetIconLabel;
        private static Color s_headerColor;
        private static Color s_selectedColor;
        private static Color s_menuColor;
        private static Color s_backColor;
        private static Color s_altBackColor;

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
        private static readonly GUIContent s_moveUpLabel = new("Move Up");
        private static readonly GUIContent s_moveDownLabel = new("Move Down");

        private static readonly GUIContent s_bindersLabel = new(
            "Binders"//, "Can also drag and drop GameObject or Component here to create binders."
        );

        private static readonly GUIContent s_bindingsLabel = new("Bindings");
        private static readonly GUIContent s_targetsLabel = new(
            "Targets", "Can also drag and drop GameObject or Component here to add to the list."
        );

        private static readonly GUIContent[] s_detailsTabLabels = new GUIContent[] {
            new("Bindings"),
            new("Targets", "Can also drag and drop GameObject or Component here to add to the list."),
        };

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
                fontSize = EditorStyles.boldLabel.fontSize + 1,
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
                fixedWidth = 0,
                fontSize = GUI.skin.button.fontSize,
                richText = true,
                alignment = TextAnchor.MiddleCenter,
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
                    = new[] { Texture2D.whiteTexture };

                style.normal.background
                    = style.onNormal.background
                    = style.active.background
                    = style.onActive.background
                    = Texture2D.whiteTexture;
            }

            s_panelHeaderStyle = new() {
                padding = new(0, 0, 0, 0),
                margin = new(0, 0, 0, 0),
                border = new(0, 0, 0, 0),
                stretchHeight = true,
                stretchWidth = true,
            };

            s_indexLabelStyle = new(EditorStyles.miniLabel) {
                padding = new(3, 0, 0, 0),
            };

            s_headerLabelStyle = new(EditorStyles.boldLabel) {
                padding = new(0, 0, 0, 0),
                margin = new(0, 0, 0, 0),
                alignment = TextAnchor.MiddleCenter,
                fontSize = EditorStyles.boldLabel.fontSize,
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

            s_iconButtonStyle = new(GUI.skin.button) {
                imagePosition = ImagePosition.ImageLeft,
                padding = new(2, 2, 1, 1),
            };

            {
                var icon = EditorAPI.GetIcon("d_Toolbar Plus", "Toolbar Plus");
                s_addIconLabel = new(icon.image, "Add");
            }

            {
                var icon = EditorAPI.GetIcon("d_Toolbar Plus More", "Toolbar Plus More");
                s_addMoreIconLabel = new(icon.image, "Add");
            }

            {
                var icon = EditorAPI.GetIcon("d_Toolbar Minus", "Toolbar Minus");
                s_removeSelectedLabel = new(icon.image, "Remove Selected Item");
            }

            {
                var icon = EditorGUIUtility.IconContent("pane options@2x");
                s_menuIconLabel = new(icon.image, "Menu");
                s_menuColor = EditorAPI.IsDark ? Color.white : Color.black;
            }

            {
                var icon = EditorAPI.GetIcon("d_console.warnicon", "console.warnicon");
                s_iconWarning = new GUIContent(icon.image);
            }

            s_iconBinding = EditorAPI.GetIcon("d_BlendTree Icon", "BlendTree Icon");

            {
                var icon = EditorAPI.GetIcon("d_P4_CheckOutRemote", "P4_CheckOutRemote");
                s_applyLabel = new GUIContent(icon.image, "Apply");
            }

            {
                var icon = EditorAPI.GetIcon("d_P4_DeletedLocal", "P4_DeletedLocal");
                s_cancelLabel = new(icon.image, "Cancel");
            }

            {
                var icon = EditorAPI.GetIcon("d_Grid.PickingTool", "Grid.PickingTool");
                s_chooseIconLabel = new(icon.image, "Choose");
            }

            {
                var icon = EditorGUIUtility.IconContent("IN foldout@2x");
                s_foldoutCollapsedIconLabel = new(icon.image);
            }

            {
                var icon = EditorGUIUtility.IconContent("IN foldout on@2x");
                s_foldoutExpandedIconLabel = new(icon.image);
            }

            {
                var icon = EditorAPI.GetIcon("d_Refresh", "Refresh");
                s_resetIconLabel = new(icon.image, "Reset");
            }

            s_selectedColor = EditorAPI.GetColor("#2C5D87", "#3A72B0");
            s_headerColor = EditorAPI.GetColor("#474747", "#D6D6D6");
            s_backColor = EditorAPI.GetColor("#383838", "#C8C8C8");
            s_altBackColor = EditorAPI.GetColor("#3F3F3F", "#CACACA");
        }
    }
}

#endif
