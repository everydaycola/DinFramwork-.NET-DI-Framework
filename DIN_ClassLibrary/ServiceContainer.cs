using System.Reflection;

namespace DIN_ClassLibrary;

using System;
using System.Collections.Generic;
using System.Linq;

public class DiContainer
{
    // Maps a String Name to a specific Type
    private readonly Dictionary<Type, Type> _registry = new();
    
    private readonly DependencyGraph _dependencyGraph = new ();
    
    // 2. Registration
    public void Register(Type serviceType, Type implementationType)
    {
        Console.WriteLine($"Registering service: {serviceType.Name} with implementation type: {implementationType.Name}");
        _registry[serviceType] = implementationType;
        _dependencyGraph.AddVertex(serviceType);
    }
    
    // 3. Resolution: The recursive magic
    public object GetService(Type serviceType)
    {
        Console.WriteLine($"Resolving service: {serviceType.Name}");
        // Validation
        if (!_registry.TryGetValue(serviceType, out var registryValue))
        {
            Console.WriteLine($"Failed to resolve service: {serviceType.Name} - Not registered");
            throw new Exception($"Service not registered: {serviceType.Name}");
        }
        
        // Get constructors
        var constructors = registryValue.GetConstructors();
        
        // Just get the first for now
        var firstConstructor = constructors.First();
        
        // Get the parameters that the constructor needs
        var parameters = firstConstructor.GetParameters();
        
        // Prepare the argument list
        var args = new object[parameters.Length];
        
        for (var i = 0; i < parameters.Length; i++)
        {
            var parameterType = parameters[i].ParameterType;
            Console.WriteLine($"Resolving dependency parameter: {parameterType.Name} for service: {serviceType.Name}");
            // Recursive call: Resolve the dependency
            args[i] = GetService(parameterType);
            _dependencyGraph.AddEdge(serviceType, parameterType);
        }

        // Create the object with the resolved arguments
        Console.WriteLine($"Creating instance of service: {serviceType.Name}");
        return firstConstructor.Invoke(args);
    }
    
    public void PrintGraph()
    {
        _dependencyGraph.PrintGraph();
    }
}