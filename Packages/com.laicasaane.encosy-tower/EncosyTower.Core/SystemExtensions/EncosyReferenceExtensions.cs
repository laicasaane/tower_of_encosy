namespace EncosyTower.SystemExtensions
{
    public static class EncosyReferenceExtensions
    {
        public static bool ReferenceEquals<T>(this T self, T other, out bool bothIsNotNull)
        {
            if (self == null && other == null)
            {
                bothIsNotNull = false;
                return true;
            }

            if (self == null || other == null)
            {
                bothIsNotNull = false;
                return false;
            }

            bothIsNotNull = true;
            return ReferenceEquals(self, other);
        }
    }
}
