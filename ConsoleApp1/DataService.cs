namespace ConsoleApp1;

public class DataService(ILogger logger)
{
    public void DoSomething()
    {
        logger.Log("DataService is working correctly!");
    }
}