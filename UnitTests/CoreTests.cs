using System;
using DinClassLibrary;
using DinClassLibrary.Attributes;
using Xunit;

namespace UnitTests;

public class CoreTests : IDisposable
{
    private readonly TextWriter _originalOutput;
    
    public CoreTests()
    {
        // arrange
        _originalOutput = Console.Out;
        DinContainer.Clear();
    }

    public void Dispose()
    {
        DinContainer.Clear();
        Console.SetOut(_originalOutput);
    }

    [Fact]
    public void MultiLevelResolution_ShouldWork()
    {
        // arrange
        DinContainer.RegisterUnique<IService1, Service1>();
        DinContainer.RegisterUnique<IService2, Service2>();
        DinContainer.RegisterUnique<IService3, Service3>();

        // act
        var service1 = DinContainer.Resolve<IService1>();
        
        // assert
        Assert.NotNull(service1);
        Assert.IsType<Service1>(service1);
    }

    [Fact]
    public void MultiConstructorWithDefault_ShouldPreferNonDefault()
    {
        // arrange
        DinContainer.RegisterUnique<IService2, Service2>();
        DinContainer.RegisterUnique<IService3, Service3>();
        DinContainer.Register(typeof(IService1), typeof(DefaultConstructor));

        // act
        var instance = DinContainer.Resolve<IService1>();
        
        // assert
        Assert.NotNull(instance);
    }

    [Fact]
    public void OnlyDefaultConstructor_ShouldWork()
    {
        // arrange
        DinContainer.Register(typeof(IService3), typeof(Service3));
        
        // act
        var instance = DinContainer.Resolve<IService3>();
        
        // assert
        Assert.NotNull(instance);
    }

    [Fact]
    public void UnregisteredService_ShouldThrowException()
    {
        // act & assert
        Assert.Throws<DinServiceNotRegisteredException>(() => DinContainer.Resolve<IService1>());
    }

    [Fact]
    public void NoPublicConstructor_ShouldThrowException()
    {
        // arrange
        DinContainer.Register(typeof(INoConstructor), typeof(NoConstructor));
        
        // act & assert
        Assert.Throws<DinNoConstructorFoundException>(() => DinContainer.Resolve<INoConstructor>());
    }

    [Fact]
    public void CyclicDependency_ShouldThrowException()
    {
        // arrange
        DinContainer.RegisterUnique<ICycle1, Cycle1>();
        DinContainer.RegisterUnique<ICycle2, Cycle2>();

        // act & assert
        var ex = Assert.Throws<InvalidOperationException>(() => DinContainer.Resolve<ICycle1>());
        Assert.Contains("Circular dependency detected", ex.Message);
        Assert.Contains("ICycle1 -> ICycle2 -> ICycle1", ex.Message);
    }

    [Fact]
    public void SelfCyclicDependency_ShouldThrowException()
    {
        // arrange
        DinContainer.RegisterUnique<ISelfCycle, SelfCycle>();
        
        // act & assert
        var ex = Assert.Throws<InvalidOperationException>(() => DinContainer.Resolve<ISelfCycle>());
        Assert.Contains("Circular dependency detected", ex.Message);
        Assert.Contains("ISelfCycle -> ISelfCycle", ex.Message);
    }

    [Fact]
    public void AmbiguousConstructor_ShouldThrowException()
    {
        // arrange
        DinContainer.RegisterUnique<IAmbiguous, Ambiguous>();
        DinContainer.RegisterUnique<IService1, Service1>();
        DinContainer.RegisterUnique<IService2, Service2>();
        DinContainer.RegisterUnique<IService3, Service3>();

        // act & assert
        Assert.Throws<DinAmbiguousConstructorException>(() => DinContainer.Resolve<IAmbiguous>());
    }

    [Fact]
    public void Singleton_ShouldBeInstantiatedOnlyOnce()
    {
        // arrange
        Singleton.InstanceCount = 0;
        DinContainer.RegisterUnique<ISingleton, Singleton>();

        // act
        var instance1 = DinContainer.Resolve<ISingleton>();
        var instance2 = DinContainer.Resolve<ISingleton>();

        // assert
        Assert.Same(instance1, instance2);
        Assert.Equal(1, Singleton.InstanceCount);
    }
}
