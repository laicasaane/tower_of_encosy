using System;

namespace EncosyTower.Editor
{
    public sealed class EditorMenuItem
    {
        public string Name { get; set; }

        public string Shortcut { get; set; }

        public bool Checked { get; set; }

        public bool Separator { get; set; }

        public int Priority { get; set; }

        public Action Execute { get; set; }

        public Func<bool> Validate { get; set; }
    }
}
