namespace mpstyle.microservice.toolkit.book
{
    public interface IConfigurationManager
    {
        bool GetBool(string key);
        string GetString(string key);
        int GetInt(string key);
        string[] GetStringArray(string key);
        int[] GetIntArray(string key);
    }
}