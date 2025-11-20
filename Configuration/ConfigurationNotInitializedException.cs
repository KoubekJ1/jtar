namespace Jtar.Configuration;

public class ConfigurationNotInitializedException : Exception
{
    public ConfigurationNotInitializedException()
        : base("Configuration instance has not been initialized. Please call Configuration.CreateInstance() before accessing the instance.")
    {
    }
}