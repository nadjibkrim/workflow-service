using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using WorkflowService.Data;
using WorkflowService.Dto;
using WorkflowService.Models;
using WorkflowService.Services;
using WorkflowService.State;
using WorkflowService.Controllers;



var builder = WebApplication.CreateBuilder(args);

// Configure the database.  
// The service falls back to SQLite if no connection string is provided.  
// To use PostgreSQL, set the environment variable DB_CONNECTION_STRING to a valid
// Npgsql connection string.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? builder.Configuration["DB_CONNECTION_STRING"]
    ?? "Data Source=workflow.db";

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    if (connectionString.StartsWith("Host=") || connectionString.Contains("Username=") || connectionString.Contains("User ID="))
    {
        options.UseNpgsql(connectionString, x => x.MigrationsAssembly("WorkflowService"));
    }
    else
    {
        options.UseSqlite(connectionString);
    }
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Register state machine services
builder.Services.AddSingleton<IRuleRepository, InMemoryRuleRepository>();
builder.Services.AddSingleton<IStateMachineRepository, InMemoryStateMachineRepository>();

// Register application services
builder.Services.AddScoped<IWorkflowService, WorkflowService.Services.WorkflowService>();

var app = builder.Build();

// Automatically apply pending EF Core migrations on startup.  
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    db.Database.Migrate();
}

// Enable Swagger in all environments for API documentation
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Workflow Service API v1");
    c.RoutePrefix = "swagger";
});

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseRouting();
app.MapControllers();

app.Run();
