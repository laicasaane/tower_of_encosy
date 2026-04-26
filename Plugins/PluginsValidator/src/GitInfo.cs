using System.Xml.Serialization;

public readonly record struct GitInfo(string GitUrl, string FolderName, string CommitSha);

public readonly struct ConsoleColorScope : IDisposable
{
    public ConsoleColorScope(ConsoleColor color)
    {
        Console.ForegroundColor = color;
    }

    public void Dispose()
    {
        Console.ResetColor();
    }
}

public static class ConsoleExtensions
{
    extension(Console)
    {
        public static ConsoleColorScope WithColor(ConsoleColor color)
            => new ConsoleColorScope(color);
    }
}
