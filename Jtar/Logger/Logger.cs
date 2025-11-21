using System;
using System.Threading;

namespace Jtar.Logging;

public static class Logger
{
    public static void Log(LogType type, string message)
    {

        Console.WriteLine(
            $"[{DateTime.Now:HH:mm:ss.fff}] [{type.ToString().ToUpper()}] {message}"
        );

    }

    public static void Log(Exception ex)
    {
        Log(LogType.Error, ex.ToString());
    }
}