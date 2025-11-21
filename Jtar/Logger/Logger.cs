using System;
using System.Threading;

namespace Jtar.Logging;

public static class Logger
{
    private static readonly object _lock = new object();
    private static bool _showDebugMessages = false;

    public static bool ShowDebugMessages
    {
        get
        {
            lock (_lock)
            {
                return _showDebugMessages;
            }
        }
        set
        {
            lock (_lock)
            {
                _showDebugMessages = value;
            }
        }
    }

    public static void Log(LogType type, string message)
    {
        if (type == LogType.Debug && !ShowDebugMessages)
        {
            return;
        }
        var prefix = ShowDebugMessages ? $"[{DateTime.Now:HH:mm:ss.fff}] [{type.ToString().ToUpper()}] " : "";
        Console.WriteLine($"{prefix}{message}");
    }

    public static void Log(Exception ex)
    {
        Log(LogType.Error, ex.ToString());
    }
}