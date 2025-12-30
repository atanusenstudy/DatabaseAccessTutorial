using Core.Interfaces;
using DAL.DataContext;
using DAL.Repositories.AdoNet;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Configuration;
using System.Net;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// WebApi/Program.cs

// Configuration for multiple database connections in appsettings.json
/*
"ConnectionStrings": {
  "EF_Core_TutorialDB_Connection": "Server=.;Database=TutorialDB;Trusted_Connection=True;TrustServerCertificate=True;",
  "Dapper_TutorialDB_Connection": "Server=.;Database=TutorialDB;Trusted_Connection=True;TrustServerCertificate=True;",
  "AdoNet_TutorialDB_Connection": "Server=.;Database=TutorialDB;Trusted_Connection=True;TrustServerCertificate=True;"
}
*/


/* In Package Manager Console with DAL as default project
    Add - Migration InitialCreate - Context EntityFrameworkDbContext
    Update-Database -Context EntityFrameworkDbContext
*/
#region Database Contexts
// Entity Framework
builder.Services.AddDbContext<EntityFrameworkDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sqlServerOptions => sqlServerOptions.MigrationsAssembly("DAL")
    ));
//builder.Services.AddDbContext<EntityFrameworkDbContext>(options =>
//    options.UseSqlServer(builder.Configuration.GetConnectionString("EF_Core_TutorialDB_Connection")));

// Dapper
builder.Services.AddSingleton<DapperContext>();

// ADO.NET
builder.Services.AddSingleton<AdoNetContext>();
#endregion

#region Repository Registration
// Choose ONE implementation based on your preference or configuration
// Option 1: Entity Framework
// builder.Services.AddScoped<IEmployeeRepository, DAL.Repositories.EntityFramework.EmployeeRepository>();
// builder.Services.AddScoped<IProjectRepository, DAL.Repositories.EntityFramework.ProjectRepository>();
// builder.Services.AddScoped<ITicketRepository, DAL.Repositories.EntityFramework.TicketRepository>();

// Option 2: Dapper
// builder.Services.AddScoped<IEmployeeRepository, DAL.Repositories.Dapper.EmployeeRepository>();
// builder.Services.AddScoped<IProjectRepository, DAL.Repositories.Dapper.ProjectRepository>();
// builder.Services.AddScoped<ITicketRepository, DAL.Repositories.Dapper.TicketRepository>();

// Option 3: ADO.NET
// builder.Services.AddScoped<IEmployeeRepository, DAL.Repositories.AdoNet.EmployeeRepository>();
// builder.Services.AddScoped<IProjectRepository, DAL.Repositories.AdoNet.ProjectRepository>();
// builder.Services.AddScoped<ITicketRepository, DAL.Repositories.AdoNet.TicketRepository>();

// OR: Use a factory pattern to switch between implementations
var dataAccessTechnology = builder.Configuration["DataAccess:Type"] ?? "EntityFramework";

switch (dataAccessTechnology.ToLower())
{
    case "dapper":
        builder.Services.AddScoped<IEmployeeRepository, EmployeeRepository>();
        builder.Services.AddScoped<IProjectRepository, ProjectRepository>();
        builder.Services.AddScoped<ITicketRepository, TicketRepository>();
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

#region CORS Configuration
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        builder =>
        {
            builder.AllowAnyOrigin()
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

        await context.Response.WriteAsync(new
        {
            StatusCode = context.Response.StatusCode,
            Message = "An internal server error occurred.",
            Detail = exception?.Message
        }.ToString());
    });
});


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
