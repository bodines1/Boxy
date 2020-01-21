using System;

namespace Boxy.Resources
{
    public static class ExtensionMethods
    {
        public static string ToDescription(this Enum enumerated)
        {
            return enumerated.ToString();
        }
    }
}
