using DinClassLibrary;
using DinClassLibrary.Attributes;
using Xunit.Sdk;

namespace UnitTests;

public class InterceptionTests : IDisposable
{
    private readonly StringWriter _consoleOutput = new();
    private readonly TextWriter _originalOutput;

    public InterceptionTests()
    {
        DinContainer.Clear();
        DinLogger.CurrentLevel = DinLogger.LogLevel.Debug;
        _originalOutput = Console.Out;
        Console.SetOut(_consoleOutput);
    }

    public void Dispose()
    {
        Console.SetOut(_originalOutput);
        DinContainer.Clear();
        _consoleOutput.Dispose();
    }

    [Fact]
    public void ServiceWithDinLogged_ShouldBeProxiedAndLog()
    {
        // Arrange
        DinContainer.RegisterUnique<ILogService, InterceptedService>();
        
        // Act
        var service = DinContainer.Resolve<ILogService>();
        var result = service.DoWork();

        // Assert
        Assert.Equal("Work Done", result);
        Assert.True(service.GetType().Name.Contains("Proxy"), $"Service type {service.GetType().Name} should be a proxy");
        
        var output = _consoleOutput.ToString();
        Assert.Contains("[INTERCEPTOR] Before calling DoWork", output);
        Assert.Contains("[INTERCEPTOR] After calling DoWork. Result: Work Done", output);
    }

    [Fact]
    public void ServiceWithMethodDinLogged_ShouldBeProxiedAndLog()
    {
        // Arrange
        DinContainer.RegisterUnique<ILogService, MethodInterceptedService>();

        // Act
        var service = DinContainer.Resolve<ILogService>();
        var result = service.DoWork();

        // Assert
        Assert.Equal("Method Work Done", result);
        Assert.True(service.GetType().Name.Contains("Proxy"), $"Service type {service.GetType().Name} should be a proxy");

        var output = _consoleOutput.ToString();
        Assert.Contains("[INTERCEPTOR] Before calling DoWork", output);
        Assert.Contains("[INTERCEPTOR] After calling DoWork. Result: Method Work Done", output);
    }

    [Fact]
    public void ServiceWithoutAttribute_ShouldNotBeProxied()
    {
        // Arrange
        DinContainer.RegisterUnique<ILogService, NonInterceptedService>();
        var service = DinContainer.Resolve<ILogService>();
        
        // Act
        var result = service.DoWork();

        // Assert
        Assert.Equal("No logs", result);
        Assert.DoesNotContain("Proxy", service.GetType().Name);
        
        var output = _consoleOutput.ToString();
        Assert.DoesNotContain("[INTERCEPTOR]", output);
    }

    [Fact]
    public void Interceptor_ShouldLogExceptionAndRethrow()
    {
        // Arrange
        DinContainer.RegisterUnique<IExceptionService, ExceptionService>();
        var service = DinContainer.Resolve<IExceptionService>();

        // Act & Assert
        Assert.Throws<Exception>(() => service.Throw());

        var output = _consoleOutput.ToString();
        Assert.Contains("[INTERCEPTOR] Exception in Throw: Test Exception", output);
    }
}
