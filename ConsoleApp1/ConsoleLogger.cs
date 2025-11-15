namespace ConsoleApp1;

public class ConsoleLogger : ILogger
{
    public void Log(string message)
    {
        Console.WriteLine($"[Log]: {message}");
    }
}