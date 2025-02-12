namespace EncosyTower.Modules.Editor.Data.Settings.Views
{
    internal static class Notes
    {
        public static void ThrowsIllegalCharactersWhenSearchWindow()
        {
            // NOTE 1:
            // When performing search in the Project Settings window
            // `evt.newValue` will contain something like `<color></color>`
            // to highlight the search results.
            // However `<` and `>` are illegal characters for directory path
            // this `Path.Combine` will throw exceptions.
            // That's why we should return if we detect a `<` in `evt.newValue`.

            // NOTE 2:
            // Exhausting detection using `Path.GetInvalidPathChars()`
            // might negatively affect the performance.
        }
    }
}
