using System;

namespace EncosyTower.Entities.Stats
{
    [Serializable]
    public readonly struct None : IEquatable<None>
    {
        public readonly override bool Equals(object obj)
            => obj is None other && Equals(other);

        public readonly bool Equals(None other)
            => true;

        public readonly override int GetHashCode()
            => 0;

        public readonly override string ToString()
            => "none";
    }
}
