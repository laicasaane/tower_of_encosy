#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;

namespace Module.Core.Extended.Editor.Mvvm.ViewBinding.Unity
{
    partial class MonoViewInspector
    {
        private GUIStyle _rootTabViewStyle;
        private GUIStyle _rootTabLabelStyle;
        private GUIStyle _toolbarLeftButtonStyle;
        private GUIStyle _toolbarMidButtonStyle;
        private GUIStyle _toolbarRightButtonStyle;
        private GUIStyle _toolbarMenuButtonStyle;
        private GUIStyle _noBinderStyle;
        private GUIStyle _binderButtonStyle;
        private GUIStyle _binderIndexLabelStyle;
        private GUIStyle _binderSelectedButtonStyle;
        private GUIStyle _bindersHeaderStyle;
        private GUIStyle _detailsHeaderStyle;
        private GUIStyle _removeButtonStyle;
        private GUIStyle _indexLabelStyle;
        private GUIStyle _headerLabelStyle;
        private GUIStyle _itemLabelStyle;
        private GUIContent _addLabel;
        private GUIContent _removeLabel;
        private GUIContent _removeSelectedLabel;
        private GUIContent _menuLabel;
        private GUIContent _clearLabel;
        private GUIContent _subtitleLabel;
        private GUIContent _iconWarning;
        private GUIContent _iconBinding;
        private GUIContent _applyIconLabel;
        private GUIContent _cancelIconLabel;
        private GUIContent _bindersLabel;
        private GUIContent _propertyBindingLabel;
        private GUIContent _commandBindingLabel;
        private GUIContent[] _detailsTabLabels;
        private Color _headerColor;
        private Color _contentColor;
        private Color _selectedColor;
        private Color _menuColor;
        private Color _altContentColor;

        private void InitStyles()
        {
            if (_rootTabViewStyle != null)
            {
                return;
            }

            _rootTabViewStyle = new(EditorStyles.helpBox) {
                padding = new(0, 0, 0, 0),
                margin = new(0, 0, 0, 0),
                border = new(0, 0, 0, 0),
            };

            _rootTabLabelStyle = new(EditorStyles.boldLabel) {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 13,
            };

            _toolbarLeftButtonStyle = new(EditorStyles.miniButtonMid) {
                padding = new(0, 0, 0, 0),
                margin = new(2, 0, 0, 0),
                border = new(0, 0, 0, 0),
                fixedHeight = 0,
            };

            _toolbarMidButtonStyle = new(EditorStyles.miniButtonMid) {
                padding = new(0, 0, 0, 0),
                margin = new(0, 0, 0, 0),
                border = new(0, 0, 0, 0),
                fixedHeight = 0,
            };

            _toolbarRightButtonStyle = new(EditorStyles.miniButtonMid) {
                padding = new(0, 0, 0, 0),
                margin = new(0, 1, 0, 0),
                border = new(0, 0, 0, 0),
                fixedHeight = 0,
            };

            _toolbarMenuButtonStyle = new(_toolbarRightButtonStyle) {
                padding = new(2, 2, 2, 2),
            };

            _noBinderStyle = new(EditorStyles.miniLabel) {
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

            _binderButtonStyle = new(EditorStyles.toolbarButton) {
                padding = new(0, 0, 0, 0),
                margin = new(1, 0, 0, 0),
                fixedHeight = 0,
                fontSize = GUI.skin.button.fontSize,
                richText = true,
            };

            _binderSelectedButtonStyle = new(_binderButtonStyle) {
                fontStyle = FontStyle.Bold,
                margin = new(1, 2, 0, 0),
            };

            _binderIndexLabelStyle = new(EditorStyles.miniLabel) {
                alignment = TextAnchor.MiddleLeft,
            };

            {
                var style = _binderSelectedButtonStyle;

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

            _bindersHeaderStyle = new() {
                padding = new(0, 0, 0, 0),
                margin = new(0, 0, 0, 0),
                border = new(0, 0, 0, 0),
                stretchHeight = true,
                stretchWidth = true,
            };

            _detailsHeaderStyle = new() {
                padding = new(0, 0, 0, 0),
                margin = new(0, 0, 0, 0),
                border = new(0, 0, 0, 0),
                stretchHeight = true,
                stretchWidth = true,
            };

            _removeButtonStyle = new(EditorStyles.iconButton);

            _indexLabelStyle = new(EditorStyles.miniLabel) {
                padding = new(3, 0, 0, 0),
            };

            _headerLabelStyle = new(EditorStyles.boldLabel) {
                padding = new(0, 0, 0, 0),
                margin = new(0, 0, 0, 0),
                alignment = TextAnchor.MiddleCenter,
            };

            _itemLabelStyle = new(EditorStyles.label) {
                stretchWidth = false,
            };

            {
                var icon = EditorGUIUtility.isProSkin
                    ? EditorGUIUtility.IconContent("d_Toolbar Plus More")
                    : EditorGUIUtility.IconContent("Toolbar Plus More");

                _addLabel = new(icon.image, "Add");
            }

            {
                var icon = EditorGUIUtility.isProSkin
                    ? EditorGUIUtility.IconContent("d_Toolbar Minus")
                    : EditorGUIUtility.IconContent("Toolbar Minus");

                _removeLabel = new(icon.image, "Remove");
                _removeSelectedLabel = new(icon.image, "Remove Selected Item");
            }

            {
                var icon = EditorGUIUtility.IconContent("pane options@2x");
                _menuLabel = new(icon.image, "Menu");
                _menuColor = EditorGUIUtility.isProSkin ? Color.white : Color.black;
            }

            _clearLabel = new("Clear");
            _subtitleLabel = new("Set Subtitle _f2");

            {
                var icon = EditorGUIUtility.isProSkin
                    ? EditorGUIUtility.IconContent("d_console.warnicon")
                    : EditorGUIUtility.IconContent("console.warnicon");

                _iconWarning = new GUIContent(icon.image);
            }

            _iconBinding = EditorGUIUtility.isProSkin
                ? EditorGUIUtility.IconContent("d_BlendTree Icon")
                : EditorGUIUtility.IconContent("BlendTree Icon");

            {
                var icon = EditorGUIUtility.isProSkin
                    ? EditorGUIUtility.IconContent("d_P4_CheckOutRemote")
                    : EditorGUIUtility.IconContent("P4_CheckOutRemote");

                _applyIconLabel = new GUIContent(icon.image, "Apply");
            }

            {
                var icon = EditorGUIUtility.isProSkin
                    ? EditorGUIUtility.IconContent("d_P4_DeletedLocal")
                    : EditorGUIUtility.IconContent("P4_DeletedLocal");

                _cancelIconLabel = new GUIContent(icon.image, "Cancel");
            }

            {
                ColorUtility.TryParseHtmlString("#2C5D87", out var darkColor);
                ColorUtility.TryParseHtmlString("#3A72B0", out var lightColor);
                _selectedColor = EditorGUIUtility.isProSkin ? darkColor : lightColor;
            }

            {
                ColorUtility.TryParseHtmlString("#474747", out var darkColor);
                ColorUtility.TryParseHtmlString("#D6D6D6", out var lightColor);
                _headerColor = EditorGUIUtility.isProSkin ? darkColor : lightColor;
            }

            {
                ColorUtility.TryParseHtmlString("#383838", out var darkColor);
                ColorUtility.TryParseHtmlString("#C8C8C8", out var lightColor);
                _contentColor = EditorGUIUtility.isProSkin ? darkColor : lightColor;
            }

            {
                ColorUtility.TryParseHtmlString("#404040", out var darkColor);
                ColorUtility.TryParseHtmlString("#ABABAB", out var lightColor);
                _altContentColor = EditorGUIUtility.isProSkin ? darkColor : lightColor;
            }

            _bindersLabel = new("Binders");
            _detailsTabLabels = new GUIContent[] { new("Bindings"), new("Targets"), };
            _propertyBindingLabel = new("Property");
            _commandBindingLabel = new("Command");
        }
    }
}

#endif
