using System.Collections.Generic;
using System.Linq;

namespace microservice.toolkit.core.extension
{
    public static class ListExtension
    {
        public static bool IsNullOrEmpty<T>(this List<T> l)
        {
            return l == null || l.Any() == false;
        }
    }
}