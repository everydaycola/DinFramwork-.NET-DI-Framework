namespace DinClassLibrary;

public static class DinLogger
{
    public static LogLevel CurrentLevel { get; set; } = LogLevel.Info;

    public static void Log(string message, LogLevel level = LogLevel.Info)
    {
        if (level < CurrentLevel) return;
        var time = DateTime.Now.ToString("HH:mm:ss");
        Console.WriteLine($"[{time}][{level}]: {message}");
    }
    
    public static void LogDebug(string message)
    {
        Log(message, LogLevel.Debug);
    }

    public static void LogDetail(string message)
    {
        Log(message, LogLevel.Detail);
    }

    public static void LogInfo(string message)
    {
        Log(message);
    }

    public static void LogWarn(string message)
    {
        Log(message, LogLevel.Warn);
    }

    public static void LogError(string message)
    {
        Log(message, LogLevel.Error);
    }
    
    public enum LogLevel { Debug, Detail, Info, Warn, Error }
}