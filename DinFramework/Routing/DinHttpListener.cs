using System.Net;
using DinClassLibrary.Attributes;
using DinClassLibrary.Routing;

namespace DinClassLibrary;

public class DinHttpListener
{
    private readonly Dictionary<string, object> _controllersBySegment = new(StringComparer.OrdinalIgnoreCase);
    private readonly HttpListener _listener = new();

    public DinHttpListener(IEnumerable<object> controllers, string prefix = "http://localhost:9999/")
    {
        foreach (var ctrl in controllers)
        {
            var type = ctrl.GetType();
            var apiAttr = type.GetCustomAttributes(typeof(DinApiControllerAttribute), true)
                .Cast<DinApiControllerAttribute>()
                .FirstOrDefault();

            if (apiAttr == null)
                continue; // not an API controller; skip

            
            var key = apiAttr.Segment;
            if (_controllersBySegment.ContainsKey(key))
                throw new InvalidOperationException($"Duplicate API segment '{key}' found for controllers '{_controllersBySegment[key].GetType().Name}' and '{type.Name}'. Segments must be unique.");

            _controllersBySegment[key] = ctrl;
        }
        _listener.Prefixes.Add(prefix);
    }

    public void Start()
    {
        _listener.Start();
        Console.WriteLine("DinHttpListener started on: " + string.Join(", ", _listener.Prefixes));

        while (true)
        {
            var context = _listener.GetContext();
            try
            {
                HandleRequest(context);
            }
            catch (Exception ex)
            {
                Console.WriteLine("[DinHttpListener Error] " + ex);
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
        var request = context.Request;
        var response = context.Response;

        var path = request.Url?.AbsolutePath ?? "/";
        var segments = path.Trim('/')
            .Split('/', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        // Fast pre-selection by segment using [DinApiController] annotations
        if (segments.Length >= 2 && segments[0].Equals("api", StringComparison.OrdinalIgnoreCase))
        {
            var seg = segments[1];
            if (_controllersBySegment.TryGetValue(seg, out var controller) 
                && ConventionRouter.TryHandle(context, controller, segments)) {
                    return;
            }
        }
        
        ResponseWriter.WriteError(response, 404, "Not Found");
    }
}