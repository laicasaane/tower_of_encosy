using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace EncosyTower.DevTools;

internal static class DevToolsForVersion
{
    internal const string IMPORTED_SAMPLES_PATH = "Assets/Samples/Encosy Tower";
    internal const string PACKAGE_SAMPLES_PATH = "Packages/com.laicasaane.encosy-tower/Samples~";
    internal const string PACKAGE_MANIFEST_PATH = "Packages/com.laicasaane.encosy-tower/package.json";
    internal const string DATABASE_SETTINGS_PRESET_PATH = "EncosyTower.Samples.Data/DatabaseSettingsPreset.asset";
    internal const string SOURCE_GENERATOR_ROOT_PATH = "Plugins/SourceGenerator";
    internal const string SOURCE_GENERATOR_SOLUTION_PATH = "Plugins/SourceGenerator/EncosyTower.SourceGen.slnx";
    internal const string SOURCE_GENERATOR_VERSION_PATH =
        "Plugins/SourceGenerator/EncosyTower.SourceGen.Helpers/Helpers/SourceGenVersion.cs";

    private const string NUMERIC_IDENTIFIER_PATTERN = @"0|[1-9]\d*";

    private const string NON_NUMERIC_IDENTIFIER_PATTERN = @"[0-9A-Za-z-]*[A-Za-z-][0-9A-Za-z-]*";

    private const string PRERELEASE_IDENTIFIER_PATTERN =
        $"(?:{NUMERIC_IDENTIFIER_PATTERN}|{NON_NUMERIC_IDENTIFIER_PATTERN})";

    private const string PRERELEASE_PATTERN =
        $@"(?:-({PRERELEASE_IDENTIFIER_PATTERN}(?:\.{PRERELEASE_IDENTIFIER_PATTERN})*))?";

    private const string BUILD_PATTERN = @"(?:\+([0-9A-Za-z-]+(?:\.[0-9A-Za-z-]+)*))?";

    private const string SEMVER_PATTERN =
        $@"({NUMERIC_IDENTIFIER_PATTERN})\.({NUMERIC_IDENTIFIER_PATTERN})\.({NUMERIC_IDENTIFIER_PATTERN})"
        + $"{PRERELEASE_PATTERN}{BUILD_PATTERN}";

    private const string SAMPLE_REFERENCE_PATTERN =
        $@"Samples/Encosy Tower/{SEMVER_PATTERN}(?![0-9A-Za-z.+-])";

    private const string PACKAGE_VERSION_PATTERN =
        @"^(?<indent>\s*)""version""\s*:\s*""(?<version>[^""]+)""(?<suffix>\s*,?)$";

    private const string SOURCE_GENERATOR_VERSION_PATTERN =
        @"^(?<indent>\s*)public const string VALUE\s*=\s*""(?<version>[^""]+)""(?<suffix>\s*;?\s*)$";

    private static readonly Regex s_semverRegex = new($"^{SEMVER_PATTERN}$", RegexOptions.Compiled);
    private static readonly Regex s_sampleReferenceRegex = new(SAMPLE_REFERENCE_PATTERN, RegexOptions.Compiled);
    private static readonly Regex s_packageVersionRegex = new(PACKAGE_VERSION_PATTERN, RegexOptions.Compiled | RegexOptions.Multiline);
    private static readonly Regex s_sourceGeneratorVersionRegex = new(SOURCE_GENERATOR_VERSION_PATTERN, RegexOptions.Compiled | RegexOptions.Multiline);

    [MenuItem("Dev Tools/Update Version", priority = 68_84_00_00)]
    private static void OpenPackageVersionUpdater()
    {
        VersionUpdaterWindow.Open();
    }

    internal static Result TryUpdatePackageVersion(string targetVersion)
    {
        if (IsSemanticVersion(targetVersion) == false)
        {
            return Result.InvalidTargetVersion;
        }

        if (TryReadPackageVersion(GetAbsolutePath(PACKAGE_MANIFEST_PATH), out _) == false)
        {
            return Result.MissingPackageManifestVersion;
        }

        var importedSamplesPath = GetAbsolutePath(IMPORTED_SAMPLES_PATH);

        if (Directory.Exists(importedSamplesPath) == false)
        {
            return Result.MissingSamplesRoot;
        }

        var versions = DiscoverSampleVersions(importedSamplesPath);

        if (versions.Count == 0)
        {
            return Result.NoSampleVersionFolder;
        }

        if (versions.Count > 1)
        {
            ShowMultipleImportedSamplesDialog();
            PingImportedSamplesRoot();
            return Result.MultipleSampleVersionFolders;
        }

        var versionFolder = versions[0];
        var targetRelativePath = GetImportedSampleVersionFolderPath(targetVersion);

        if (string.Equals(versionFolder.RelativePath, targetRelativePath, StringComparison.Ordinal) == false
            && Directory.Exists(GetAbsolutePath(targetRelativePath))
        )
        {
            return Result.RenameSampleFolderFailed;
        }

        var presetPath = GetDatabaseSettingsPresetAbsolutePath(versionFolder.AbsolutePath);

        if (TryUpdateDatabaseSettingsPreset(presetPath, targetVersion) == false)
        {
            return Result.MissingDatabaseSettingsPreset;
        }

        if (string.Equals(versionFolder.Version, targetVersion, StringComparison.Ordinal) == false
            && TryRenameImportedSampleFolder(versionFolder, targetVersion, out versionFolder) == false
        )
        {
            return Result.RenameSampleFolderFailed;
        }

        CopyImportedSamplesToPackage(versionFolder, GetAbsolutePath(PACKAGE_SAMPLES_PATH));

        if (TryWritePackageVersion(GetAbsolutePath(PACKAGE_MANIFEST_PATH), targetVersion) == false)
        {
            return Result.MissingPackageManifestVersion;
        }

        if (TryWriteSourceGeneratorVersion(GetAbsolutePath(SOURCE_GENERATOR_VERSION_PATH), targetVersion) == false)
        {
            return Result.MissingSourceGeneratorVersion;
        }

        if (TryBuildSourceGeneratorRelease() == false)
        {
            return Result.SourceGeneratorBuildFailed;
        }

        AssetDatabase.Refresh();
        return Result.Succeeded;
    }

    internal static IReadOnlyList<SampleVersionFolder> DiscoverSampleVersions(string importedSamplesPath)
    {
        if (Directory.Exists(importedSamplesPath) == false)
        {
            return Array.Empty<SampleVersionFolder>();
        }

        return Directory.GetDirectories(importedSamplesPath)
            .Select(static path => new SampleVersionFolder(
                  Path.GetFileName(path)
                , GetProjectRelativePath(path)
                , path
            ))
            .Where(static x => IsSemanticVersion(x.Version))
            .OrderByDescending(x => x.Version, SemanticVersionComparer.Instance)
            .ToArray();
    }

    internal static bool IsSemanticVersion(string version)
    {
        return s_semverRegex.IsMatch(version);
    }

    internal static bool TryReadPackageVersion(string packageManifestPath, out string version)
    {
        version = null;

        if (File.Exists(packageManifestPath) == false)
        {
            return false;
        }

        var content = File.ReadAllText(packageManifestPath);
        var match = s_packageVersionRegex.Match(content);

        if (match.Success == false || IsSemanticVersion(match.Groups["version"].Value) == false)
        {
            return false;
        }

        version = match.Groups["version"].Value;
        return true;
    }

    internal static bool TryWritePackageVersion(string packageManifestPath, string version)
    {
        if (File.Exists(packageManifestPath) == false)
        {
            return false;
        }

        var content = File.ReadAllText(packageManifestPath);
        var match = s_packageVersionRegex.Match(content);

        if (match.Success == false)
        {
            return false;
        }

        var replacement = $"{match.Groups["indent"].Value}\"version\": \"{version}\"{match.Groups["suffix"].Value}";
        var updatedContent = content[..match.Index] + replacement + content[(match.Index + match.Length)..];

        if (string.Equals(content, updatedContent, StringComparison.Ordinal) == false)
        {
            File.WriteAllText(packageManifestPath, updatedContent);
        }

        return true;
    }

    internal static bool TryUpdateDatabaseSettingsPreset(string assetPath, string targetVersion)
    {
        if (File.Exists(assetPath) == false)
        {
            return false;
        }

        var content = File.ReadAllText(assetPath);

        if (TryReplaceAssetVersionReferences(content, targetVersion, out var updatedContent) == false)
        {
            return true;
        }

        File.WriteAllText(assetPath, updatedContent);
        return true;
    }

    internal static bool TryWriteSourceGeneratorVersion(string sourceGeneratorVersionPath, string version)
    {
        if (File.Exists(sourceGeneratorVersionPath) == false)
        {
            return false;
        }

        var content = File.ReadAllText(sourceGeneratorVersionPath);
        var match = s_sourceGeneratorVersionRegex.Match(content);

        if (match.Success == false)
        {
            return false;
        }

        var indent = match.Groups["indent"].Value;
        var suffix = match.Groups["suffix"].Value;
        var replacement = $"{indent}public const string VALUE = \"{version}\"{suffix}";
        var updatedContent = content[..match.Index] + replacement + content[(match.Index + match.Length)..];

        if (string.Equals(content, updatedContent, StringComparison.Ordinal) == false)
        {
            File.WriteAllText(sourceGeneratorVersionPath, updatedContent);
        }


        return true;
    }

    internal static bool TryBuildSourceGeneratorRelease()
    {
        try
        {
            using var process = Process.Start(new ProcessStartInfo {
                FileName = "dotnet",
                Arguments = $"build \"{Path.GetFileName(SOURCE_GENERATOR_SOLUTION_PATH)}\" -c Release",
                WorkingDirectory = GetAbsolutePath(SOURCE_GENERATOR_ROOT_PATH),
                UseShellExecute = false,
                CreateNoWindow = true,
            });

            if (process == null)
            {
                UnityEngine.Debug.LogError("Could not start the source-generator Release build.");
                return false;
            }

            process.WaitForExit();

            if (process.ExitCode != 0)
            {
                UnityEngine.Debug.LogError($"Source-generator Release build failed with exit code {process.ExitCode}.");
                return false;
            }

            return true;
        }
        catch (Exception exception)
        {
            UnityEngine.Debug.LogException(exception);
            return false;
        }
    }

    internal static bool TryRenameImportedSampleFolder(
          SampleVersionFolder versionFolder
        , string targetVersion
        , out SampleVersionFolder renamedFolder
    )
    {
        renamedFolder = versionFolder;

        var targetRelativePath = GetImportedSampleVersionFolderPath(targetVersion);

        if (string.Equals(versionFolder.RelativePath, targetRelativePath, StringComparison.Ordinal))
        {
            return true;
        }

        var targetAbsolutePath = GetAbsolutePath(targetRelativePath);

        if (Directory.Exists(targetAbsolutePath))
        {
            return false;
        }

        var moveError = AssetDatabase.MoveAsset(versionFolder.RelativePath, targetRelativePath);

        if (string.IsNullOrEmpty(moveError) == false)
        {
            return false;
        }

        renamedFolder = new SampleVersionFolder(targetVersion, targetRelativePath, targetAbsolutePath);
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

    internal static bool TryReplaceAssetVersionReferences(
          string content
        , string replacementVersion
        , out string result
    )
    {
        result = s_sampleReferenceRegex.Replace(content, $"Samples/Encosy Tower/{replacementVersion}");

        if (string.Equals(content, result, StringComparison.Ordinal))
        {
            result = content;
            return false;
        }

        return true;
    }

    private static void ShowMultipleImportedSamplesDialog()
    {
        const string MSG = "Multiple sample version folders were found under 'Assets/Samples/Encosy Tower/'. "
            + "Delete redundant folders before updating.";

        EditorUtility.DisplayDialog("Package Version Updater", MSG, "I understand");
    }

    private static void PingImportedSamplesRoot()
    {
        var root = AssetDatabase.LoadMainAssetAtPath(IMPORTED_SAMPLES_PATH);

        if (root != null)
        {
            EditorGUIUtility.PingObject(root);
        }
    }

    private static string GetDatabaseSettingsPresetAbsolutePath(string versionFolderAbsolutePath)
    {
        return GetAbsolutePath(Path.Combine(versionFolderAbsolutePath, DATABASE_SETTINGS_PRESET_PATH));
    }

    private static string GetImportedSampleVersionFolderPath(string version)
    {
        return NormalizeProjectPath(Path.Combine(IMPORTED_SAMPLES_PATH, version));
    }

    private static string GetAbsolutePath(string relativePath)
    {
        return Path.GetFullPath(relativePath);
    }

    private static string GetProjectRelativePath(string absolutePath)
    {
        return NormalizeProjectPath(Path.GetRelativePath(GetProjectRootPath(), absolutePath));
    }

    private static string GetProjectRootPath()
    {
        return Path.GetFullPath(".");
    }

    private static string NormalizeProjectPath(string path)
    {
        return path.Replace('\\', '/');
    }

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

    internal enum Result
    {
        Succeeded = 0,
        InvalidTargetVersion = 1,
        MissingPackageManifestVersion = 2,
        MissingSamplesRoot = 3,
        NoSampleVersionFolder = 4,
        MultipleSampleVersionFolders = 5,
        MissingDatabaseSettingsPreset = 6,
        RenameSampleFolderFailed = 7,
        MissingSourceGeneratorVersion = 8,
        SourceGeneratorBuildFailed = 9,
    }

    internal readonly struct SampleVersionFolder
    {
        public readonly string Version;
        public readonly string RelativePath;
        public readonly string AbsolutePath;

        public SampleVersionFolder(string version, string relativePath, string absolutePath)
        {
            Version = version;
            RelativePath = relativePath;
            AbsolutePath = absolutePath;
        }
    }

    internal sealed class SemanticVersionComparer : IComparer<string>
    {
        public static readonly SemanticVersionComparer Instance = new();

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
                int.Parse(match.Groups[1].Value),
                int.Parse(match.Groups[2].Value),
                int.Parse(match.Groups[3].Value),
                match.Groups[4].Success
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

            for (var i = 0; i < _prereleaseIdentifiers.Length && i < other._prereleaseIdentifiers.Length; i++)
            {
                var result = ComparePrereleaseIdentifier(
                    _prereleaseIdentifiers[i],
                    other._prereleaseIdentifiers[i]
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

    private sealed class VersionUpdaterWindow : EditorWindow
    {
        private TextField _versionField;

        public static void Open()
        {
            var window = CreateInstance<VersionUpdaterWindow>();
            window.titleContent = new GUIContent("Version Updater");
            window.minSize = new Vector2(360, 120);
            window.ShowUtility();
        }

        public void CreateGUI()
        {
            rootVisualElement.style.paddingLeft = 8;
            rootVisualElement.style.paddingRight = 8;
            rootVisualElement.style.paddingTop = 8;
            rootVisualElement.style.paddingBottom = 8;
            rootVisualElement.style.flexDirection = FlexDirection.Column;

            var versionField = new TextField("Version") { value = GetInitialVersion() };
            var updateButton = new Button(OnUpdateClicked) { text = "Update" };

            _versionField = versionField;

            rootVisualElement.Add(versionField);
            rootVisualElement.Add(updateButton);
        }

        private string GetInitialVersion()
        {
            if (TryReadPackageVersion(GetAbsolutePath(PACKAGE_MANIFEST_PATH), out var version))
            {
                return version;
            }

            const string MSG = "Package manifest version could not be read from or validated in "
                + "'Packages/com.laicasaane.encosy-tower/package.json'. Enter the target version manually.";

            EditorUtility.DisplayDialog("Version Updater", MSG, "OK");

            return string.Empty;
        }

        private void OnUpdateClicked()
        {
            Result result;

            try
            {
                result = TryUpdatePackageVersion(_versionField.value);
            }
            catch (Exception exception)
            {
                EditorUtility.DisplayDialog("Version Updater", exception.Message, "OK");
                return;
            }

            switch (result)
            {
                case Result.Succeeded:
                    Close();
                    return;

                default:
                    EditorUtility.DisplayDialog("Version Updater", GetMessage(result), "OK");
                    return;
            }
        }

        private static string GetMessage(Result result)
        {
            return result switch {
                Result.InvalidTargetVersion =>
                    "Enter a valid semantic version before updating.",

                Result.MissingPackageManifestVersion =>
                    "Package manifest version could not be read or written.",

                Result.MissingSamplesRoot =>
                    "Samples root was not found.",

                Result.NoSampleVersionFolder =>
                    "No sample version folder was found under 'Assets/Samples/Encosy Tower'.",

                Result.MissingDatabaseSettingsPreset =>
                    "DatabaseSettingsPreset.asset was not found under the active sample version folder.",

                Result.RenameSampleFolderFailed =>
                    "Sample version folder could not be renamed to the target version.",

                Result.MultipleSampleVersionFolders =>
                    "Multiple sample version folders were found under 'Assets/Samples/Encosy Tower/'.",

                Result.MissingSourceGeneratorVersion =>
                    "Source-generator version file could not be read or written.",

                Result.SourceGeneratorBuildFailed =>
                    "Source-generator Release build failed before the editor could refresh.",

                _ => "Package version update failed.",
            };
        }
    }
}
