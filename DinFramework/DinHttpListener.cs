using System.Net;
using System.Text;

namespace DinClassLibrary;

public class DinHttpListener
{
    private readonly object _controller;
    private readonly HttpListener _listener = new();

    public DinHttpListener(object controller, string prefix = "http://localhost:9999/")
    {
        _controller = controller;
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
                TryWriteError(context.Response, 500, "Internal Server Error");
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

        // Supported routes:
        // GET    /api/numbers
        // POST   /api/numbers/{id}
        // DELETE /api/numbers/{id}

        if (segments.Length >= 2 && segments[0].Equals("api", StringComparison.OrdinalIgnoreCase) &&
            segments[1].Equals("numbers", StringComparison.OrdinalIgnoreCase))
        {
            switch (segments.Length)
            {
                // /api/numbers
                case 2 when !request.HttpMethod.Equals("GET", StringComparison.OrdinalIgnoreCase):
                    response.StatusCode = 405;
                    response.StatusDescription = "Method Not Allowed";
                    response.ContentLength64 = 0;
                    return;
                case 2:
                {
                    var method = _controller.GetType().GetMethod("GetAll");
                    if (method == null)
                        throw new MissingMethodException("GetAll not found on controller");
                    var result = method.Invoke(_controller, null);
                    var data = (ICollection<int>)result!;
                    var json = "[" + string.Join(",", data) + "]";
                    var buffer = Encoding.UTF8.GetBytes(json);
                    response.StatusCode = 200;
                    response.ContentType = "application/json; charset=utf-8";
                    response.ContentLength64 = buffer.Length;
                    response.OutputStream.Write(buffer, 0, buffer.Length);
                    return;
                }
                case 3:
                {
                    if (!int.TryParse(segments[2], out var id))
                    {
                        TryWriteError(response, 400, "Invalid id");
                        return;
                    }

                    if (request.HttpMethod.Equals("POST", StringComparison.OrdinalIgnoreCase))
                    {
                        try
                        {
                            InvokeSingleIntParamMethod("Post", id);
                            response.StatusCode = 201;
                            response.Headers["Location"] = $"/api/numbers/{id}";
                            response.ContentLength64 = 0;
                        }
                        catch (ArgumentException)
                        {
                            TryWriteError(response, 409, "Conflict");
                        }

                        return;
                    }

                    if (request.HttpMethod.Equals("DELETE", StringComparison.OrdinalIgnoreCase))
                    {
                        try
                        {
                            InvokeSingleIntParamMethod("Delete", id);
                            response.StatusCode = 204;
                            response.StatusDescription = "No Content";
                            response.ContentLength64 = 0;
                        }
                        catch (ArgumentException)
                        {
                            TryWriteError(response, 404, "Not Found");
                        }

                        return;
                    }

                    response.StatusCode = 405;
                    response.StatusDescription = "Method Not Allowed";
                    response.ContentLength64 = 0;
                    return;
                }
            }
        }

        // If reached here: not found
        TryWriteError(response, 404, "Not Found");
    }

    private void InvokeSingleIntParamMethod(string methodName, int id)
    {
        var method = _controller.GetType().GetMethod(methodName);
        if (method == null)
            throw new MissingMethodException($"{methodName} not found on controller");
        var parameters = method.GetParameters();
        if (parameters.Length != 1 || parameters[0].ParameterType != typeof(int))
            throw new MissingMethodException($"{methodName}(int id) signature not found on controller");
        method.Invoke(_controller, [id]);
    }

    private static void TryWriteError(HttpListenerResponse response, int status, string message)
    {
        try
        {
            Console.WriteLine($"[DinHttpListener Error] {status} {message}");
            var payload = Encoding.UTF8.GetBytes(message);
            response.StatusCode = status;
            response.ContentType = "text/plain; charset=utf-8";
            response.ContentLength64 = payload.Length;
            response.OutputStream.Write(payload, 0, payload.Length);
        }
        catch
        {
            Console.WriteLine("[DinHttpListener Error] Failed to write error response");
        }
    }
}