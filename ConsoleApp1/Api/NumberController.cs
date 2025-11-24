using ConsoleApp1.Infrastructure;

namespace ConsoleApp1.Api;

public class NumberController(INumberRepository repository, ILogger logger) : INumberController
{
       
    // todo add better status codes
    public ICollection<int> GetAll()
    {
        logger.Log("Getting all numbers");
        return repository.ReadAll();
    }
    
    public void Post(int number)
    {
        logger.Log($"Adding a new number: {number}");
        repository.Create(number);
    }
    
    public void Delete(int number)
    {
        logger.Log($"Deleting number: {number}");
        repository.Delete(number);
    }
}