using System.Reflection;
using DinClassLibrary.Attributes;
using Castle.DynamicProxy;
using DinClassLibrary.Logging;
using DinClassLibrary.Routing;

namespace DinClassLibrary;

using System;
using System.Collections.Generic;
using System.Linq;

public class DinContainer
{
    private static readonly Dictionary<Type, Type> Registry = new();
    private static readonly Dictionary<Type, object> Instances = new();

    private static readonly DinDependencyGraph DependencyGraph = new();
    private static readonly ProxyGenerator ProxyGenerator = new();

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
                    DinLogger.LogError($"[AssemblyScan] No implementation found for interface {intf.FullName} in {assembly.GetName().Name}");
                    break;
                default:
                    DinLogger.LogError($"[AssemblyScan] Multiple implementations for interface {intf.FullName}: {string.Join(", ", impls.Select(t => t.FullName))}. Skipping auto-registration.");
                    break;
            }
        }
    }

    // 2. Registration
    public static void Register(Type serviceType, Type implementationType)
    {
        DinLogger.LogInfo(
            $"Registering service: {serviceType.Name} with implementation type: {implementationType.Name}");
        Registry[serviceType] = implementationType;
        // If re-registering a service, drop any existing instance so the next resolve creates the new mapping
        Instances.Remove(serviceType);
        DependencyGraph.AddVertex(serviceType);
    }

    // Convenience generic registration for singletons
    public static void RegisterUnique<TInterface, TImplementation>() where TImplementation : TInterface
    {
        Register(typeof(TInterface), typeof(TImplementation));
    }

    // 3. Resolution: The recursive magic
    public static object GetService(Type serviceType)
    {
        DinLogger.LogInfo($"Resolving service: {serviceType.Name}");
        // Return cached singleton if available
        if (Instances.TryGetValue(serviceType, out var existing))
        {
            DinLogger.LogInfo($"Returning cached instance of service: {serviceType.Name}");
            return existing;
        }
        // Validation
        if (!Registry.TryGetValue(serviceType, out var registryValue))
        {
            DinLogger.LogError($"Failed to resolve service: {serviceType.Name} - Not registered");
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
                if (defaultConstructor.Count == 0)
                {
                    // todo throw better exception
                    throw new Exception($"No constructor found for: {serviceType.Name}");
                }
                DinLogger.LogInfo($"Creating instance of service (default ctor): {serviceType.Name}");
                var inst = defaultConstructor[0].Invoke(null);

                // Interception logic for default constructor
                if (serviceType.IsInterface && (registryValue.GetCustomAttribute<DinAutoLoggingAttribute>() != null ||
                                                registryValue.GetMethods().Any(m => m.GetCustomAttribute<DinAutoLoggingAttribute>() != null)))
                {
                    DinLogger.LogInfo($"Applying interception to service: {serviceType.Name}");
                    inst = ProxyGenerator.CreateInterfaceProxyWithTarget(serviceType, inst, new LoggingInterceptor());
                }

                Instances[serviceType] = inst;
                return inst;
            case >= 2:
            {
                DinLogger.LogInfo($"Multiple non-default constructors found for: {serviceType.Name}");
                foreach (var c in nonDefaultConstructors)
                {
                    // todo test if output is usefull
                    DinLogger.LogInfo(c.ToString());
                }

                throw new DinAmbiguousConstructorException(serviceType);
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
            DinLogger.LogInfo($"Resolving dependency parameter: {parameterType.Name} for service: {serviceType.Name}");
            // First, record the dependency edge and check for cycles before attempting recursion
            DependencyGraph.AddEdge(serviceType, parameterType);
            // This will throw immediately if a cycle is detected, preventing deep recursion/StackOverflow
            DependencyGraph.CheckForCycles();

            // Recursive call: Resolve the dependency
            args[i] = GetService(parameterType);
        }

        // Create the object with the resolved arguments
        DinLogger.LogInfo($"Creating instance of service: {serviceType.Name}");
        var instance = constructor.Invoke(args);

        // Interception logic
        if (serviceType.IsInterface && (registryValue.GetCustomAttribute<DinAutoLoggingAttribute>() != null ||
                                        registryValue.GetMethods().Any(m => m.GetCustomAttribute<DinAutoLoggingAttribute>() != null)))
        {
            DinLogger.LogInfo($"Applying interception to service: {serviceType.Name}");
            instance = ProxyGenerator.CreateInterfaceProxyWithTarget(serviceType, instance, new LoggingInterceptor());
        }

        // Cache instance as singleton for the requested service type
        Instances[serviceType] = instance;
        return instance;
    }

    // Convenience generic resolver
    public static T Resolve<T>()
    {
        return (T)GetService(typeof(T));
    }

    public static void PrintGraph(Type startNode)
    {
        DependencyGraph.PrintGraph(startNode);
    }

    public static void CheckForCycles()
    {
        DependencyGraph.CheckForCycles();
    }
    
    public static void StartApiControllersFromAssembly(Assembly assembly)
    {
        
        var controllers = new List<object>();
        var firstControllerServiceType = new List<Type>();
        var types = assembly.GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract && t.IsPublic && t.GetCustomAttribute<DinApiControllerAttribute>() != null)
            .ToList();

        
        foreach (var t in types)
        {
            // Find a registered service mapping that points to this implementation type
            var mapping = Registry.FirstOrDefault(kv => kv.Value == t);
            if (mapping.Key != null)
            {
                try
                {
                    var instance = GetService(mapping.Key);
                    controllers.Add(instance);
                    // Remember the interface (service) type for dependency graph printing
                    firstControllerServiceType.Add(mapping.Key);
                }
                catch (Exception ex)
                {
                    DinLogger.LogWarn($"[ResolveApiControllers] Failed to resolve controller {t.Name}: {ex.Message}");
                }
            }
            else
            {
                DinLogger.LogInfo($"[ResolveApiControllers] No registered service mapping found for controller {t.FullName}. Skipping.");
            }
        }
        
        foreach (var iController in firstControllerServiceType)
        {
            PrintGraph(iController);
        }

        new DinHttpListener(controllers).Start();
    }
}