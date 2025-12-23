using System;

namespace DinClassLibrary;

public class DinNoConstructorFoundException : Exception
{
    public DinNoConstructorFoundException(Type serviceType) 
        : base($"No public constructor found for: {serviceType.FullName}")
    {
    }
}