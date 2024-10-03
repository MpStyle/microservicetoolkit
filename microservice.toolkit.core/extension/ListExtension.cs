using System.Collections.Generic;

namespace microservice.toolkit.core.extension
{
    public static class ListExtension
    {
        public static bool IsNullOrEmpty<T>(this List<T> l)
        {
            return l == null || l.Count != 0 == false;
        }
    }
}