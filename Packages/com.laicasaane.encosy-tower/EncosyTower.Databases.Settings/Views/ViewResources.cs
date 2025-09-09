using EncosyTower.Logging;
using UnityEngine.UIElements;

namespace EncosyTower.Databases.Settings.Views
{
    internal readonly record struct ViewResources(VisualElement Root)
    {
        public static implicit operator ViewResources(VisualElement root)
            => new(root);

        public ViewResources_GoogleSheet GoogleSheet
            => new(this);

        public ViewResources_LocalFolder LocalFolder
            => new(this);

        public string DatabaseInvalid => GetLabelText("database-invalid");

        public string RelativePath => GetLabelText("relative-path");

        private string GetLabelText(string name)
        {
            if (Root.Q<Label>(name: name)?.text is string value)
            {
                return value;
            }

            DevLoggerAPI.LogError($"Cannot find Label by name `{name}`");
            return string.Empty;
        }

        public readonly record struct ViewResources_GoogleSheet(ViewResources Resources)
        {
            private const string BASE = "google-sheet";

            public string Credential
                => Resources.GetLabelText($"{BASE}__credential");

            public string CredentialMissing
                => Resources.GetLabelText($"{BASE}__credential-missing");

            public string ApiKeyMissing
                => Resources.GetLabelText($"{BASE}__api-key-missing");

            public string SpreadSheetId
                => Resources.GetLabelText($"{BASE}__spreadsheet");

            public string SpreadSheetIdInvalid
                => Resources.GetLabelText($"{BASE}__spreadsheet-invalid");

            public string TokenFolderInvalid
                => Resources.GetLabelText("token-folder-invalid");

            public string TokenFolderMissing
                => Resources.GetLabelText("token-folder-missing");

            public string OutputFolderInvalid
                => Resources.GetLabelText("output-folder-invalid");

            public string OutputFolderMissing
                => Resources.GetLabelText("output-folder-missing");
        }

        public readonly record struct ViewResources_LocalFolder(ViewResources Resources)
        {
            public string InputFolderInvalid
                => Resources.GetLabelText("input-folder-invalid");

            public string InputFolderMissing
                => Resources.GetLabelText("input-folder-missing");

            public string OutputFolderInvalid
                => Resources.GetLabelText("output-folder-invalid");

            public string OutputFolderMissing
                => Resources.GetLabelText("output-folder-missing");
        }
    }
}
