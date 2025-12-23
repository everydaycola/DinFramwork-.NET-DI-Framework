using System;

namespace DinClassLibrary;

public class DinServiceNotRegisteredException : Exception
{
    public DinServiceNotRegisteredException(Type serviceType) 
        : base($"Service not registered: {serviceType.FullName}")
    {
    }
}