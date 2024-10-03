using System;
using System.Collections.Generic;

namespace microservice.toolkit.connection.extensions.objectmapper;

internal static class TypeMapper
{
    private static readonly Dictionary<string, TypeMap> sharedDatabase = new();

    public static TypeMap Map(Type target)
    {
        if (target == null)
        {
            throw new ArgumentNullException(nameof(target));
        }

        var fullNameType = target.FullName;

        if (string.IsNullOrEmpty(fullNameType))
        {
            throw new Exception($"Invalid full name");
        }

        if (sharedDatabase.ContainsKey(fullNameType))
        {
            return sharedDatabase[fullNameType];
        }

        sharedDatabase.Add(fullNameType, new TypeMap(target));

        return sharedDatabase[fullNameType];
    }
}