using System.Net;
using DinClassLibrary.Attributes;
using Castle.DynamicProxy;

namespace DinClassLibrary.Routing;

public class DinHttpListener
{
    private readonly Dictionary<string, object> _controllersBySegment = new(StringComparer.OrdinalIgnoreCase);
    private readonly HttpListener _listener = new();

    public DinHttpListener(IEnumerable<object> controllers, string prefix = "http://localhost:9999/")
    {
        foreach (var ctrl in controllers)
        {
            var type = ctrl.GetType();

            // for logging interception, the classes are made as a proxy.
            // these proxies don't have the original annotations, so you need the original class as well. 
            if (ctrl is IProxyTargetAccessor accessor)
            {
                type = accessor.DynProxyGetTarget().GetType();
            }

            // get the attribute
            var apiAttr = type.GetCustomAttributes(typeof(DinApiControllerAttribute), true)
                .Cast<DinApiControllerAttribute>()
                .FirstOrDefault();

            if (apiAttr == null) continue; // not an API controller: skip
            
            // get the value of the annotation
            var key = apiAttr.Segment;
            
            // register the controllers by api segment
            if (_controllersBySegment.TryGetValue(key, out var value))
                throw new InvalidOperationException(
                    $"Duplicate API segment '{key}' found for controllers '{value.GetType().Name}' and '{type.Name}'. Segments must be unique.");
            
            _controllersBySegment[key] = ctrl;
        }
        _listener.Prefixes.Add(prefix);
    }

    public void Start()
    {
        _listener.Start();
        DinLogger.LogInfo("DinHttpListener started on: " + string.Join(", ", _listener.Prefixes));

        while (true)
        {
            // wait for a request
            var context = _listener.GetContext();
            try
            {
                HandleRequest(context);
            }
            catch (Exception ex)
            {
                DinLogger.LogError("[DinHttpListener Error] " + ex);
                ResponseWriter.WriteError(context.Response, 500, "Internal Server Error");
            }
            finally
            {
                context.Response.OutputStream.Close();
            }
        }
    }

    private void HandleRequest(HttpListenerContext context)
    {
        // get the segments of the URL (like ["api", "controller", "method/args"...])
        var segments = (context.Request.Url?.AbsolutePath ?? "/")
            .Trim('/')
            .Split('/', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        // Fast pre-selection by segment using [DinApiController] annotations
        if (
            // one for api, one for controller, and one for the method/args
            segments.Length >= 2 
            // the first segment must be "api"
            && segments[0].Equals("api") 
            // the second segment must be a valid controller (also get the controller)
            && _controllersBySegment.TryGetValue(segments[1], out var controller) 
            // the third segment must be a valid method. Then execute it
            && ConventionRouter.TryHandle(context, controller, segments)
            // if this all succeeds, return
            ) return;
        // else, give a 404 for method/controller not found
        ResponseWriter.WriteError(context.Response, 404, "Not Found");
    }
}