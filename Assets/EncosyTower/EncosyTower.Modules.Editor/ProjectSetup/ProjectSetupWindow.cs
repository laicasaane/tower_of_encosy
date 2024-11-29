#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEngine;

namespace EncosyTower.Modules.Editor.ProjectSetup
{
    public sealed class ProjectSetupWindow : EditorWindow
    {
        [MenuItem("Encosy Tower/Project Setup")]
        public static void OpenWindow()
        {
            var window = GetWindow<ProjectSetupWindow>();
            window.titleContent = new GUIContent("Project Setup");
            window.Show();
        }

        private const string LIST_TITLE = "Package Information";
        private const string LIST_MSG = "Retrieving installed packages...";

        private const string INSTALL_TITLE = "Install Selected Packages";
        private const string INSTALL_MSG = "Do you want to install selected packages?";
        private const string INSTALLING_MSG = "Installing selected packages...";

        private const string OPEN_UPM_CLI_TITLE = "OpenUPM CLI";
        private const string OPEN_UPM_CLI_MSG = "Cannot find OpenUPM CLI on your machine. Please install it first.";

        private const string UNITY_UPM_URL = "https://docs.unity3d.com/Packages/{0}@latest";
        private const string OPEN_UPM_URL = "https://openupm.com/packages/{0}";

        private static ListRequest s_listRequest;
        private static AddAndRemoveRequest s_addRequest;
        private static StringBuilder s_openUpmSb;
        private static List<string> s_unityIdentifiers;

        private Dictionary<string, PackageInfo[]> _features;
        private SimpleTableView<PackageInfo> _table;
        private PackageInfo[] _selectedPackages = Array.Empty<PackageInfo>();
        private GUIStyle _featureButtonStyle;
        private Vector2 _scrollViewPos;
        private float? _featureTitleWidth;

        private readonly GUIContent _featureTitle = new();

        private static Dictionary<string, PackageInfo[]> GetFeatures(HashSet<string> installedPackages)
        {
            var types = UnityEditor.TypeCache.GetTypesWithAttribute<FeatureAttribute>();
            var result = new Dictionary<string, PackageInfo[]>(StringComparer.Ordinal);
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

                if (result.Remove(featureName, out var featurePackages))
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

                result[featureName] = packages
                    .OrderBy(static x => x.name)
                    .ThenBy(static x => x.registry)
                    .ToArray();
            }

            return result;
        }

        public void OnEnable()
        {
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;

            s_listRequest = Client.List(true, true);
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

            _features = GetFeatures(installedPackages);
        }

        public void OnGUI()
        {
            var features = _features;

            if (features == null || features.Count < 1)
            {
                EditorGUILayout.LabelField(LIST_MSG);
                return;
            }

            BuildTable();

            var featureTitle = _featureTitle;
            var featureButtonStyle = _featureButtonStyle;

            EditorGUILayout.BeginHorizontal();
            {
                var isPressed = false;
                var selectedPackages = _selectedPackages.AsSpan();
                var selectedPackagesLength = selectedPackages.Length;
                var selectedCount = 0;

                for (var i = 0; i < selectedPackagesLength; i++)
                {
                    ref readonly var package = ref selectedPackages[i];
                    selectedCount += package.ShouldBeInstalled() ? 1 : 0;
                }

                if (selectedCount < 1)
                {
                    var enabled = GUI.enabled;
                    GUI.enabled = false;
                    GUILayout.Button(INSTALL_TITLE);
                    GUI.enabled = enabled;
                }
                else
                {
                    isPressed = GUILayout.Button(INSTALL_TITLE);
                }

                if (isPressed)
                {
                    ApplySelectedPackages(_selectedPackages);
                }
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal();

            if (_featureTitleWidth.HasValue == false)
            {
                var maxValue = 0f;

                foreach (var (feature, _) in features)
                {
                    featureTitle.text = feature;
                    featureButtonStyle.CalcMinMaxWidth(featureTitle, out _, out var max);

                    maxValue = Math.Max(max, maxValue);
                }

                _featureTitleWidth = maxValue;
            }

            var featureTitleWidth = _featureTitleWidth ?? 200f;
            var scrollWidth = featureTitleWidth + 40f;

            _scrollViewPos = EditorGUILayout.BeginScrollView(_scrollViewPos, GUILayout.MinWidth(scrollWidth));

            featureButtonStyle.contentOffset = new((scrollWidth - featureTitleWidth) / 2f, 0f);

            foreach (var (feature, packages) in features.OrderBy(static x => x.Key))
            {
                featureTitle.text = feature;

                if (GUILayout.Button(featureTitle, featureButtonStyle))
                {
                    _selectedPackages = packages;
                }
            }

            EditorGUILayout.EndScrollView();
            EditorGUILayout.Space();
            EditorGUILayout.BeginVertical();
            _table.DrawTableGUI(_selectedPackages, rowHeight: EditorGUIUtility.singleLineHeight * 1.3f);
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();
        }

        private void BuildTable()
        {
            if (_table != null)
            {
                return;
            }

            _featureButtonStyle = new(GUI.skin.button) {
                alignment = TextAnchor.MiddleLeft,
            };

            var table = _table = new();

            table.AddColumn("Install", 60, TableColumn_Choose)
                .SetMaxWidth(60);

            table.AddColumn("Registry", 80, TableColumn_Registry)
                .SetMaxWidth(80)
                .SetSorting(static (a, b) => a.registry - b.registry);

            table.AddColumn("Package Name", 200, TableColumn_Name)
                .SetAutoResize(true)
                .SetSorting((a, b) => string.Compare(a.name, b.name, StringComparison.Ordinal));

            table.AddColumn("Version", 150, TableColumn_Version)
                .SetMaxWidth(150);
        }

        private static void TableColumn_Choose(Rect rect, PackageInfo item)
        {
            rect.xMin += 20;

            if (item.isInstalled)
            {
                var iconSize = rect.height - 4f;
                var iconRect = new Rect(rect.x - 3f, rect.y + 2f, iconSize, iconSize);
                var icon = EditorGUIUtility.IconContent("Installed@2x");
                icon.tooltip = "Installed";
                EditorGUI.LabelField(iconRect, icon);
                return;
            }

            if (item.isOptional)
            {
                item.isChosen = EditorGUI.Toggle(position: rect, value: item.isChosen);
            }
            else
            {
                var enabled = GUI.enabled;
                GUI.enabled = false;
                EditorGUI.Toggle(position: rect, value: true);
                GUI.enabled = enabled;
            }
        }

        private static void TableColumn_Registry(Rect rect, PackageInfo item)
        {
            var label = item.registry switch
            {
                PackageRegistry.Unity => "Unity",
                PackageRegistry.OpenUpm => "OpenUPM",
                PackageRegistry.GitUrl => "Git URL",
                _ => "Unknown",
            };

            EditorGUI.SelectableLabel(rect, label);
        }

        private static void TableColumn_Name(Rect rect, PackageInfo item)
        {
            if (string.IsNullOrWhiteSpace(item.url))
            {
                return;
            }

            var label = new GUIContent(item.name, tooltip: item.url);
            EditorStyles.linkLabel.CalcMinMaxWidth(label, out _, out var maxW);

            rect.height = EditorGUIUtility.singleLineHeight;
            rect.width = Mathf.Min(rect.width, maxW);

            if (EditorGUI.LinkButton(rect, label))
            {
                Application.OpenURL(item.url);
            }
        }

        private static void TableColumn_Version(Rect rect, PackageInfo item)
        {
            EditorGUI.SelectableLabel(rect, text: item.version);
        }

        private static void ApplySelectedPackages(PackageInfo[] packages)
        {
            if (packages == null || packages.Length < 1)
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

            var length = packages.Length;
            var unityIdentifiers = s_unityIdentifiers = new List<string>(length);
            var openUpmSb = s_openUpmSb = new StringBuilder();
            var firstOpenUpm = true;

            for (var i = 0; i < length; i++)
            {
                ref readonly var package = ref packages[i];
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

            ApplyOpenUpm();

            s_addRequest = Client.AddAndRemove(packagesToAdd: s_unityIdentifiers.ToArray());
            EditorApplication.update += AddRequestProgress;
        }

        public static void ApplyOpenUpm()
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

        public static bool ExistsOnPath(string exeName)
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

        private class PackageInfo : IEquatable<PackageInfo>
        {
            public PackageRegistry registry;
            public string name;
            public string version;
            public string url;
            public bool isOptional;
            public bool isInstalled;
            public bool isChosen;

            public override int GetHashCode()
                => HashCode.Combine(name);

            public override bool Equals(object obj)
                => obj is PackageInfo other && Equals(other);

            public bool Equals(PackageInfo other)
                => string.Equals(name, other?.name, StringComparison.Ordinal);

            public bool ShouldBeInstalled()
                => isInstalled == false && (isOptional == false || isChosen);
        }
    }
}

#endif
