using EncosyTower.Logging;
using UnityEditor;

namespace EncosyTower.Editor.Data.Settings
{
    partial class DatabaseCollectionSettings
    {
        partial class GoogleSheetSettings
        {
            private readonly static string s_progressTitle = "Convert Google SpreadSheet";

            private void Progress(ProgressMessageType messageType)
            {
                EditorUtility.DisplayProgressBar(s_progressTitle, GetMessage(messageType), 0);
            }

            private void StopProgress()
            {
                EditorUtility.ClearProgressBar();
            }

            private void Log(LogMessageType type)
            {
                DevLoggerAPI.LogError(GetMessage(type));
            }

            private string GetMessage(ProgressMessageType type)
            {
                return type switch {
                    ProgressMessageType.ReadFileTable => "Try retrieving 'file_list' table...",
                    ProgressMessageType.Download => "Downloading all SpreadSheets...",
                    ProgressMessageType.Validate => "Validating all SpreadSheets...",
                    _ => string.Empty,
                };
            }

            private string GetMessage(LogMessageType type)
            {
                return type switch {
                    LogMessageType.ServiceAccountJsonUndefined => "Service account JSON is undefined.",
                    LogMessageType.FoundNoAppropriateSpreadSheet => "Found no appropriate SpreadSheet.",
                    _ => string.Empty,
                };
            }

            private enum ProgressMessageType
            {
                None,
                ReadFileTable,
                Download,
                Validate,
            }

            private enum LogMessageType
            {
                None,
                ServiceAccountJsonUndefined,
                FoundNoAppropriateSpreadSheet,
            }
        }
    }
}
