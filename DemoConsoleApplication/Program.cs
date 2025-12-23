using DinClassLibrary;


DinContainer.RegisterAssembly(typeof(Program).Assembly);

// replaced with assembly scanning
// DinContainer.Register(typeof(ILogger), typeof(ConsoleLogger));
// DinContainer.Register(typeof(INumberRepository), typeof(NumberInMemoryRepository));
// DinContainer.Register(typeof(IApiNumberController), typeof(ApiNumberController));

DinContainer.CheckForCycles();
    
DinLogger.LogInfo("Container is ready. Resolving controllers and starting HTTP listener...");

DinContainer.StartApiControllersFromAssembly(typeof(Program).Assembly);

