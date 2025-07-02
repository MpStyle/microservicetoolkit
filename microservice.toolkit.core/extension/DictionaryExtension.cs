using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace microservice.toolkit.core.extension
{
    public static class DictionaryExtension
    {
        public static bool IsNullOrEmpty<TKey, TValue>([NotNullWhen(false)] this Dictionary<TKey, TValue>? dict) where TKey : notnull
        {
            return dict == null || dict.Count == 0;
        }
    }
}