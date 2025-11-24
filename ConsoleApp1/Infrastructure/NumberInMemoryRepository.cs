using ConsoleApp1.Api;

namespace ConsoleApp1.Infrastructure;

public class NumberInMemoryRepository(ILogger logger) : INumberRepository
{
    private readonly HashSet<int> _numbers = DataSeeder.Seed();
    
    public void Create(int number)
    {
        logger.Log($"Adding a new number: {number}");
        if (_numbers.Add(number)) return;
        logger.Error($"Number {number} already exists");
        throw new ArgumentException("Number already exists");
    }
    
    public ICollection<int> ReadAll()
    {
        logger.Log($"Reading all numbers");
        return _numbers.ToList();
    }
    
    public void Delete(int number)
    {
        logger.Log($"Deleting number: {number}");
        if (_numbers.Remove(number)) return;
        logger.Error($"Number {number} not found");
        throw new ArgumentException($"Student with ID {number} not found.");
    }
}