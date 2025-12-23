using DinClassLibrary.Attributes;

namespace UnitTests;

// This contains a lot of non op classes and interfaces to test the DI container

// Generic Services for general DI testing
public interface IService1 {}
public interface IService2 {}
public interface IService3 {}

public class Service3 : IService3;
public class Service2(IService3 service3) : IService2;
public class Service1(IService2 service2) : IService1;

// Cycle detection
public interface ICycle1 {}
public interface ICycle2 {}
public class Cycle1(ICycle2 cycle2) : ICycle1;
public class Cycle2(ICycle1 cycle1) : ICycle2;

public interface ISelfCycle {}
public class SelfCycle(ISelfCycle self) : ISelfCycle;

// Constructor selection & errors
public interface IAmbiguous {}
public class Ambiguous : IAmbiguous
{
    public Ambiguous(IService1 s1) {}
    public Ambiguous(IService2 s2) {}
}

public class DefaultConstructor : IService1
{
    public DefaultConstructor() {}
    public DefaultConstructor(IService2 b) {}
}

public interface INoConstructor {}
public class NoConstructor : INoConstructor
{
    private NoConstructor() {}
}

// Singleton testing
public interface ISingleton {}
public class Singleton : ISingleton
{
    public static int InstanceCount;
    public Singleton() => InstanceCount++;
}

// Interception & Logging
public interface ILogService { string DoWork(); }

[DinAutoLogging]
public class InterceptedService : ILogService
{
    public string DoWork() => "Work Done";
}

public class MethodInterceptedService : ILogService
{
    [DinAutoLogging]
    public string DoWork() => "Method Work Done";
}

public class NonInterceptedService : ILogService
{
    public string DoWork() => "No logs";
}

public interface IExceptionService { void Throw(); }

[DinAutoLogging]
public class ExceptionService : IExceptionService
{
    public void Throw() => throw new Exception("Test Exception");
}