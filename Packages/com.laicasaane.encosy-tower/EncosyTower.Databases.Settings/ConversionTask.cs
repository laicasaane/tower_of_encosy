using System.Threading.Tasks;
using EncosyTower.Logging;
using UnityEditor;

namespace EncosyTower.Databases.Settings
{
    using DatabaseSettings = DatabaseCollectionSettings.DatabaseSettings;

    internal delegate void ReportAction(string title, string message);

    internal sealed class ConversionTask
    {
        private readonly int _id;
        private readonly Task<bool> _task;

        private ConversionTask(int id, Task<bool> task)
        {
            _id = id;
            _task = task;
        }

        public static void Run(
              DatabaseSettings settings
            , DataSourceFlags sources
            , UnityEngine.Object owner
            , string title
        )
        {
            if (settings == null)
            {
                return;
            }

            var id = Progress.Start(title);
            var task = new ConversionTask(id, settings.ConvertEditorAsync(
                  sources
                , owner
                , (title, msg) => Progress.Report(id, 0f, msg)
                , default
                , false
            ));

            EditorApplication.update += task.Update;
        }

        private void Update()
        {
            if (_task.IsCompleted == false && _task.IsCanceled == false)
            {
                return;
            }

            EditorApplication.update -= Update;

            if (_task.IsFaulted)
            {
                StaticDevLogger.LogException(_task.Exception);
            }

            Progress.Remove(_id);

            if (_task.Result)
            {
                AssetDatabase.Refresh();
            }
        }
    }
}
