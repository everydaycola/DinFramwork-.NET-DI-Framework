using System.Net;
using System.Reflection;
using DinClassLibrary.Attributes;

namespace DinClassLibrary.Routing;

internal static class ConventionRouter
{
    private static readonly HashSet<Type> ValidatedControllers = [];

    public static bool TryHandle(HttpListenerContext context, object controller, string[] requestSegments)
    {
        var request = context.Request;
        var response = context.Response;

        var type = controller.GetType();
        var apiAttr = type.GetCustomAttribute<DinApiControllerAttribute>();
        if (apiAttr == null)
            return false;

        // Validate naming conventions once per controller type
        if (!ValidatedControllers.Contains(type))
        {
            ValidateControllerConventions(type);
            ValidatedControllers.Add(type);
        }

        // Expect path like /api/{segment} or /api/{segment}/{id}
        if (requestSegments.Length is < 2 or > 3)
            return false;

        if (!requestSegments[0].Equals("api", StringComparison.OrdinalIgnoreCase))
            return false;
        if (!requestSegments[1].Equals(apiAttr.Segment, StringComparison.OrdinalIgnoreCase))
            return false;

        var httpMethod = request.HttpMethod.ToUpperInvariant();

        // Determine parameter binding based on extra segment count
        var extraCount = requestSegments.Length - 2; // 0 or 1
        var args = Array.Empty<object>();

        MethodInfo target = null;
        var methods = type.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly);

        if (extraCount == 0)
        {
            // Match method starting with verb and zero parameters
            target = methods.FirstOrDefault(m => InferHttpMethodFromName(m.Name) == httpMethod && m.GetParameters().Length == 0);
        }
        else // extraCount == 1
        {
            var seg = requestSegments[2];
            // Only support int for now
            if (!int.TryParse(seg, out var id))
            {
                ResponseWriter.WriteError(response, 400, "Invalid id");
                return true;
            }
            target = methods.FirstOrDefault(m => InferHttpMethodFromName(m.Name) == httpMethod
                                                  && m.GetParameters().Length == 1
                                                  && m.GetParameters()[0].ParameterType == typeof(int));
            args = target != null ? new object[] { id } : Array.Empty<object>();
        }

        if (target == null)
            return false; // let other mechanisms try

        try
        {
            var result = target.Invoke(controller, args);

            if (target.ReturnType == typeof(void))
            {
                // Choose defaults like before
                if (httpMethod == "POST")
                {
                    ResponseWriter.WriteCreated(response);
                }
                else if (httpMethod == "DELETE")
                {
                    ResponseWriter.WriteNoContent(response);
                }
                else
                {
                    ResponseWriter.WriteNoContent(response);
                }
            }
            else if (result is ICollection<int> ints)
            {
                var json = "[" + string.Join(",", ints) + "]";
                ResponseWriter.WriteJson(response, json);
            }
            else
            {
                ResponseWriter.WriteText(response, 200, result?.ToString() ?? string.Empty);
            }

            return true;
        }
        catch (TargetInvocationException ex) when (ex.InnerException is ArgumentException argEx)
        {
            // Map common patterns from demo
            if (httpMethod == "POST")
                ResponseWriter.WriteError(response, 409, argEx.Message);
            else if (httpMethod == "DELETE")
                ResponseWriter.WriteError(response, 404, argEx.Message);
            else
                ResponseWriter.WriteError(response, 400, "Bad Request");
            return true;
        }
    }

    private static string? InferHttpMethodFromName(string name)
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
        var declared = type.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly);
        foreach (var m in declared)
        {
            if (m.IsSpecialName) // skip property getters/setters
                continue;
            var verb = InferHttpMethodFromName(m.Name);
            if (verb == null)
            {
                throw new InvalidOperationException($"Method '{m.Name}' in controller '{type.Name}' must start with an HTTP verb (GET, POST, PUT, DELETE, PATCH, OPTIONS, HEAD).");
            }
        }
    }
}
