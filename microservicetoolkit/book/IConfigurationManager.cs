namespace mpstyle.microservice.toolkit.book
{
    public interface IConfigurationManager
    {
        bool GetBool(string key);
        string GetString(string key);
        int GetInt(string key);
    }
}