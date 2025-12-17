using System.Net;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace UserInfoWebApi.Controllers;

[ApiController]
[Route("[controller]")]
[ApiExplorerSettings(IgnoreApi = true)]
public class ErrorController : ControllerBase
{
    private readonly ILogger<ErrorController> _logger;

    public ErrorController(ILogger<ErrorController> logger)
    {
        _logger = logger;
    }

    public ActionResult Error()
    {
        var exceptionHandlerPathFeature = HttpContext.Features.Get<IExceptionHandlerPathFeature>();
        if (exceptionHandlerPathFeature?.Error == null)
        {
            return StatusCode((int)HttpStatusCode.InternalServerError, new { Message = "Unknown error" });
        }

        var ex = exceptionHandlerPathFeature.Error;

        _logger.LogError($"Internal error: {ex}");

        return StatusCode((int)HttpStatusCode.InternalServerError, new { ex.Message });
    }
}