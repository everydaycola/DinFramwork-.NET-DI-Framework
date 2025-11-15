// A. Create Container

using ConsoleApp1;
using DIN_ClassLibrary;

var container = new DiContainer();

container.Register("logger", typeof(ConsoleLogger));

container.Register("mainApp", typeof(DataService));

Console.WriteLine("Container is ready. Resolving 'mainApp'...");


var myService = (DataService) container.GetService("mainApp");

myService.DoSomething();

Console.ReadLine();