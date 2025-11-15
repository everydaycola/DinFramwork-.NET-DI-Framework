namespace ConsoleApp1;

public class DataService
{
    private readonly ILogger _logger;
    
    public DataService(ILogger logger)
    {
        _logger = logger;
    }
    
    public void DoSomething()
    {
        _logger.Log("DataService is working correctly!");
    }
}