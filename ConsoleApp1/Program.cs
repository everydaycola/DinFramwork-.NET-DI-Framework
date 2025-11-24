using ConsoleApp1.Api;
using ConsoleApp1.Infrastructure;
using DinClassLibrary;

var container = new DinContainer();

container.Register(typeof(ILogger), typeof(ConsoleLogger));
container.Register(typeof(INumberRepository), typeof(NumberInMemoryRepository));
container.Register(typeof(INumberController), typeof(NumberController));

Console.WriteLine("Container is ready. Resolving 'mainApp'...");

var numberController = (INumberController) container.GetService(typeof(INumberController));

container.PrintGraph(typeof(INumberController));


Console.WriteLine("Getting all numbers: " + string.Join(", ", numberController.GetAll()));
Console.WriteLine("Deleting 4: ");
numberController.Delete(4);
Console.WriteLine("Adding 5: ");
numberController.Post(5);
Console.WriteLine("Getting all numbers: " + string.Join(", ", numberController.GetAll()));