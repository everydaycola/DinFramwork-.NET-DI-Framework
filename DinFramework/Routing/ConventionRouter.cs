using System.Net;
using System.Reflection;
using DinClassLibrary.Attributes;
using Castle.DynamicProxy;

namespace DinClassLibrary.Routing;

internal static class ConventionRouter
{
    private static readonly HashSet<Type> ValidatedControllers = [];
    private static readonly Dictionary<Type, List<RouteInfo>> RouteCache = [];
    private sealed record RouteInfo(string HttpMethod, MethodInfo Method, int ParameterCount);

    public static bool TryHandle(HttpListenerContext context, object controller, string[] requestSegments)
    {
        // get the controller type
        var targetType = GetTargetType(controller);
        // get the annotation
        var apiAttr = targetType.GetCustomAttribute<DinApiControllerAttribute>();
        // check for a valid annotation and valid api path.
        if (apiAttr == null || !IsValidPath(requestSegments, apiAttr.Segment)) 
            return false;

        EnsureControllerInitialized(targetType);

        if (!TryGetRouteAndArgs(context, requestSegments, targetType, out var route, out var args))
            return route == null && args == null; // return true if error was already written to response

        try
        {
            var methodToInvoke = ResolveMethodForInvoke(controller, route.Method);
            var result = methodToInvoke.Invoke(controller, args);
            SendResponse(context, route.Method, result);
            return true;
        }
        catch (TargetInvocationException ex) when (ex.InnerException is ArgumentException argEx)
        {
            HandleException(context, context.Request.HttpMethod.ToUpperInvariant(), argEx);
            return true;
        }
    }
    
    // for proxies, we need `DynProxyGetTarget` to get the original type, which is needed to read annotations.
    private static Type GetTargetType(object controller) =>
        controller is IProxyTargetAccessor accessor ? accessor.DynProxyGetTarget().GetType() : controller.GetType();

    private static bool IsValidPath(string[] segments, string segmentAttr) =>
        segments.Length is >= 2 and <= 3 &&
        segments[0].Equals("api", StringComparison.OrdinalIgnoreCase) &&
        segments[1].Equals(segmentAttr, StringComparison.OrdinalIgnoreCase);
    
    private static void EnsureControllerInitialized(Type type)
    {
        if (ValidatedControllers.Add(type)) ValidateControllerConventions(type);
        if (!RouteCache.ContainsKey(type)) CacheRoutes(type);
    }
    
    private static bool TryGetRouteAndArgs(HttpListenerContext context, string[] segments, Type type, out RouteInfo route, out object[] args)
    {
        var httpMethod = context.Request.HttpMethod.ToUpperInvariant();
        var routes = RouteCache[type];
        args = [];

        if (segments.Length == 2)
        {
            route = routes.FirstOrDefault(r => r.HttpMethod == httpMethod && r.ParameterCount == 0);
            return route != null;
        }

        if (!int.TryParse(segments[2], out var id))
        {
            ResponseWriter.WriteError(context.Response, 400, "Invalid id");
            route = null;
            args = null;
            return false;
        }

        route = routes.FirstOrDefault(r => 
            r.HttpMethod == httpMethod && 
            r.ParameterCount == 1 && 
            r.Method.GetParameters()[0].ParameterType == typeof(int));
        
        if (route != null) args = [id];
        return route != null;
    }
    
    private static MethodInfo ResolveMethodForInvoke(object controller, MethodInfo targetMethod)
    {
        if (controller is not IProxyTargetAccessor) return targetMethod;
        
        var proxyType = controller.GetType();
        var map = proxyType.GetInterfaceMap(targetMethod.DeclaringType!);
        var index = Array.IndexOf(map.TargetMethods, targetMethod);
        return map.InterfaceMethods[index];
    }
    
    private static void SendResponse(HttpListenerContext context, MethodInfo method, object result)
    {
        var response = context.Response;
        if (method.ReturnType == typeof(void))
        {
            if (context.Request.HttpMethod.Equals("POST", StringComparison.InvariantCultureIgnoreCase)) 
                ResponseWriter.WriteCreated(response);
            else 
                ResponseWriter.WriteNoContent(response);
        }
        else if (result is ICollection<int> ints)
            ResponseWriter.WriteJson(response, $"[{string.Join(",", ints)}]");
        else
            ResponseWriter.WriteText(response, 200, result?.ToString() ?? string.Empty);
    }
    
    private static void HandleException(HttpListenerContext context, string httpMethod, ArgumentException ex)
    {
        var statusCode = httpMethod switch
        {
            "POST" => 409,
            "DELETE" => 404,
            _ => 400
        };
        ResponseWriter.WriteError(context.Response, statusCode, statusCode == 400 ? "Bad Request" : ex.Message);
    }

    private static string InferHttpMethodFromName(string name)
    {
        if (name.StartsWith("Get", StringComparison.OrdinalIgnoreCase)) return "GET";
        if (name.StartsWith("Post", StringComparison.OrdinalIgnoreCase)) return "POST";
        if (name.StartsWith("Put", StringComparison.OrdinalIgnoreCase)) return "PUT";
        if (name.StartsWith("Delete", StringComparison.OrdinalIgnoreCase)) return "DELETE";
        if (name.StartsWith("Patch", StringComparison.OrdinalIgnoreCase)) return "PATCH";
        if (name.StartsWith("Options", StringComparison.OrdinalIgnoreCase)) return "OPTIONS";
        if (name.StartsWith("Head", StringComparison.OrdinalIgnoreCase)) return "HEAD";
        return null;
    }

    private static void ValidateControllerConventions(Type type)
    {
        foreach (var m in type.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly))
        {
            if (m.IsSpecialName) // skip property getters/setters
                continue;
            if (InferHttpMethodFromName(m.Name) == null)
                throw new InvalidOperationException($"Method '{m.Name}' in controller '{type.Name}' must start with an HTTP verb (GET, POST, PUT, DELETE, PATCH, OPTIONS, HEAD).");
        }
    }

    private static void CacheRoutes(Type type)
    {
        var methods = type.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly);
        var routes = (
            from m in methods 
            where !m.IsSpecialName 
            let httpMethod = InferHttpMethodFromName(m.Name) 
            where httpMethod != null 
            select new RouteInfo(httpMethod, m, m.GetParameters().Length)
            ).ToList();
        RouteCache[type] = routes;
    }
}
