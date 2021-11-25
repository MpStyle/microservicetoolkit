using System.Linq;

namespace microservice.toolkit.core.extension
{
    public static class ArrayExtension
    {
        public static bool IsNullOrEmpty<T>(this T[] l)
        {
            return l == null || l.Any() == false;
        }
    }
}