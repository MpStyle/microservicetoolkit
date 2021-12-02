using System.Collections.Generic;

namespace microservice.toolkit.core.extension
{
    public static class DictionaryExtension
    {
        public static bool IsNullOrEmpty<TKey, TValue>(this Dictionary<TKey, TValue> dict)
        {
            return dict == null || dict.Count == 0;
        }
    }
}