#if !UNITY_LOGGING
namespace Unity.Logging
{
    using System;

    public class HideInStackTrace : Attribute
    {
        public readonly bool HideEverythingInside;

        public HideInStackTrace(bool hideEverythingInside = false)
        {
            HideEverythingInside = hideEverythingInside;
        }
    }
}
#endif