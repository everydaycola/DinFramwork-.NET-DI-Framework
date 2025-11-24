namespace ConsoleApp1.Infrastructure;

public class ConsoleLogger : ILogger
{
    public void Log(string message)
    {
        Console.WriteLine($"[Log]: {message}");
    }
    
    public void Error(string message)
    {
        Console.WriteLine($"[Error]: {message}");
    }
}