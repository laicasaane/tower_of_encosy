using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace EncosyTower.DevTools
{
    internal static class DevToolsForSamples
    {
        internal const string IMPORTED_SAMPLES_PATH = "Assets/Samples/Encosy Tower";
        internal const string PACKAGE_SAMPLES_PATH = "Packages/com.laicasaane.encosy-tower/Samples~";
        internal const string UPDATE_ASSET_SEMVER_PATHS_MENU = "Dev Tools/Samples/Update Asset Semver Paths";
        internal const string COPY_IMPORTED_SAMPLES_TO_PACKAGE_MENU = "Dev Tools/Samples/Copy Imported Samples To Package";

        private static readonly string s_numericIdentifierPattern = @"0|[1-9]\d*";
        private static readonly string s_nonNumericIdentifierPattern = @"[0-9A-Za-z-]*[A-Za-z-][0-9A-Za-z-]*";
        private static readonly string s_prereleaseIdentifierPattern =
            $"(?:{s_numericIdentifierPattern}|{s_nonNumericIdentifierPattern})";
        private static readonly string s_prereleasePattern =
            $@"(?:-({s_prereleaseIdentifierPattern}(?:\.{s_prereleaseIdentifierPattern})*))?";
        private static readonly string s_buildPattern = @"(?:\+([0-9A-Za-z-]+(?:\.[0-9A-Za-z-]+)*))?";
        private static readonly string s_semverPattern =
            $@"({s_numericIdentifierPattern})\.({s_numericIdentifierPattern})\.({s_numericIdentifierPattern})" +
            $"{s_prereleasePattern}{s_buildPattern}";
        private static readonly Regex s_semverRegex = new($"^{s_semverPattern}$", RegexOptions.Compiled);
        private static readonly Regex s_sampleReferenceRegex = new(
              $@"Samples/Encosy Tower/{s_semverPattern}(?![0-9A-Za-z.+-])"
            , RegexOptions.Compiled
        );

        [MenuItem(UPDATE_ASSET_SEMVER_PATHS_MENU, priority = 68_84_00_00)]
        private static void UpdateAssetSemverPaths()
        {
            var changedCount = UpdateAssetSemverPaths(GetAbsolutePath(IMPORTED_SAMPLES_PATH));

            if (changedCount > 0)
            {
                AssetDatabase.Refresh();
            }
        }

        [MenuItem(COPY_IMPORTED_SAMPLES_TO_PACKAGE_MENU, priority = 68_84_00_01)]
        private static void CopyImportedSamplesToPackage()
        {
            var versions = DiscoverSampleVersions(GetAbsolutePath(IMPORTED_SAMPLES_PATH));
            var decision = DecideCopyVersion(versions);

            switch (decision.Kind)
            {
                case CopyVersionDecisionKind.NoVersion:
                    return;

                case CopyVersionDecisionKind.Selected:
                    CopyAndRefresh(decision.SelectedVersion);
                    return;

                case CopyVersionDecisionKind.RequiresChoice:
                    SampleVersionSelectionWindow.Open(decision.Versions, CopyAndRefresh);
                    return;

                default:
                    throw new InvalidOperationException($"Unsupported copy decision: {decision.Kind}");
            }
        }

        internal static IReadOnlyList<SampleVersionFolder> DiscoverSampleVersions(string importedSamplesPath)
        {
            if (Directory.Exists(importedSamplesPath) == false)
            {
                return Array.Empty<SampleVersionFolder>();
            }

            return Directory.GetDirectories(importedSamplesPath)
                .Select(static path => new SampleVersionFolder(Path.GetFileName(path), path))
                .Where(static x => IsSemanticVersion(x.Version))
                .OrderByDescending(x => x.Version, SemanticVersionComparer.Instance)
                .ToArray();
        }

        internal static CopyVersionDecision DecideCopyVersion(IReadOnlyList<SampleVersionFolder> versions)
        {
            if (versions.Count < 1)
            {
                return CopyVersionDecision.NoVersion();
            }

            var sortedVersions = versions
                .OrderByDescending(x => x.Version, SemanticVersionComparer.Instance)
                .ToArray();

            return sortedVersions.Length == 1
                ? CopyVersionDecision.Selected(sortedVersions[0])
                : CopyVersionDecision.RequiresChoice(sortedVersions);
        }

        internal static int UpdateAssetSemverPaths(string importedSamplesPath)
        {
            var changedCount = 0;
            var versions = DiscoverSampleVersions(importedSamplesPath);

            foreach (var version in versions)
            {
                foreach (var assetPath in Directory.EnumerateFiles(
                      version.AbsolutePath
                    , "*.asset"
                    , SearchOption.AllDirectories
                ))
                {
                    if (TryReplaceAssetVersionReferences(
                          File.ReadAllText(assetPath)
                        , version.Version
                        , out var result
                    ) == false)
                    {
                        continue;
                    }

                    File.WriteAllText(assetPath, result);
                    changedCount++;
                }
            }

            return changedCount;
        }

        internal static bool TryReplaceAssetVersionReferences(
              string content
            , string containingVersion
            , out string result
        )
        {
            result = s_sampleReferenceRegex.Replace(content, $"Samples/Encosy Tower/{containingVersion}");

            if (string.Equals(content, result, StringComparison.Ordinal))
            {
                result = content;
                return false;
            }

            return true;
        }

        internal static bool CopyImportedSamplesToPackage(SampleVersionFolder version, string packageSamplesPath)
        {
            var changed = false;

            if (Directory.Exists(packageSamplesPath) == false)
            {
                Directory.CreateDirectory(packageSamplesPath);
                changed = true;
            }

            foreach (var directory in Directory.GetDirectories(packageSamplesPath))
            {
                Directory.Delete(directory, true);
                changed = true;
            }

            foreach (var sourceDirectory in Directory.GetDirectories(version.AbsolutePath))
            {
                var destinationDirectory = Path.Combine(packageSamplesPath, Path.GetFileName(sourceDirectory));
                CopyDirectory(sourceDirectory, destinationDirectory);
                changed = true;
            }

            return changed;
        }

        internal static bool IsSemanticVersion(string version)
            => s_semverRegex.IsMatch(version);

        private static void CopyAndRefresh(SampleVersionFolder version)
        {
            if (CopyImportedSamplesToPackage(version, GetAbsolutePath(PACKAGE_SAMPLES_PATH)))
            {
                AssetDatabase.Refresh();
            }
        }

        private static string GetAbsolutePath(string relativePath)
            => Path.GetFullPath(relativePath);

        private static void CopyDirectory(string sourceDirectory, string destinationDirectory)
        {
            Directory.CreateDirectory(destinationDirectory);

            foreach (var file in Directory.GetFiles(sourceDirectory))
            {
                File.Copy(file, Path.Combine(destinationDirectory, Path.GetFileName(file)), true);
            }

            foreach (var directory in Directory.GetDirectories(sourceDirectory))
            {
                CopyDirectory(directory, Path.Combine(destinationDirectory, Path.GetFileName(directory)));
            }
        }

        internal sealed class SampleVersionFolder
        {
            public SampleVersionFolder(string version, string absolutePath)
            {
                Version = version;
                AbsolutePath = absolutePath;
            }

            public string Version { get; }

            public string AbsolutePath { get; }
        }

        internal enum CopyVersionDecisionKind
        {
            NoVersion = 0,
            Selected = 1,
            RequiresChoice = 2,
        }

        internal sealed class CopyVersionDecision
        {
            private CopyVersionDecision(
                  CopyVersionDecisionKind kind
                , SampleVersionFolder selectedVersion
                , IReadOnlyList<SampleVersionFolder> versions
            )
            {
                Kind = kind;
                SelectedVersion = selectedVersion;
                Versions = versions;
            }

            public CopyVersionDecisionKind Kind { get; }

            public SampleVersionFolder SelectedVersion { get; }

            public IReadOnlyList<SampleVersionFolder> Versions { get; }

            public static CopyVersionDecision NoVersion()
                => new(CopyVersionDecisionKind.NoVersion, null, Array.Empty<SampleVersionFolder>());

            public static CopyVersionDecision Selected(SampleVersionFolder version)
                => new(CopyVersionDecisionKind.Selected, version, new[] { version });

            public static CopyVersionDecision RequiresChoice(IReadOnlyList<SampleVersionFolder> versions)
                => new(CopyVersionDecisionKind.RequiresChoice, null, versions);
        }

        internal sealed class SemanticVersionComparer : IComparer<string>
        {
            public static readonly SemanticVersionComparer Instance = new();

            private SemanticVersionComparer() { }

            public int Compare(string x, string y)
            {
                var xVersion = SemanticVersion.Parse(x);
                var yVersion = SemanticVersion.Parse(y);
                return xVersion.CompareTo(yVersion);
            }
        }

        private readonly struct SemanticVersion : IComparable<SemanticVersion>
        {
            private readonly int _major;
            private readonly int _minor;
            private readonly int _patch;
            private readonly string[] _prereleaseIdentifiers;

            private SemanticVersion(int major, int minor, int patch, string[] prereleaseIdentifiers)
            {
                _major = major;
                _minor = minor;
                _patch = patch;
                _prereleaseIdentifiers = prereleaseIdentifiers;
            }

            public int CompareTo(SemanticVersion other)
            {
                var result = _major.CompareTo(other._major);

                if (result != 0)
                {
                    return result;
                }

                result = _minor.CompareTo(other._minor);

                if (result != 0)
                {
                    return result;
                }

                result = _patch.CompareTo(other._patch);

                if (result != 0)
                {
                    return result;
                }

                return ComparePrerelease(other);
            }

            public static SemanticVersion Parse(string version)
            {
                var match = s_semverRegex.Match(version);

                if (match.Success == false)
                {
                    throw new ArgumentException($"Invalid semantic version: {version}", nameof(version));
                }

                return new SemanticVersion(
                      int.Parse(match.Groups[1].Value)
                    , int.Parse(match.Groups[2].Value)
                    , int.Parse(match.Groups[3].Value)
                    , match.Groups[4].Success
                        ? match.Groups[4].Value.Split('.')
                        : Array.Empty<string>()
                );
            }

            private int ComparePrerelease(SemanticVersion other)
            {
                var hasPrerelease = _prereleaseIdentifiers.Length > 0;
                var otherHasPrerelease = other._prereleaseIdentifiers.Length > 0;

                if (hasPrerelease != otherHasPrerelease)
                {
                    return hasPrerelease ? -1 : 1;
                }

                for (
                    var i = 0;
                    i < _prereleaseIdentifiers.Length && i < other._prereleaseIdentifiers.Length;
                    i++
                )
                {
                    var result = ComparePrereleaseIdentifier(
                          _prereleaseIdentifiers[i]
                        , other._prereleaseIdentifiers[i]
                    );

                    if (result != 0)
                    {
                        return result;
                    }
                }

                return _prereleaseIdentifiers.Length.CompareTo(other._prereleaseIdentifiers.Length);
            }

            private static int ComparePrereleaseIdentifier(string x, string y)
            {
                var xIsNumeric = int.TryParse(x, out var xNumber);
                var yIsNumeric = int.TryParse(y, out var yNumber);

                if (xIsNumeric && yIsNumeric)
                {
                    return xNumber.CompareTo(yNumber);
                }

                if (xIsNumeric != yIsNumeric)
                {
                    return xIsNumeric ? -1 : 1;
                }

                return string.CompareOrdinal(x, y);
            }
        }

        private sealed class SampleVersionSelectionWindow : EditorWindow
        {
            private IReadOnlyList<SampleVersionFolder> _versions;
            private Action<SampleVersionFolder> _onCopy;
            private int _selectedIndex;

            public static void Open(IReadOnlyList<SampleVersionFolder> versions, Action<SampleVersionFolder> onCopy)
            {
                var window = CreateInstance<SampleVersionSelectionWindow>();
                window.titleContent = new GUIContent("Select Sample Version");
                window.minSize = new Vector2(320, 220);
                window._versions = versions;
                window._onCopy = onCopy;
                window.ShowUtility();
            }

            public void CreateGUI()
            {
                _selectedIndex = 0;

                var versionNames = _versions.Select(static x => x.Version).ToList();
                var listView = new ListView(
                      versionNames
                    , 22
                    , static () => new Label()
                    , (element, index) => ((Label)element).text = versionNames[index]
                )
                {
                    selectionType = SelectionType.Single,
                    selectedIndex = 0,
                    style =
                    {
                        flexGrow = 1,
                    },
                };

                listView.selectionChanged += selectedItems =>
                {
                    var selectedVersion = selectedItems.OfType<string>().FirstOrDefault();
                    var index = versionNames.IndexOf(selectedVersion);

                    if (index >= 0)
                    {
                        _selectedIndex = index;
                    }
                };

                var buttonRow = new VisualElement
                {
                    style =
                    {
                        flexDirection = FlexDirection.Row,
                        justifyContent = Justify.FlexEnd,
                    },
                };

                buttonRow.Add(new Button(Close) { text = "Cancel" });
                buttonRow.Add(new Button(CopySelected) { text = "Copy" });

                rootVisualElement.Add(listView);
                rootVisualElement.Add(buttonRow);
            }

            private void CopySelected()
            {
                _onCopy?.Invoke(_versions[_selectedIndex]);
                Close();
            }
        }
    }
}
