using System;

namespace DinClassLibrary;

public sealed class ServiceDescriptor
{
    public Type ServiceType { get; }
    public Type? ImplementationType { get; }
    public object? ImplementationInstance { get; }
    public Func<IServiceProvider, object>? ImplementationFactory { get; }
    public ServiceLifetime Lifetime { get; }

    public ServiceDescriptor(Type serviceType,
        Type? implementationType,
        object? implementationInstance,
        Func<IServiceProvider, object>? implementationFactory,
        ServiceLifetime lifetime)
    {
        ServiceType = serviceType ?? throw new ArgumentNullException(nameof(serviceType));

        // Basic validation: instance only allowed for singleton
        if (implementationInstance is not null && lifetime != ServiceLifetime.Singleton)
        {
            throw new ArgumentException("ImplementationInstance is only valid for Singleton lifetime.", nameof(implementationInstance));
        }

        ImplementationType = implementationType;
        ImplementationInstance = implementationInstance;
        ImplementationFactory = implementationFactory;
        Lifetime = lifetime;
    }
}
