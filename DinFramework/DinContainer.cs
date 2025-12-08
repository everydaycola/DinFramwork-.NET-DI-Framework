using System.Reflection;

namespace DinClassLibrary;

using System;
using System.Collections.Generic;
using System.Linq;

public class DinContainer : IServiceProvider
{
    private static readonly Dictionary<Type, Type> Registry = new();

    private static readonly DinDependencyGraph DependencyGraph = new();

    public static void RegisterAssembly(Assembly assembly)
    {
        var types = assembly.GetTypes();
        var interfaces = types.Where(t => t.IsInterface).ToList();
        var classes = types.Where(t => t.IsClass && !t.IsAbstract && t.IsPublic).ToList();

        foreach (var intf in interfaces)
        {
            var impls = classes.Where(c => intf.IsAssignableFrom(c)).ToList();

            switch (impls.Count)
            {
                case 1:
                    Register(intf, impls[0]);
                    break;
                case 0:
                    Console.WriteLine($"[AssemblyScan] No implementation found for interface {intf.FullName} in {assembly.GetName().Name}");
                    break;
                default:
                    Console.WriteLine($"[AssemblyScan] Multiple implementations for interface {intf.FullName}: {string.Join(", ", impls.Select(t => t.FullName))}. Skipping auto-registration.");
                    break;
            }
        }
    }

    // 2. Registration
    public static void Register(Type serviceType, Type implementationType)
    {
        Console.WriteLine(
            $"Registering service: {serviceType.Name} with implementation type: {implementationType.Name}");
        Registry[serviceType] = implementationType;
        DependencyGraph.AddVertex(serviceType);
    }

    // 3. Resolution: The recursive magic
    public static object GetService(Type serviceType)
    {
        Console.WriteLine($"Resolving service: {serviceType.Name}");
        // Validation
        if (!Registry.TryGetValue(serviceType, out var registryValue))
        {
            Console.WriteLine($"Failed to resolve service: {serviceType.Name} - Not registered");
            // todo throw better exception
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

        switch (nonDefaultConstructors.Count)
        {
            case 0:
                return defaultConstructor.Count == 0
                    // todo throw better exception
                    ? throw new Exception($"No constructor found for: {serviceType.Name}")
                    : defaultConstructor[0].Invoke(null);
            case >= 2:
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
            DependencyGraph.AddEdge(serviceType, parameterType);
        }

        // Create the object with the resolved arguments
        Console.WriteLine($"Creating instance of service: {serviceType.Name}");
        return constructor.Invoke(args);
    }
    
    object? IServiceProvider.GetService(Type serviceType)
    {
        return GetService(serviceType);
    }

    public static void PrintGraph(Type startNode)
    {
        DependencyGraph.PrintGraph(startNode);
    }

    public static void CheckForCycles()
    {
        DependencyGraph.CheckForCycles();
    }
}