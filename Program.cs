using System.Net;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using UserInfoWebApi.AuthenticationHandlers;
using UserInfoWebApi.Controllers;
using UserInfoWebApi.Logger;
using UserInfoWebApi.Model;
using UserInfoWebApi.ServiceFactory;

var builder = WebApplication.CreateBuilder(args);

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

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddMemoryCache();
builder.Services.AddAuthentication()
    .AddScheme<ApplicationIdAuthenticationOptions, ApplicationIdAuthenticationHandler>
        (AuthenticationConstants.ApplicationIdAuthentication, (options) => { });

builder.Logging.ClearProviders();
builder.Services.AddHttpContextAccessor();
builder.Services.AddSingleton<MiddlewareContextAccessor>();
builder.Services.AddSingleton<ILoggerProvider, UserInfoLoggerProvider>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<CustomHeaderMiddleware>();
app.UseExceptionHandler("/error");
app.UseHttpsRedirection();
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();