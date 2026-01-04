using Core.Interfaces;
using DAL.DataContext;
using DAL.Utilities;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.EntityFrameworkCore;
using System.Net;

var builder = WebApplication.CreateBuilder(args);

// Test connections immediately
Console.Clear();
Console.WriteLine("=== Application Starting ===");
ConnectionTester.TestAllConnections(builder.Configuration);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

#region Database Contexts
// Entity Framework
builder.Services.AddDbContext<EntityFrameworkDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sqlServerOptions => sqlServerOptions.MigrationsAssembly("DAL")
    ));

// Dapper
builder.Services.AddSingleton<DapperContext>();

// ADO.NET
builder.Services.AddSingleton<AdoNetContext>();
#endregion

#region Repository Registration
var dataAccessTechnology = builder.Configuration["DataAccess:Technology"] ?? "EntityFramework";

switch (dataAccessTechnology.ToLower())
{
    case "dapper":
        builder.Services.AddScoped<IEmployeeRepository, DAL.Repositories.Dapper.EmployeeRepository>();
        builder.Services.AddScoped<IProjectRepository, DAL.Repositories.Dapper.ProjectRepository>();
        builder.Services.AddScoped<ITicketRepository, DAL.Repositories.Dapper.TicketRepository>();
        break;

    case "ado.net":
    case "adonet":
        builder.Services.AddScoped<IEmployeeRepository, DAL.Repositories.AdoNet.EmployeeRepository>();
        builder.Services.AddScoped<IProjectRepository, DAL.Repositories.AdoNet.ProjectRepository>();
        builder.Services.AddScoped<ITicketRepository, DAL.Repositories.AdoNet.TicketRepository>();
        break;

    case "entityframework":
    default:
        builder.Services.AddScoped<IEmployeeRepository, DAL.Repositories.EntityFramework.EmployeeRepository>();
        builder.Services.AddScoped<IProjectRepository, DAL.Repositories.EntityFramework.ProjectRepository>();
        builder.Services.AddScoped<ITicketRepository, DAL.Repositories.EntityFramework.TicketRepository>();
        break;
}
#endregion

#region Health Check Services Registration (FIXED)
// Add this BEFORE the builder.Build() call
builder.Services.AddScoped<IHealthCheckService, Infrastructure.Health.HealthCheckService>();

// Register individual health checkers
builder.Services.AddScoped<Infrastructure.Health.IHealthChecker,
    Infrastructure.Health.HealthCheckers.EntityFrameworkHealthChecker>();
builder.Services.AddScoped<Infrastructure.Health.IHealthChecker,
    Infrastructure.Health.HealthCheckers.DapperHealthChecker>();
builder.Services.AddScoped<Infrastructure.Health.IHealthChecker,
    Infrastructure.Health.HealthCheckers.AdoNetHealthChecker>();

// Collect all health checkers
builder.Services.AddScoped<System.Collections.Generic.IEnumerable<Infrastructure.Health.IHealthChecker>>(provider =>
{
    var checkers = provider.GetServices<Infrastructure.Health.IHealthChecker>();
    return checkers;
});
#endregion

#region CORS Configuration
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        policy =>
        {
            policy.AllowAnyOrigin()
                   .AllowAnyMethod()
                   .AllowAnyHeader();
        });
});
#endregion

var app = builder.Build();

// Global exception handler
app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
        context.Response.ContentType = "application/json";

        var exceptionHandlerPathFeature = context.Features.Get<IExceptionHandlerPathFeature>();
        var exception = exceptionHandlerPathFeature?.Error;

        await context.Response.WriteAsJsonAsync(new
        {
            StatusCode = context.Response.StatusCode,
            Message = "An internal server error occurred.",
            Detail = exception?.Message
        });
    });
});

// Add health check endpoints
app.MapHealthChecks("/health/ready");
app.MapHealthChecks("/health/live");

// Add minimal API endpoint for quick health check
app.MapGet("/health/minimal", async (IHealthCheckService healthService) =>
{
    var isConnected = await healthService.IsDatabaseConnectedAsync();
    return Results.Json(new
    {
        status = isConnected ? "healthy" : "unhealthy",
        database = isConnected ? "connected" : "disconnected",
        timestamp = DateTime.UtcNow
    });
});

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthorization();
app.MapControllers();

app.Run();