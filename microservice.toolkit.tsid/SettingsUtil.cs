using microservice.toolkit.core.extension;

using System;

namespace microservice.toolkit.tsid;

internal static class SettingsUtil
{
    private static readonly string PROPERTY_PREFIX = "tsidcreator";
    private static readonly string PROPERTY_NODE = "node";

    internal static int? getNode()
    {
        var fullName = string.Join("_", PROPERTY_PREFIX, PROPERTY_NODE).ToUpper().Replace(".", "_");
        var value = Environment.GetEnvironmentVariable(fullName);

        if (value.IsNullOrEmpty())
        {
            return null;
        }

        try
        {
            return int.Parse(value);
        }
        catch (FormatException e)
        {
            return null;
        }
    }
}
