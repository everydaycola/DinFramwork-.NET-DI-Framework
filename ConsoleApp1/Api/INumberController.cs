namespace ConsoleApp1.Api;

public interface INumberController
{
    public ICollection<int> GetAll();
    public void Post(int id);
    public void Delete(int id);
}