using DemoConsoleApplication.Api;
using DinClassLibrary;
using DinClassLibrary.Attributes;

namespace DemoConsoleApplication.Infrastructure;

[DinAutoLogging]
public class NumberInMemoryRepository() : INumberRepository
{
    private readonly HashSet<int> _numbers = DataSeeder.Seed();
    
    public void Create(int number)
    {
        DinLogger.LogInfo($"Adding a new number: {number}");
        if (_numbers.Add(number)) return;
        DinLogger.LogError($"Number {number} already exists");
        throw new ArgumentException("Number already exists");
    }
    
    public ICollection<int> ReadAll()
    {
        DinLogger.LogInfo("Reading all numbers");
        return _numbers.ToList();
    }
    
    public void Delete(int number)
    {
        DinLogger.LogInfo($"Deleting number: {number}");
        if (_numbers.Remove(number)) return;
        DinLogger.LogError($"Number {number} not found");
        throw new ArgumentException($"Student with ID {number} not found.");
    }
}