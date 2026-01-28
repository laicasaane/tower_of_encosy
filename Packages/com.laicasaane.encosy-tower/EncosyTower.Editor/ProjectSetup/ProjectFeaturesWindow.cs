#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using EncosyTower.Common;
using EncosyTower.Editor.UIElements;
using EncosyTower.UIElements;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace EncosyTower.Editor.ProjectSetup
{
    public sealed class ProjectFeaturesWindow : EditorWindow
    {
        private const string MODULE_ROOT = $"{EditorStyleSheetPaths.ROOT}/EncosyTower.Editor/ProjectSetup";
        private const string STYLE_SHEETS_PATH = $"{MODULE_ROOT}/StyleSheets";
        private const string FILE_NAME = nameof(ProjectFeaturesWindow);

        public const string THEME_STYLE_SHEET = $"{STYLE_SHEETS_PATH}/{FILE_NAME}.uss";

        private const string LIST_TITLE = "Package Information";
        private const string LIST_MSG = "Retrieving installed packages...";

        private const string INSTALL_TITLE = "Install Selected Packages";
        private const string INSTALL_MSG = "Do you want to install selected packages?";
        private const string INSTALLING_MSG = "Installing selected packages...";

        private const string OPEN_UPM_CLI_TITLE = "OpenUPM CLI";
        private const string OPEN_UPM_CLI_MSG = "Cannot find OpenUPM CLI on your machine. Please install it first.";

        private const string UNITY_UPM_URL = "https://docs.unity3d.com/Packages/{0}@latest";
        private const string OPEN_UPM_URL = "https://openupm.com/packages/{0}";

        private const float DEFAULT_LEFT_PANE_WIDTH = 300f;

        private static ListRequest s_listRequest;
        private static AddAndRemoveRequest s_addRequest;
        private static StringBuilder s_openUpmSb;
        private static List<string> s_unityIdentifiers;

        [SerializeField, HideInInspector]
        private FeatureCollectionAsset _featureCollectionAsset;

        [SerializeField, HideInInspector]
        private PackageCollectionAsset _packageCollectionAsset;

        private readonly List<string> _features = new();
        private readonly Dictionary<string, PackageInfo[]> _featureMap = new(StringComparer.Ordinal);
        private readonly List<PackageInfo> _selectedPackages = new();

        private bool _initAfterRequest = false;
        private SimpleTableView<string> _featureTable;
        private SimpleTableView<PackageInfo> _packageTable;

        private VisualElement _requestInfoContainer;
        private VisualElement _featureContainer;
        private ToolbarButton _installButton;

        [MenuItem("Encosy Tower/Project Settings/Features", priority = 80_70_00_00)]
        public static void OpenWindow()
        {
            var window = GetWindow<ProjectFeaturesWindow>();
            window.titleContent = new GUIContent("Project Features");
            window.Show();
        }

        public void OnEnable()
        {
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;

            _initAfterRequest = false;
            s_listRequest = Client.List(true, true);
            EditorApplication.update -= ListRequestProgress;
            EditorApplication.update += ListRequestProgress;
        }

        public void OnDestroy()
        {
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;

            if (s_listRequest != null)
            {
                EditorApplication.update -= ListRequestProgress;
                s_listRequest = null;
            }

            if (s_addRequest != null)
            {
                EditorApplication.update -= AddRequestProgress;
                s_addRequest = null;
            }
        }

        private void OnPlayModeStateChanged(PlayModeStateChange _)
        {
            Close();
        }

        private void ListRequestProgress()
        {
            if (s_listRequest == null)
            {
                return;
            }

            if (s_listRequest.IsCompleted == false)
            {
                EditorUtility.DisplayProgressBar(LIST_TITLE, LIST_MSG, 0f);
                return;
            }

            EditorApplication.update -= ListRequestProgress;
            EditorUtility.ClearProgressBar();

            OnListRequestCompleted();
        }

        private void OnListRequestCompleted()
        {
            var result = s_listRequest.Result;
            s_listRequest = null;

            var installedPackages = new HashSet<string>();

            foreach (var item in result)
            {
                var name = "";

                if (item.source == PackageSource.Git)
                {
                    try
                    {
                        var packageId = item.packageId;
                        var indexOfAt = packageId.IndexOf('@');

                        if (indexOfAt >= 0)
                        {
                            name = packageId.Substring(indexOfAt + 1, packageId.Length - indexOfAt - 1);
                        }
                    }
                    catch
                    {
                        // ignored
                    }
                }

                if (string.IsNullOrWhiteSpace(name))
                {
                    name = item.name;
                }

                installedPackages.Add(name);
            }

            GetFeatures(installedPackages, _features, _featureMap);
        }

        private void CreateGUI()
        {
            rootVisualElement.WithEditorStyleSheet(THEME_STYLE_SHEET);

            var requestInfoContainer = _requestInfoContainer = new VisualElement() {
                name = "request-info-container",
            };

            rootVisualElement.Add(requestInfoContainer);

            var requestInfoToolbar = new Toolbar();
            requestInfoContainer.Add(requestInfoToolbar);

            requestInfoToolbar.Add(new ToolbarButton(Refresh) {
                text = "Refresh"
            }.WithClass("refresh-button"));

            requestInfoContainer.Add(new Label(LIST_MSG) {
                name = "request-info-label",
            });

            var featureContainer = _featureContainer = new VisualElement() {
                name = "feature-container",
            }.WithDisplay(DisplayStyle.None);

            rootVisualElement.Add(featureContainer);

            var featureToolbar = new Toolbar();
            featureContainer.Add(featureToolbar);

            featureToolbar.Add(new ToolbarButton(Refresh) {
                text = "Refresh",
            }.WithClass("refresh-button"));

            featureToolbar.Add(new ToolbarSpacer());

            featureToolbar.Add(_installButton = new ToolbarButton(Install) {
                text = "Install",
                name = "install-button",
                enabledSelf = false,
            });

            var splitter = new TwoPaneSplitView(0, DEFAULT_LEFT_PANE_WIDTH, TwoPaneSplitViewOrientation.Horizontal);

            CreateFeatureTable(splitter);
            CreatePackageTable(splitter);

            featureContainer.Add(splitter);
        }

        private void OnGUI()
        {
            if (_initAfterRequest == false
                && s_listRequest == null
                && _requestInfoContainer != null
                && _featureContainer != null
            )
            {
                _initAfterRequest = true;
                _requestInfoContainer.WithDisplay(DisplayStyle.None);
                _featureContainer.WithDisplay(DisplayStyle.Flex);

                _featureCollectionAsset.features = _featureTable.Items = _features;
                _packageCollectionAsset.packages = _packageTable.Items = _selectedPackages;
            }

            var packages = _selectedPackages;

            if (_initAfterRequest == false || packages.Count < 1)
            {
                return;
            }

            var count = 0;

            foreach (var package in packages)
            {
                if (package.ShouldBeInstalled())
                {
                    count += 1;
                }
            }

            _installButton.enabledSelf = count > 0;
        }

        private void Refresh()
        {
            _requestInfoContainer.WithDisplay(DisplayStyle.Flex);
            _featureContainer.WithDisplay(DisplayStyle.None);

            _features.Clear();
            _featureMap.Clear();
            _selectedPackages.Clear();

            OnEnable();
        }

        private void Install()
        {
            ApplySelectedPackages(_selectedPackages);
        }

        private void CreateFeatureTable(VisualElement container)
        {
            var table = _featureTable = new SimpleTableView<string>() {
                name = "feature-table",
                alternateRows = false,
                bindingPath = nameof(FeatureCollectionAsset.features),
                showBoundCollectionSize = false,
            }.WithEditorStyleSheets();

            container.Add(table);

            var column = table.AddColumn("Features"
                , DEFAULT_LEFT_PANE_WIDTH
                , FeatureTableColumn_MakeCell_Feature
                , FeatureTableColumn_BindCell_Feature
                , header: new(static (a, b) => string.Compare(a, b, StringComparison.Ordinal))
            );

            column.resizable = true;
            column.sortable = true;

            var asset = _featureCollectionAsset = CreateInstance<FeatureCollectionAsset>();
            var serializedObject = new SerializedObject(asset);
            _featureTable.Bind(serializedObject);
        }

        private Button FeatureTableColumn_MakeCell_Feature()
        {
            var button = new Button();
            button.AddToClassList("feature-button");

            button.clicked += () => {
                if (button.userData is string feature && feature.IsNotEmpty())
                {
                    _selectedPackages.Clear();

                    if (_featureMap.TryGetValue(feature, out var packages))
                    {
                        _selectedPackages.AddRange(packages);
                    }

                    _packageTable.Rebuild();
                }
            };

            return button;
        }

        private static void FeatureTableColumn_BindCell_Feature(VisualElement element, string item)
        {
            if (element is not Button button || string.IsNullOrEmpty(item))
            {
                return;
            }

            button.text = item;
            button.userData = item;
        }

        private void CreatePackageTable(VisualElement container)
        {
            var table = _packageTable = new SimpleTableView<PackageInfo>() {
                name = "package-table",
                bindingPath = nameof(PackageCollectionAsset.packages),
                showBoundCollectionSize = false,
            }.WithEditorStyleSheets();

            container.Add(table);

            var column = table.AddColumn("Install"
                , 60
                , PackageTableColumn_MakeCell_Install
                , PackageTableColumn_BindCell_Install
            );

            column.maxWidth = 60;

            column = table.AddColumn("Registry"
                , 80
                , PackageTableColumn_MakeCell_Registry
                , PackageTableColumn_BindCell_Registry
                , header: new(PackageInfo.CompareRegistries)
            );

            column.maxWidth = 80;
            column.sortable = true;

            column = table.AddColumn("Package Name"
                , 200
                , PackageTableColumn_MakeCell_PackageName
                , PackageTableColumn_BindCell_PackageName
                , header: new(PackageInfo.CompareNames)
            );

            column.resizable = true;
            column.sortable = true;

            column = table.AddColumn("Version"
                , 150
                , PackageTableColumn_MakeCell_Version
                , PackageTableColumn_BindCell_Version
            );

            column.maxWidth = 150;

            var asset = _packageCollectionAsset = CreateInstance<PackageCollectionAsset>();
            var serializedObject = new SerializedObject(asset);
            _packageTable.Bind(serializedObject);
        }

        private static ToggleOrImage PackageTableColumn_MakeCell_Install()
        {
            var toi = new ToggleOrImage();
            toi.Image.image = EditorGUIUtility.IconContent("Installed").image;
            toi.Image.tooltip = "Installed";
            toi.AddToClassList("install-toggle-or-image");

            return toi;
        }

        private static void PackageTableColumn_BindCell_Install(VisualElement element, PackageInfo item)
        {
            if (element is not ToggleOrImage tol || item is null)
            {
                return;
            }

            tol.Toggle.userData = null;
            tol.Toggle.UnregisterValueChangedCallback(OnValueChanged);

            tol.SetToggle(item.isInstalled == false);

            if (item.isInstalled)
            {
                return;
            }

            if (item.isOptional == false)
            {
                tol.Toggle.SetValueWithoutNotify(true);
                return;
            }

            tol.Toggle.SetValueWithoutNotify(item.isChosen);
            tol.Toggle.userData = item;
            tol.Toggle.RegisterValueChangedCallback(OnValueChanged);

            static void OnValueChanged(ChangeEvent<bool> evt)
            {
                if (evt.currentTarget is not Toggle toggle || toggle.userData is not PackageInfo item)
                {
                    return;
                }

                item.isChosen = evt.newValue;
            }
        }

        private static Label PackageTableColumn_MakeCell_Registry()
        {
            var label = new Label();
            label.AddToClassList("registry-label");

            return label;
        }

        private static void PackageTableColumn_BindCell_Registry(VisualElement element, PackageInfo item)
        {
            if (element is not Label label || item is null)
            {
                return;
            }

            label.text = item.registry switch {
                PackageRegistry.Unity => "Unity",
                PackageRegistry.OpenUpm => "OpenUPM",
                PackageRegistry.GitUrl => "Git URL",
                _ => "Unknown",
            };
        }

        private static Label PackageTableColumn_MakeCell_PackageName()
        {
            var label = new Label();
            label.AddToClassList("package-name-label");

            return label;
        }

        private static void PackageTableColumn_BindCell_PackageName(VisualElement element, PackageInfo item)
        {
            if (element is not Label label || item is null)
            {
                return;
            }

            label.UnregisterCallback<PointerDownEvent>(OnPointerDown);

            if (string.IsNullOrEmpty(item.url))
            {
                label.text = string.Empty;
                label.tooltip = string.Empty;
                label.userData = null;
                return;
            }

            label.text = item.name;
            label.tooltip = item.url;
            label.userData = item.url;
            label.RegisterCallback<PointerDownEvent>(OnPointerDown);

            static void OnPointerDown(PointerDownEvent evt)
            {
                if (evt.currentTarget is Label label && label.userData is string url)
                {
                    Application.OpenURL(url);
                }
            }
        }

        private static Label PackageTableColumn_MakeCell_Version()
        {
            var label = new Label();
            label.AddToClassList("version-label");

            return label;
        }

        private static void PackageTableColumn_BindCell_Version(VisualElement element, PackageInfo item)
        {
            if (element is not Label label || item is null)
            {
                return;
            }

            label.text = item.version;
        }

        private static void GetFeatures(
              HashSet<string> installedPackages
            , List<string> features
            , Dictionary<string, PackageInfo[]> featureMap
        )
        {
            features.Clear();
            featureMap.Clear();

            var types = UnityEditor.TypeCache.GetTypesWithAttribute<FeatureAttribute>();
            var packages = new HashSet<PackageInfo>();

            foreach (var type in types)
            {
                var featureAttrib = type.GetCustomAttribute<FeatureAttribute>();

                if (string.IsNullOrWhiteSpace(featureAttrib?.Name))
                {
                    continue;
                }

                packages.Clear();

                var featureName = featureAttrib.Name;

                if (featureMap.Remove(featureName, out var featurePackages))
                {
                    foreach (var package in featurePackages)
                    {
                        packages.Add(package);
                    }
                }

                var packageAttribs = type.GetCustomAttributes<RequiresPackageAttribute>();

                foreach (var packageAttrib in packageAttribs)
                {
                    if (string.IsNullOrWhiteSpace(packageAttrib?.PackageName))
                    {
                        continue;
                    }

                    var packageName = packageAttrib.PackageName;

                    var package = new PackageInfo {
                        registry = packageAttrib.Registry,
                        name = packageName,
                        version = packageAttrib.Version ?? string.Empty,
                        isOptional = packageAttrib.IsOptional,
                        isInstalled = installedPackages.Contains(packageName),
                        isChosen = !packageAttrib.IsOptional,
                        url = packageAttrib.Registry switch {
                            PackageRegistry.Unity => string.Format(UNITY_UPM_URL, packageName),
                            PackageRegistry.OpenUpm => string.Format(OPEN_UPM_URL, packageName),
                            PackageRegistry.GitUrl => packageName,
                            _ => string.Empty,
                        },
                    };

                    packages.Add(package);
                }

                if (packages.Count < 1)
                {
                    continue;
                }

                featureMap[featureName] = packages
                    .OrderBy(static x => x.name)
                    .ThenBy(static x => x.registry)
                    .ToArray();
            }

            features.AddRange(featureMap.Keys.OrderBy(static x => x));
        }

        private static void ApplySelectedPackages(List<PackageInfo> packages)
        {
            if (packages == null || packages.Count < 1)
            {
                return;
            }

            if (EditorUtility.DisplayDialog(INSTALL_TITLE, INSTALL_MSG, "Proceed", "Cancel") == false)
            {
                return;
            }

            EditorUtility.DisplayProgressBar(INSTALL_TITLE, INSTALLING_MSG, 0f);

            if (ExistsOnPath("openupm") == false)
            {
                EditorUtility.ClearProgressBar();
                EditorUtility.DisplayDialog(OPEN_UPM_CLI_TITLE, OPEN_UPM_CLI_MSG, "OK");
                return;
            }

            EditorUtility.DisplayProgressBar(INSTALL_TITLE, INSTALLING_MSG, 0.25f);

            var length = packages.Count;
            var unityIdentifiers = s_unityIdentifiers = new List<string>(length);
            var openUpmSb = s_openUpmSb = new StringBuilder();
            var firstOpenUpm = true;

            for (var i = 0; i < length; i++)
            {
                var package = packages[i];
                var shouldBeInstalled = package.ShouldBeInstalled();

                if (shouldBeInstalled == false || string.IsNullOrWhiteSpace(package.name))
                {
                    continue;
                }

                switch (package.registry)
                {
                    case PackageRegistry.Unity:
                    {
                        unityIdentifiers.Add(
                            string.IsNullOrWhiteSpace(package.version)
                                ? package.name
                                : $"{package.name}@{package.version}"
                        );
                        break;
                    }

                    case PackageRegistry.GitUrl:
                    {
                        unityIdentifiers.Add(package.name);
                        break;
                    }

                    case PackageRegistry.OpenUpm:
                    {
                        if (firstOpenUpm)
                        {
                            openUpmSb.Append("openupm add");
                        }

                        firstOpenUpm = false;
                        openUpmSb.Append(' ').Append(package.name);

                        if (string.IsNullOrWhiteSpace(package.version) == false)
                        {
                            openUpmSb.Append('@').Append(package.version);
                        }
                        break;
                    }
                }
            }

            ApplyOpenUpmPackages();
            ApplyUnityPackages();
        }

        public static void ApplyOpenUpmPackages()
        {
            if (s_openUpmSb.Length < 1)
            {
                return;
            }

            EditorUtility.DisplayProgressBar(INSTALL_TITLE, INSTALLING_MSG, 0.5f);

            try
            {
                using var p = new Process();
                p.StartInfo.UseShellExecute = false;
                p.StartInfo.RedirectStandardInput = true;
                p.StartInfo.RedirectStandardOutput = true;
                p.StartInfo.CreateNoWindow = true;
                p.StartInfo.FileName = "cmd.exe";
                p.Start();
                p.StandardInput.WriteLine(s_openUpmSb.ToString());
                p.StandardInput.Flush();
                p.StandardInput.Close();
                p.WaitForExit();
            }
            catch (Exception ex)
            {
                EditorUtility.DisplayDialog(INSTALL_TITLE, ex.ToString(), "OK");
            }
        }

        public static void ApplyUnityPackages()
        {
            if (s_unityIdentifiers.Count < 1)
            {
                return;
            }

            s_addRequest = Client.AddAndRemove(packagesToAdd: s_unityIdentifiers.ToArray());

            EditorApplication.update -= AddRequestProgress;
            EditorApplication.update += AddRequestProgress;
        }

        private static void AddRequestProgress()
        {
            if (s_addRequest == null)
            {
                return;
            }

            if (s_addRequest.IsCompleted == false)
            {
                EditorUtility.DisplayProgressBar(INSTALL_TITLE, INSTALLING_MSG, 0.75f);
                return;
            }

            EditorApplication.update -= AddRequestProgress;
            EditorUtility.ClearProgressBar();

            s_addRequest = null;
            s_openUpmSb = null;
            s_unityIdentifiers = null;
        }

        private static bool ExistsOnPath(string exeName)
        {
            try
            {
                using var p = new Process();
                p.StartInfo.UseShellExecute = false;
                p.StartInfo.FileName = "where";
                p.StartInfo.Arguments = exeName;
                p.Start();
                p.WaitForExit();
                return p.ExitCode == 0;
            }
            catch
            {
                return false;
            }
        }

        private class FeatureCollectionAsset : ScriptableObject
        {
            public List<string> features;
        }

        [Serializable]
        private class PackageInfo : IEquatable<PackageInfo>
        {
            public PackageRegistry registry;
            public string name;
            public string version;
            public string url;
            public bool isOptional;
            public bool isInstalled;
            public bool isChosen;

            public static int CompareNames(PackageInfo a, PackageInfo b)
                => string.Compare(a.name, b.name, StringComparison.Ordinal);

            public static int CompareRegistries(PackageInfo a, PackageInfo b)
                => a.registry - b.registry;

            public override int GetHashCode()
                => HashCode.Combine(name);

            public override bool Equals(object obj)
                => obj is PackageInfo other && Equals(other);

            public bool Equals(PackageInfo other)
                => string.Equals(name, other?.name, StringComparison.Ordinal);

            public bool ShouldBeInstalled()
                => isInstalled == false && (isOptional == false || isChosen);
        }

        private class PackageCollectionAsset : ScriptableObject
        {
            public List<PackageInfo> packages;
        }

        private class ToggleOrImage : VisualElement
        {
            public static readonly string UssClassName = "toggle-or-image";
            public static readonly string ToggleUssClassName = $"{UssClassName}__toggle";
            public static readonly string ImageUssClassName = $"{UssClassName}__image";

            public readonly Toggle Toggle;
            public readonly Image Image;

            public ToggleOrImage()
            {
                AddToClassList(UssClassName);

                Add(Toggle = new());
                Toggle.AddToClassList(ToggleUssClassName);
                Toggle.WithDisplay(DisplayStyle.Flex);

                Add(Image = new());
                Image.AddToClassList(ImageUssClassName);
                Image.WithDisplay(DisplayStyle.None);
            }

            public void SetToggle(bool value)
            {
                Toggle.WithDisplay(value ? DisplayStyle.Flex : DisplayStyle.None);
                Image.WithDisplay(value ? DisplayStyle.None : DisplayStyle.Flex);
            }
        }
    }
}

#endif
