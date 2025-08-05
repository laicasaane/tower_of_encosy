using System;

namespace EncosyTower.Common
{
    public sealed class UndefinedException : Exception
    {
        public static readonly UndefinedException Default = new();

        private UndefinedException()
            : base("This kind of exception should accompany the Error type.")
        {
        }
    }
}

