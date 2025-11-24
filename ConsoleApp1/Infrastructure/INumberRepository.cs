namespace ConsoleApp1.Infrastructure;

public interface INumberRepository
{
    public void Create(int number);
    public ICollection<int> ReadAll();
    public void Delete(int id);
}