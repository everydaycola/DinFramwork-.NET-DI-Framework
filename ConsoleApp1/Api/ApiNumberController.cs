using ConsoleApp1.Infrastructure;
using DinClassLibrary;
using DinClassLibrary.Attributes;

namespace ConsoleApp1.Api;

[DinApiController("numbers")]
[DinAutoLogging]
public class ApiNumberController(INumberRepository repository) : IApiNumberController
{
    public ICollection<int> GetAll()
    {
        DinLogger.Log("Getting all numbers");
        return repository.ReadAll();
    }
    
    public void Post(int id)
    {
        DinLogger.Log($"Adding a new number: {id}");
        repository.Create(id);
    }
    
    public void Delete(int id)
    {
        DinLogger.Log($"Deleting number: {id}");
        repository.Delete(id);
    }
}