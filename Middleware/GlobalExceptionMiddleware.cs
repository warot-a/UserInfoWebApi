using System.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using UserInfoWebApi.Model;

namespace UserInfoWebApi.Middleware;

public class GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        logger.LogError(exception, "An unexpected error occurred during request processing.");

        context.Response.ContentType = "application/json";

        var response = new CommonError
        {
            Message = "An internal server error occurred. Please try again later."
        };

        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

        if (exception is ArgumentException || exception is ArgumentNullException)
        {
            context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            response.Message = "Invalid request arguments.";
        }

        // We log the specific exception above but return a generic message to the client 
        // to avoid exposing stack traces or internal details (Redis, OpenSearch, etc.)
        var json = JsonConvert.SerializeObject(response, new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Ignore
        });

        await context.Response.WriteAsync(json);
    }
}