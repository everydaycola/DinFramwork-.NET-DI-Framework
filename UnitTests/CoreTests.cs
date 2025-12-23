using System;
using DinClassLibrary;
using DinClassLibrary.Attributes;
using Xunit;

namespace UnitTests;

public interface IServiceA { }
public class ServiceA : IServiceA
{
    public ServiceA(IServiceB serviceB) { }
}

public interface IServiceB { }
public class ServiceB : IServiceB { }

public interface ICycleA { }
public class CycleA : ICycleA
{
    public CycleA(ICycleB cycleB) { }
}

public interface ICycleB { }
public class CycleB : ICycleB
{
    public CycleB(ICycleA cycleA) { }
}

public interface IAmbiguous { }
public class Ambiguous : IAmbiguous
{
    public Ambiguous(IServiceA a) { }
    public Ambiguous(IServiceB b) { }
}

public class CoreTests
{
    [Fact]
    public void SimpleResolution_ShouldWork()
    {
        DinContainer.RegisterUnique<IServiceA, ServiceA>();
        DinContainer.RegisterUnique<IServiceB, ServiceB>();

        var serviceA = DinContainer.Resolve<IServiceA>();
        Assert.NotNull(serviceA);
        Assert.IsType<ServiceA>(serviceA);
    }

    [Fact]
    public void CyclicDependency_ShouldThrowException()
    {
        DinContainer.RegisterUnique<ICycleA, CycleA>();
        DinContainer.RegisterUnique<ICycleB, CycleB>();

        Assert.Throws<InvalidOperationException>(() => DinContainer.Resolve<ICycleA>());
    }

    [Fact]
    public void AmbiguousConstructor_ShouldThrowException()
    {
        DinContainer.RegisterUnique<IAmbiguous, Ambiguous>();
        DinContainer.RegisterUnique<IServiceA, ServiceA>();
        DinContainer.RegisterUnique<IServiceB, ServiceB>();

        Assert.Throws<DinAmbiguousConstructorException>(() => DinContainer.Resolve<IAmbiguous>());
    }

    public interface ISingleton { }
    public class Singleton : ISingleton
    {
        public static int InstanceCount = 0;
        public Singleton()
        {
            InstanceCount++;
        }
    }

    [Fact]
    public void Singleton_ShouldBeInstantiatedOnlyOnce()
    {
        Singleton.InstanceCount = 0;
        DinContainer.RegisterUnique<ISingleton, Singleton>();

        var instance1 = DinContainer.Resolve<ISingleton>();
        var instance2 = DinContainer.Resolve<ISingleton>();

        Assert.Same(instance1, instance2);
        Assert.Equal(1, Singleton.InstanceCount);
    }
}
