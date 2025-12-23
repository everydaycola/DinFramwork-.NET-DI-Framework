using System.Reflection;
using DinClassLibrary;

namespace UnitTests;

public interface IScanService { }
public class ScanService : IScanService { }

public interface IMultipleImpl { }
public class Impl1 : IMultipleImpl { }
public class Impl2 : IMultipleImpl { }

public class StandaloneService {}

public class AssemblyScanningTests : IDisposable
{
    private readonly StringWriter _consoleOutput = new();
    private readonly TextWriter _originalOutput;

    public AssemblyScanningTests()
    {
        // arrange
        DinContainer.Clear();
        DinLogger.CurrentLevel = DinLogger.LogLevel.Debug;
        _originalOutput = Console.Out;
        Console.SetOut(_consoleOutput);
    }

    public void Dispose()
    {
        DinContainer.Clear();
        Console.SetOut(_originalOutput);
        _consoleOutput.Dispose();
    }

    [Fact]
    public void RegisterAssembly_ShouldRegisterSingleImplementations()
    {
        // act
        DinContainer.RegisterAssembly(Assembly.GetExecutingAssembly());

        // assert
        var service = DinContainer.Resolve<IScanService>();
        Assert.NotNull(service);
        Assert.IsType<ScanService>(service);
    }

    [Fact]
    public void RegisterAssembly_ShouldRegisterStandaloneClasses()
    {
        // act
        DinContainer.RegisterAssembly(Assembly.GetExecutingAssembly());

        // assert
        var service = DinContainer.Resolve<StandaloneService>();
        Assert.NotNull(service);
        Assert.IsType<StandaloneService>(service);
    }

    [Fact]
    public void RegisterAssembly_ShouldSkipMultipleImplementationsAndLogWarning()
    {
        // act
        DinContainer.RegisterAssembly(Assembly.GetExecutingAssembly());

        // assert
        Assert.Throws<DinServiceNotRegisteredException>(() => DinContainer.Resolve<IMultipleImpl>());

        Assert.Contains("Multiple implementations for interface UnitTests.IMultipleImpl", _consoleOutput.ToString());
    }
}
