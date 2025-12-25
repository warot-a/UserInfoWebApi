using System.Net;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using UserInfoWebApi.AuthenticationHandlers;
using UserInfoWebApi.Logger;
using UserInfoWebApi.Middleware;
using UserInfoWebApi.Model;
using UserInfoWebApi.ServiceFactory;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateLogger();

builder.Host.UseSerilog();


// Add services to the container.
builder.Services
    .AddControllers(s =>
    {
        s.Filters.Add(new ProducesResponseTypeAttribute(
            statusCode: (int)HttpStatusCode.UnprocessableEntity,
            type: typeof(CommonError))
        );
    })
    .AddNewtonsoftJson(o => { o.SerializerSettings.NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore; });
builder.Services.AddCors(options =>
{
    // Update this policy when we have concrete plan about security
    options.AddDefaultPolicy(policyBuilder =>
    {
        policyBuilder
            .AllowAnyOrigin()
            .AllowAnyHeader();
    });
});
builder.Services.AddUserInfoServiceDependencies();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddMemoryCache();
builder.Services.AddAuthentication()
    .AddScheme<ApplicationIdAuthenticationOptions, ApplicationIdAuthenticationHandler>
        (AuthenticationConstants.ApplicationIdAuthentication, (_) => { });

builder.Logging.ClearProviders();
builder.Services.AddHttpContextAccessor();
builder.Services.AddSingleton<MiddlewareContextAccessor>();
builder.Services.AddSingleton<ILoggerProvider, UserInfoLoggerProvider>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<GlobalExceptionMiddleware>();
app.UseMiddleware<CustomHeaderMiddleware>();
// app.UseExceptionHandler("/error");
app.UseHttpsRedirection();
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();