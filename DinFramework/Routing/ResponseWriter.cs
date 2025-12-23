using System.Net;
using System.Text;

namespace DinClassLibrary.Routing;

internal static class ResponseWriter
{
    public static void WriteJson(HttpListenerResponse response, string json)
    {
        var buffer = Encoding.UTF8.GetBytes(json);
        response.StatusCode = 200;
        response.ContentType = "application/json; charset=utf-8";
        response.ContentLength64 = buffer.Length;
        response.OutputStream.Write(buffer, 0, buffer.Length);
    }

    public static void WriteText(HttpListenerResponse response, int statusCode, string text)
    {
        var payload = Encoding.UTF8.GetBytes(text ?? string.Empty);
        response.StatusCode = statusCode;
        response.ContentType = "text/plain; charset=utf-8";
        response.ContentLength64 = payload.Length;
        response.OutputStream.Write(payload, 0, payload.Length);
    }

    public static void WriteError(HttpListenerResponse response, int status, string message)
    {
        try
        {
            DinLogger.LogError($"[DinHttpListener Error] {status} {message}");
            WriteText(response, status, message);
        }
        catch
        {
            DinLogger.LogError("[DinHttpListener Error] Failed to write error response");
        }
    }

    public static void WriteCreated(HttpListenerResponse response, string? location = null)
    {
        response.StatusCode = 201;
        if (!string.IsNullOrEmpty(location))
            response.Headers["Location"] = location;
        response.ContentLength64 = 0;
    }

    public static void WriteNoContent(HttpListenerResponse response)
    {
        response.StatusCode = 204;
        response.ContentLength64 = 0;
    }
}
