using ConsoleApp1.Api;
using ConsoleApp1.Infrastructure;
using DinClassLibrary;

var container = new DinContainer();

container.Register(typeof(ILogger), typeof(ConsoleLogger));
container.Register(typeof(INumberRepository), typeof(NumberInMemoryRepository));
container.Register(typeof(IApiNumberController), typeof(ApiNumberController));

Console.WriteLine("Container is ready. Resolving controller and starting HTTP listener...");

var numberController = (IApiNumberController) container.GetService(typeof(IApiNumberController));

container.PrintGraph(typeof(IApiNumberController));

var http = new DinHttpListener(numberController);
http.Start();