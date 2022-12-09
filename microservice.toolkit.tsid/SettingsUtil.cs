using microservice.toolkit.core.extension;

using System;

namespace microservice.toolkit.tsid;

internal class SettingsUtil
{
    static readonly string PROPERTY_PREFIX = "tsidcreator";
    static readonly string PROPERTY_NODE = "node";

    protected SettingsUtil()
    {
    }

    public static int? getNode()
    {
        var value = GetProperty(PROPERTY_NODE);

        if (value == null)
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

    // public static void SetNode(int node)
    // {
    //     SetProperty(PROPERTY_NODE, node.ToString());
    // }

    static string GetProperty(string name)
    {

        var fullName = GetPropertyName(name);
        // var value = System.getProperty(fullName);
        // if (!IsEmpty(value))
        // {
        //     return value;
        // }

        fullName = GetEnvinronmentName(name);
        var value = Environment.GetEnvironmentVariable(fullName);
        if (!IsEmpty(value))
        {
            return value;
        }

        return null;
    }

    // static void SetProperty(string key, string value)
    // {
    //     System.SetProperty(GetPropertyName(key), value);
    // }

    // static void ClearProperty(string key)
    // {
    //     System.clearProperty(GetPropertyName(key));
    // }

    static string GetPropertyName(string key)
    {
        return string.Join(".", PROPERTY_PREFIX, key);
    }

    static string GetEnvinronmentName(string key)
    {
        return string.Join("_", PROPERTY_PREFIX, key).ToUpper().Replace(".", "_");
    }

    private static bool IsEmpty(string value)
    {
        return value == null || value.IsNullOrEmpty();
    }
}
