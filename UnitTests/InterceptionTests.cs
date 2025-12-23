using DinClassLibrary;
using DinClassLibrary.Attributes;
using Xunit;
using System.Reflection;

namespace UnitTests;

public interface ITestService
{
    string DoWork();
}

[DinAutoLogging]
public class TestService : ITestService
{
    public string DoWork()
    {
        return "Work Done";
    }
}

public interface ITestMethodService
{
    string DoWork();
}

public class TestMethodService : ITestMethodService
{
    [DinAutoLogging]
    public string DoWork()
    {
        return "Method Work Done";
    }
}

public class InterceptionTests
{
    [Fact]
    public void ServiceWithDinLogged_ShouldBeProxiedAndLog()
    {
        // Arrange
        DinContainer.RegisterUnique<ITestService, TestService>();
        
        // Act
        var service = DinContainer.Resolve<ITestService>();
        var result = service.DoWork();

        // Assert
        Assert.Equal("Work Done", result);
        Assert.True(service.GetType().Name.Contains("Proxy"), $"Service type {service.GetType().Name} should be a proxy");
    }

    [Fact]
    public void ServiceWithMethodDinLogged_ShouldBeProxiedAndLog()
    {
        // Arrange
        DinContainer.RegisterUnique<ITestMethodService, TestMethodService>();

        // Act
        var service = DinContainer.Resolve<ITestMethodService>();
        var result = service.DoWork();

        // Assert
        Assert.Equal("Method Work Done", result);
        Assert.True(service.GetType().Name.Contains("Proxy"), $"Service type {service.GetType().Name} should be a proxy");
    }
}
