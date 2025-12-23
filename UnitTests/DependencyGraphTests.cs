using DinClassLibrary;

namespace UnitTests;

public class DependencyGraphTests : IDisposable
{
    private readonly StringWriter _consoleOutput = new();
    private readonly TextWriter _originalOutput;

    public DependencyGraphTests()
    {
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
    public void CheckForCycles_ShouldNotThrowIfNoCycles()
    {
        // arrange
        DinContainer.RegisterUnique<IService1, Service1>();
        DinContainer.RegisterUnique<IService2, Service2>();
        DinContainer.RegisterUnique<IService3, Service3>();

        // act & assert
        DinContainer.CheckForCycles();
    }

    [Fact]
    public void PrintGraph_ShouldOutputGraphStructure()
    {
        // arrange
        DinContainer.RegisterUnique<IService1, Service1>();
        DinContainer.RegisterUnique<IService2, Service2>();
        DinContainer.RegisterUnique<IService3, Service3>();

        // Resolve to build the graph edges
        DinContainer.Resolve<IService1>();

        DinContainer.PrintGraph(typeof(IService1));

        var output = _consoleOutput.ToString();
        Assert.Contains("Dependency graph:", output);
        Assert.Contains("IService1", output);
        Assert.Contains("IService2", output);
        Assert.Contains("Total vertices:", output);
    }

    [Fact]
    public void Resolve_ShouldFailIfCycleDetected()
    {
        // arrange
        DinContainer.RegisterUnique<ICycle1, Cycle1>();
        DinContainer.RegisterUnique<ICycle2, Cycle2>();
        
        // act & assert
        Assert.Throws<InvalidOperationException>(() => DinContainer.Resolve<ICycle1>());
    }
}
