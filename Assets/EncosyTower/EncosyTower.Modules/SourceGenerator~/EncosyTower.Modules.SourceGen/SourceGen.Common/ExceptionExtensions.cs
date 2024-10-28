using System;

// Everything in this file was copied from Unity's source generators.
namespace EncosyTower.Modules.SourceGen
{
    public static class ExceptionExtensions
    {
        public static string ToUnityPrintableString(this Exception exception)
            => exception.ToString().Replace(Environment.NewLine, " |--| ");
    }
}

