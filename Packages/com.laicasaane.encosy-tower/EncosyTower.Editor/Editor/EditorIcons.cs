#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using EncosyTower.Collections;
using EncosyTower.Logging;
using UnityEditor;
using UnityEditor.Experimental;
using UnityEngine;

namespace EncosyTower.Editor
{
    public class EditorIcons : EditorWindow
    {
        [MenuItem("Encosy Tower/Editor Icons")]
        public static void EditorIconsOpen()
        {
            var w = CreateWindow<EditorIcons>("Editor Icons");
            w.Initialize();
            w.ShowUtility();
            w.minSize = new Vector2(320, 450);
        }

        private GUIStyle _iconButtonStyle;
        private GUIStyle _iconPreviewBlack;
        private GUIStyle _iconPreviewWhite;

        private readonly List<GUIContent> _iconContentListAll = new();
        private readonly List<GUIContent> _iconContentListSmall = new();
        private readonly List<GUIContent> _iconContentListBig = new();
        private readonly FasterList<string> _iconNames = new();

        private GUIContent _iconSelected;

        private bool _viewBigIcons = true;
        private bool _darkPreview = true;
        private Vector2 _scroll;
        private int _buttonSize = 70;
        private string _search = "";

        private static bool IsWide => Screen.width > 550;

        private bool DoSearch => !string.IsNullOrWhiteSpace(_search) && _search != "";

        private void Initialize()
        {
            _iconButtonStyle = new GUIStyle(EditorStyles.miniButton) {
                margin = new RectOffset(0, 0, 0, 0),
                fixedHeight = 0
            };

            _iconPreviewBlack = new GUIStyle(_iconButtonStyle);
            EveryTextures(ref _iconPreviewBlack, Texture2DPixel(new(0.15f, 0.15f, 0.15f)));

            _iconPreviewWhite = new GUIStyle(_iconButtonStyle);
            EveryTextures(ref _iconPreviewWhite, Texture2DPixel(new(0.85f, 0.85f, 0.85f)));

            var builtInIconNames = GetBuiltinIconNames();
            _iconNames.AddRange(builtInIconNames);

            var iconContentListAll = _iconContentListAll;
            var iconContentListSmall = _iconContentListSmall;
            var iconContentListBig = _iconContentListBig;

            var iconNames = GetIconNames();
            var iconNamesLength = iconNames.Length;

            for (var i = 0; i < iconNamesLength; i++)
            {
                var iconName = iconNames[i];
                var icon = GetIcon(iconName);

                if (icon == null || icon.image == false)
                {
                    continue;
                }

                icon.tooltip = iconName;
                iconContentListAll.Add(icon);

                if (icon.image.width <= 36 || icon.image.height <= 36)
                {
                    iconContentListSmall.Add(icon);
                }
                else
                {
                    iconContentListBig.Add(icon);
                }
            }
        }

        private ReadOnlySpan<string> GetIconNames()
            => _iconNames.AsReadOnlySpan();

        private void OnGUI()
        {
            var ppp = EditorGUIUtility.pixelsPerPoint;

            if (!IsWide)
            {
                SearchGUI();
            }

            using (new GUILayout.HorizontalScope(EditorStyles.toolbar))
            {
                if (GUILayout.Button("Save all icons to folder...", EditorStyles.miniButton))
                {
                    SaveAllIcons();
                }

                GUILayout.Label("Select what icons to show", GUILayout.Width(160));

                _viewBigIcons = GUILayout.SelectionGrid(
                      _viewBigIcons ? 1 : 0
                    , new[] { "Small", "Big" }
                    , 2
                    , EditorStyles.toolbarButton
                ) == 1;

                if (IsWide)
                {
                    SearchGUI();
                }
            }

            if (IsWide)
            {
                GUILayout.Space(3);
            }

            using (var scope = new GUILayout.ScrollViewScope(_scroll))
            {
                GUILayout.Space(10);

                _scroll = scope.scrollPosition;

                _buttonSize = _viewBigIcons ? 70 : 40;

                // scrollbar_width = ~ 12.5
                var renderWidth = (Screen.width / ppp - 13f);
                var gridW = Mathf.FloorToInt( renderWidth / _buttonSize );
                var marginLeft = ( renderWidth - _buttonSize * gridW ) / 2;

                int row = 0, index = 0;

                List<GUIContent> iconList = DoSearch
                    ? _iconContentListAll.Where(x => x.tooltip.ToLower().Contains(_search.ToLower())).ToList()
                    : _viewBigIcons ? _iconContentListBig : _iconContentListSmall;

                while (index < iconList.Count)
                {
                    using (new GUILayout.HorizontalScope())
                    {
                        GUILayout.Space(marginLeft);

                        for (var i = 0; i < gridW; ++i)
                        {
                            var k = i + row * gridW;
                            var icon = iconList[ k ];

                            if (GUILayout.Button(icon, _iconButtonStyle, GUILayout.Width(_buttonSize), GUILayout.Height(_buttonSize)))
                            {
                                EditorGUI.FocusTextInControl("");
                                _iconSelected = icon;
                            }

                            index++;

                            if (index == iconList.Count)
                            {
                                break;
                            }
                        }
                    }

                    row++;
                }

                GUILayout.Space(10);
            }


            if (_iconSelected == null)
            {
                return;
            }

            GUILayout.FlexibleSpace();

            using (new GUILayout.HorizontalScope(EditorStyles.helpBox, GUILayout.MaxHeight(_viewBigIcons ? 140 : 120)))
            {
                using (new GUILayout.VerticalScope(GUILayout.Width(130)))
                {
                    GUILayout.Space(2);

                    GUILayout.Button(
                          _iconSelected
                        , _darkPreview ? _iconPreviewBlack : _iconPreviewWhite
                        , GUILayout.Width(128)
                        , GUILayout.Height(_viewBigIcons ? 128 : 40)
                    );

                    GUILayout.Space(5);

                    _darkPreview = GUILayout.SelectionGrid(
                          _darkPreview ? 1 : 0
                        , new[] { "Light", "Dark" }
                        , 2
                        , EditorStyles.miniButton
                    ) == 1;

                    GUILayout.FlexibleSpace();
                }

                GUILayout.Space(10);

                using (new GUILayout.VerticalScope())
                {
                    var isDark = _iconSelected.tooltip.IndexOf("d_", StringComparison.Ordinal) == 0;
                    var s = $"Size: {_iconSelected.image.width}x{_iconSelected.image.height}";
                    s += "\nIs Pro Skin Icon: " + (isDark ? "Yes" : "No");
                    s += $"\nTotal {_iconContentListAll.Count} icons";

                    GUILayout.Space(5);
                    EditorGUILayout.HelpBox(s, MessageType.None);
                    GUILayout.Space(5);
                    EditorGUILayout.TextField("EditorGUIUtility.IconContent(\"" + _iconSelected.tooltip + "\")");
                    GUILayout.Space(5);

                    if (GUILayout.Button("Copy to clipboard", EditorStyles.miniButton))
                    {
                        EditorGUIUtility.systemCopyBuffer = _iconSelected.tooltip;
                    }

                    if (GUILayout.Button("Save icon to file ...", EditorStyles.miniButton))
                    {
                        SaveIcon(_iconSelected.tooltip);
                    }
                }

                GUILayout.Space(10);

                if (GUILayout.Button("X", GUILayout.ExpandHeight(true)))
                {
                    _iconSelected = null;
                }

            }
        }

        private void SearchGUI()
        {
            using (new GUILayout.HorizontalScope())
            {
                if (IsWide) GUILayout.Space(10);

                _search = EditorGUILayout.TextField(_search, EditorStyles.toolbarSearchField);

#if UNITY_6000_0_OR_NEWER
                var icon = EditorAPI.GetIcon("d_winbtn_mac_close_a", "winbtn_mac_close_a");
#else
                var icon = EditorAPI.GetIcon("d_winbtn_mac_close_h", "winbtn_mac_close_h");
#endif

                if (GUILayout.Button(icon, EditorStyles.toolbarButton, GUILayout.Width(22)))
                {
                    _search = "";
                }
            }
        }

        private void SaveIcon(string iconName)
        {
            if (EditorGUIUtility.IconContent(iconName).image is not Texture2D tex)
            {
                DevLoggerAPI.LogError($"Cannot save the icon '{iconName}'");
                return;
            }

            var path = EditorUtility.SaveFilePanel("Save icon", "", iconName, "png");

            if (path == null)
            {
                return;
            }

            try
            {
                var outTex = new Texture2D(tex.width, tex.height, tex.format, tex.mipmapCount, true);

                Graphics.CopyTexture(tex, outTex);
                File.WriteAllBytes(path, outTex.EncodeToPNG());
            }
            catch (Exception ex)
            {
                DevLoggerAPI.LogException(ex);
            }
        }

        private void SaveAllIcons()
        {
            var folderpath = EditorUtility.SaveFolderPanel("", "", "");

            try
            {
                var iconNames = GetIconNames();
                var iconNamesLength = iconNames.Length;

                for (var i = 0; i < iconNamesLength; i++)
                {
                    var iconName = iconNames[i];
                    var split = iconName.Split('/').Last();

                    if (EditorGUIUtility.IconContent(iconName).image is not Texture2D tex)
                    {
                        continue;
                    }

                    if (string.IsNullOrWhiteSpace(folderpath))
                    {
                        DevLoggerAPI.LogError("Folder path invalid...");
                        break;
                    }

                    var path = folderpath + "//" + $"{split}.png";

                    if (File.Exists(path))
                    {
                        DevLoggerAPI.LogWarning($"File already exists at {path}");
                    }
                    else
                    {
                        var outTex = new Texture2D(tex.width, tex.height, tex.format, tex.mipmapCount, true);

                        Graphics.CopyTexture(tex, outTex);
                        File.WriteAllBytes(path, outTex.EncodeToPNG());
                    }
                }
            }
            catch (Exception ex)
            {
                DevLoggerAPI.LogException(ex);
            }
        }

        // https://github.com/SolarianZ/UnityBuiltinUIResBrowser/blob/main/Editor/Scripts/BuiltinUIResUtility.cs#L20
        private static HashSet<string> GetBuiltinIconNames(AssetBundle editorAssetBundle = null)
        {
            if (editorAssetBundle == false)
            {
                editorAssetBundle = GetEditorAssetBundle();
            }

            var shortNames = new HashSet<string>();
            var iconsPath = EditorResources.iconsPath;

            foreach (string assetName in editorAssetBundle.GetAllAssetNames())
            {
                var startsWithIconPath = assetName.StartsWith(iconsPath, StringComparison.OrdinalIgnoreCase);
                var isPng = assetName.EndsWith(".png", StringComparison.OrdinalIgnoreCase);
                var isAsset = assetName.EndsWith(".asset", StringComparison.OrdinalIgnoreCase);

                if (startsWithIconPath == false && isPng == false)
                {
                    continue;
                }

                if (isPng == false && isAsset == false)
                {
                    continue;
                }

                var shortName = Path.GetFileNameWithoutExtension(assetName);
                shortNames.Add(shortName);
            }

            return shortNames;

            static AssetBundle GetEditorAssetBundle()
            {
                var getEditorAssetBundle = typeof(EditorGUIUtility).GetMethod(
                    "GetEditorAssetBundle", BindingFlags.NonPublic | BindingFlags.Static
                );

                return (AssetBundle)getEditorAssetBundle.Invoke(null, null);
            }
        }

        private static GUIContent GetIcon(string iconName)
        {
            if (string.IsNullOrEmpty(iconName))
            {
                return null;
            }

            var logEnabled = Debug.unityLogger.logEnabled;
            Debug.unityLogger.logEnabled = false;

            var obj = EditorGUIUtility.LoadRequired(iconName);

            Debug.unityLogger.logEnabled = logEnabled;

            return obj is Texture texture ? new GUIContent(texture) : null;
        }

        private static void EveryTextures(ref GUIStyle s, Texture2D t)
        {
            s.hover.background
                = s.onHover.background
                = s.focused.background
                = s.onFocused.background
                = s.active.background
                = s.onActive.background
                = s.normal.background
                = s.onNormal.background
                = t;

            s.hover.scaledBackgrounds
                = s.onHover.scaledBackgrounds
                = s.focused.scaledBackgrounds
                = s.onFocused.scaledBackgrounds
                = s.active.scaledBackgrounds
                = s.onActive.scaledBackgrounds
                = s.normal.scaledBackgrounds
                = s.onNormal.scaledBackgrounds
                = new[] { t };
        }

        private static Texture2D Texture2DPixel(in Color c)
        {
            var t = new Texture2D(1,1);
            t.SetPixel(0, 0, c);
            t.Apply();
            return t;
        }
    }
}

#endif
