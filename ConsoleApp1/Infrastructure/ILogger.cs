namespace ConsoleApp1.Infrastructure;

public interface ILogger
{
    void Log(string message);
    void Error(string message);
}