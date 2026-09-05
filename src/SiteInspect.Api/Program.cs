using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using SiteInspect.Api.Authentication;
using SiteInspect.Api.Diagnostics;
using SiteInspect.Api.ErrorHandling;
using SiteInspect.Api.Hosting;
using SiteInspect.Application;
using SiteInspect.Application.Common.Abstractions.Identity;
using SiteInspect.Infrastructure;
using SiteInspect.Infrastructure.Persistence.Initialization;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, services, loggerConfiguration) => loggerConfiguration
    .ReadFrom.Configuration(context.Configuration)
    .ReadFrom.Services(services)
    .Enrich.FromLogContext());

builder.Services.AddApplication();
builder.Configuration.ValidateProductionSettings(builder.Environment);
builder.Services.AddApiHosting();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddApiAuthentication(builder.Configuration, builder.Environment);
builder.Services.AddSiteInspectAuthorization();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUser, HttpCurrentUser>();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddOpenApi();
builder.Services
    .AddControllers(options =>
        options.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true)
    .AddJsonOptions(options =>
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = context => new BadRequestObjectResult(
        ApiResponseFactory.Failure<object?>(context.HttpContext, [ApiErrors.BadRequest]));
});

var app = builder.Build();

var initializeDatabase = app.Configuration.GetValue("Database:InitializeOnly", false);
var seedDemo = app.Configuration.GetValue("DemoData:SeedOnly", false);
if (initializeDatabase || seedDemo)
{
    if (initializeDatabase)
    {
        await app.Services.MigrateDatabaseAsync();
    }
    if (seedDemo)
    {
        await app.Services.SeedDemoDataAsync();
    }
    return;
}

if (app.Environment.IsDevelopment())
{
    await app.Services.MigrateDatabaseAsync();
    await app.Services.SeedDemoDataAsync();
}

app.UseMiddleware<CorrelationIdMiddleware>();
app.UseSerilogRequestLogging();
app.UseExceptionHandler(_ => { });
if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
    app.UseHttpsRedirection();
}
app.UseMiddleware<ResponseSecurityMiddleware>();
app.UseWhen(
    context => context.Request.Path.StartsWithSegments("/api"),
    apiPipeline => apiPipeline.UseStatusCodePages(async statusCodeContext =>
    {
        var httpContext = statusCodeContext.HttpContext;
        var error = ApiErrors.FromStatusCode(httpContext.Response.StatusCode);

        await ApiResponseWriter.WriteFailureAsync(
            httpContext,
            httpContext.Response.StatusCode,
            [error],
            httpContext.RequestAborted);
    }));
app.UseDefaultFiles();
app.UseStaticFiles();
app.UseRouting();
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapHealthChecks("/health/live", new HealthCheckOptions
{
    Predicate = _ => false,
});

app.MapHealthChecks("/health/ready");

app.MapControllers();

app.MapFallback("/api/{**path}", () => Results.NotFound());
app.MapFallbackToFile("index.html");

app.Run();

public partial class Program;
