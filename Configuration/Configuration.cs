namespace Jtar.Configuration;

public class Configuration
{
    private static Configuration? _instance;

    public int ThreadCount { get; private set; }

    private Configuration(int threadCount = 0)
    {
        // Use all but one thread to prevent system performance issues
        if (threadCount < 0) threadCount = Environment.ProcessorCount - 1;
        ThreadCount = threadCount;
    }

    public static void CreateInstance(int threadCount = 0)
    {
        if (_instance == null) _instance = new Configuration(threadCount);
    }

    public static Configuration GetInstance()
    {
        if (_instance == null) throw new ConfigurationNotInitializedException();

        return _instance;
    }
}