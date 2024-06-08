using System.Net;
using System.Text.Json;

namespace APBD_11_01.Middleware;

public class ErrorHandlingMiddleware(RequestDelegate @delegate, ILogger<ErrorHandlingMiddleware> log)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await @delegate(context);
        }
        catch (Exception e)
        {
            var message = e.Message;
            log.LogError(e, message);
            await HandleExceptionAsync(context, e);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var response = context.Response;
        response.ContentType = "application/json";

        var (status, message) = exception switch
        {
            _ => (HttpStatusCode.InternalServerError, "O, cholera! :((")
        };

        var errorMsg = JsonSerializer.Serialize(new { errorMessage = message });
        response.StatusCode = (int)status;
        return response.WriteAsync(errorMsg);
    }
}