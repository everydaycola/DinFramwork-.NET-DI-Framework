using ConsoleApp1.Api;
using DinClassLibrary;


DinContainer.RegisterAssembly(typeof(Program).Assembly);

// DinContainer.Register(typeof(ILogger), typeof(ConsoleLogger));
// DinContainer.Register(typeof(INumberRepository), typeof(NumberInMemoryRepository));
// DinContainer.Register(typeof(IApiNumberController), typeof(ApiNumberController));

DinContainer.CheckForCycles();
    
Console.WriteLine("Container is ready. Resolving controller and starting HTTP listener...");


var numberController = (IApiNumberController) DinContainer.GetService(typeof(IApiNumberController));

DinContainer.PrintGraph(typeof(IApiNumberController));

var http = new DinHttpListener(numberController);
http.Start();