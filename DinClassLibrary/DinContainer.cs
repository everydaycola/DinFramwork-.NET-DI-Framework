namespace DinClassLibrary;

using System;
using System.Collections.Generic;
using System.Linq;

public class DinContainer
{
    // Maps a String Name to a specific Type
    private readonly Dictionary<Type, Type> _registry = new();
    
    private readonly DinDependencyGraph _dependencyGraph = new();
    
    // 2. Registration
    public void Register(Type serviceType, Type implementationType)
    {
        Console.WriteLine(
            $"Registering service: {serviceType.Name} with implementation type: {implementationType.Name}");
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
        
        // Get non default constructors
        var constructors = registryValue.GetConstructors();
        var defaultConstructor = constructors
            .Where(c => c.GetParameters().Length == 0)
            .ToList();
        var nonDefaultConstructors = constructors
            .Where(c => c.GetParameters().Length != 0)
            .ToList();
        
        if (!nonDefaultConstructors.Any())
        {
            return defaultConstructor.Count == 0
                // todo throw better exception
                ? throw new Exception($"No constructor found for: {serviceType.Name}") 
                : defaultConstructor[0].Invoke(null);
        }
        
        if (nonDefaultConstructors.Count >= 2)
        {
            Console.WriteLine($"Multiple non-default constructors found for: {serviceType.Name}");
            foreach (var c in nonDefaultConstructors)
            {
                // todo test if output is usefull
                Console.WriteLine(c.ToString());
            }
            // todo throw better exception
            throw new Exception($"Multiple non-default constructors found for: {serviceType.Name}");
        }
        
        var constructor = nonDefaultConstructors[0];
        
        // Get the parameters that the constructor needs
        var parameters = constructor.GetParameters();
        
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
        return constructor.Invoke(args);
    }
    
    public void PrintGraph(Type startNode)
    {
        _dependencyGraph.PrintGraph(startNode);
    }
}