using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace UserInfoWebApi.Controllers;

public class CustomHeaderMiddleware
{
    private readonly RequestDelegate _next;
    private const string TraceIdHeader = "x-amzn-trace-id";
    private const string ClientNameHeader = "x-clientname";
    private const string SdkHeader = "x-sdk-version";

    public CustomHeaderMiddleware(RequestDelegate next, ILogger<CustomHeaderMiddleware> logger)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext context)
    {
        context.Request.Headers.TryGetValue(ClientNameHeader, out var serviceFullName);
        context.Request.Headers.TryGetValue(SdkHeader, out var sdkVersion);
        context.Request.Headers.TryGetValue(TraceIdHeader, out var traceId);

        context.Response.Headers.Add(TraceIdHeader, traceId);
        context.Items["ServiceFullname"] = serviceFullName.ToString();
        context.Items["TraceId"] = traceId.ToString();
        context.Items["SdkVersion"] = sdkVersion.ToString();
        context.Items["Path"] = context.Request.Path.ToString();

        await _next(context);
    }
}