using System.Net;

namespace DinClassLibrary;

public class DinHttpListener
{
    public DinHttpListener()
    {
        var listener = new HttpListener();
        listener.Prefixes.Add("http://localhost:9999/");
        listener.Start();
        
        var context = listener.GetContext();
        var request = context.Request;
        var response = context.Response;
        
        Console.WriteLine(request.HttpMethod);
        Console.WriteLine(request.UserAgent);
    }
}