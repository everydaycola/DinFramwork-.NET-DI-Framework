# DinFramework

DinFramework is a lightweight, fast Dependency Injection (DI) container for .NET, featuring assembly scanning, constructor injection, cycle detection, and dynamic interception.

## Features

- **Singleton Registration:** Support for adding singletons to the container.
- **Assembly Scanning:** Automatically register services and controllers from an assembly.
- **Constructor Injection:** Automatically resolve dependencies through constructors, with preference for non-default constructors.
- **Cycle Detection:** Automatically detects cyclic dependencies using a dependency graph and throws informative exceptions.
- **API Controller Support:** Built-in HTTP listener that routes requests to classes marked with `[DinApiController]`.
- **Dynamic Interception (AOP):** Add cross-cutting concerns like logging using the `[DinAutoLogging]` attribute and Castle DynamicProxy.
- **High Performance:** Uses caching for constructor metadata to minimize reflection overhead after initial resolution.

## Usage Guide

### 1. Registering Services

You can register services manually as singletons:

```csharp
using DinClassLibrary;

// Map interface to implementation
DinContainer.RegisterUnique<INumberRepository, NumberInMemoryRepository>();

// Or use non-generic version
DinContainer.Register(typeof(ILogger), typeof(ConsoleLogger));
```

### 2. Assembly Scanning

Automatically register all interfaces and their single implementations in an assembly:

```csharp
DinContainer.RegisterAssembly(typeof(Program).Assembly);
```

### 3. Resolving Services

Retrieve instances from the container:

```csharp
var repository = DinContainer.Resolve<INumberRepository>();
```

### 4. API Controllers

Mark your classes with `[DinApiController]` and start the HTTP listener:

```csharp
[DinApiController("numbers")]
public class ApiNumberController
{
    public IEnumerable<int> Get() => new[] { 1, 2, 3 };
    
    public int Get(int id) => id;
}

// In Program.cs:
DinContainer.StartApiControllersFromAssembly(typeof(Program).Assembly);
```

The listener will be available at `http://localhost:9999/api/numbers`.

### 5. Dynamic Interception (Logging)

Add the `[DinAutoLogging]` attribute to a class or method to automatically log calls:

```csharp
[DinAutoLogging]
public class MyService : IMyService
{
    public void DoSomething() { ... }
}
```

## Getting Started

### Prerequisites

- .NET 8.0 SDK

### Compiling the Code

To compile the entire solution, run the following command in the root directory:

```bash
dotnet build
```

### Running the Demo Application

The demo application starts an HTTP server demonstrating the DI container and API routing.

```bash
dotnet run --project ConsoleApp1
```

Once running, you can access the API at `http://localhost:9999/api/numbers`.

### Running the Tests

To execute the unit tests and verify the framework's functionality:

```bash
dotnet test
```

Tests cover:
- Simple and recursive service resolution.
- Singleton behavior.
- Cycle detection (throws `InvalidOperationException`).
- Ambiguous constructor detection (throws `DinAmbiguousConstructorException`).
- Dynamic interception and proxy generation.
