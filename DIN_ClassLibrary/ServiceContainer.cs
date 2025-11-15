namespace DIN_ClassLibrary;

using System;
using System.Collections.Generic;
using System.Linq;

public class DiContainer
{
    // Maps a String Name to a specific Type
    private readonly Dictionary<string, Type> _registry = new();
    
    // 2. Registration: strictly name-based
    public void Register(string serviceName, Type implementationType)
    {
        _registry[serviceName] = implementationType;
    }
    
    // 3. Resolution: The recursive magic
    public object GetService(string serviceName)
    {
        // Validation
        if (!_registry.ContainsKey(serviceName))
        {
            throw new Exception($"Service not registered: {serviceName}");
        }
        
        // Get the first constructor
        var constructor = _registry[serviceName].GetConstructors().First();
        
        // Get the parameters that constructor needs
        var parameters = constructor.GetParameters();
        
        // Prepare the arguments list
        var args = new object[parameters.Length];
        
        for (var i = 0; i < parameters.Length; i++)
        {
            var parameter = parameters[i];
            
            // CORE LOGIC: We use the Parameter's NAME to find the dependency
            // If the parameter is "ILogger myLogger", we look for "myLogger"
            var dependentServiceName = parameter.Name;
            
            // Recursive call: Resolve the dependency
            args[i] = GetService(dependentServiceName);
        }
        
        // Create the object with the resolved arguments
        return constructor.Invoke(args);
    }
}