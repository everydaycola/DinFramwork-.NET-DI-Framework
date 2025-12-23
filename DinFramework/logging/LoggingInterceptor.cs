using Castle.DynamicProxy;

namespace DinClassLibrary.Logging;

public class LoggingInterceptor : IInterceptor
{
    public void Intercept(IInvocation invocation)
    {
        DinLogger.LogDebug($"[INTERCEPTOR] Before calling {invocation.Method.Name} with parameters: {string.Join(", ", invocation.Arguments)}");
        
        try
        {
            invocation.Proceed();
            DinLogger.LogDetail($"[INTERCEPTOR] After calling {invocation.Method.Name}. Result: {invocation.ReturnValue}");
        }
        catch (Exception ex)
        {
            // warn because it could it caught later.
            DinLogger.LogWarn($"[INTERCEPTOR] Exception in {invocation.Method.Name}: {ex.Message}");
            throw;
        }
    }
}
