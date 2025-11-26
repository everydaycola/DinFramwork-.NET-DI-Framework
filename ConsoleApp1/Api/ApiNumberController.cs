using ConsoleApp1.Infrastructure;

namespace ConsoleApp1.Api;

public class ApiNumberController(INumberRepository repository, ILogger logger) : IApiNumberController
{
    public ICollection<int> GetAll()
    {
        logger.Log("Getting all numbers");
        return repository.ReadAll();
    }
    
    public void Post(int id)
    {
        logger.Log($"Adding a new number: {id}");
        repository.Create(id);
    }
    
    public void Delete(int id)
    {
        logger.Log($"Deleting number: {id}");
        repository.Delete(id);
    }
}