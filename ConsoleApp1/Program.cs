using ConsoleApp1;
using DIN_ClassLibrary;

var container = new DiContainer();

container.Register(typeof(ILogger), typeof(ConsoleLogger));

container.Register(typeof(DataService), typeof(DataService));

Console.WriteLine("Container is ready. Resolving 'mainApp'...");

var myService = (DataService) container.GetService(typeof(DataService));


container.PrintGraph();

myService.DoSomething();

Console.ReadLine();