using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using WorkflowService.Data;
using WorkflowService.Dto;
using WorkflowService.Models;
using WorkflowService.State;

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
        options.UseNpgsql(connectionString);
    }
    else
    {
        options.UseSqlite(connectionString);
    }
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Automatically apply pending EF Core migrations on startup.  
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    db.Database.Migrate();
}

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Create a new record with default state.
app.MapPost("/records", async (WorkflowRecordDto dto, ApplicationDbContext db) =>
{
    var now = DateTime.UtcNow;
    var record = new WorkflowRecord
    {
        Id = Guid.NewGuid(),
        Name = dto.Name,
        State = StateMachine.InitialState,
        CreatedAt = now,
        UpdatedAt = now
    };
    await db.Records.AddAsync(record);
    await db.SaveChangesAsync();
    return Results.Created($"/records/{record.Id}", record);
});

// Retrieve a single record by ID.
app.MapGet("/records/{id:guid}", async (Guid id, ApplicationDbContext db) =>
{
    var record = await db.Records.FindAsync(id);
    return record is not null ? Results.Ok(record) : Results.NotFound();
});

// List all records.
app.MapGet("/records", async (ApplicationDbContext db) => await db.Records.ToListAsync());


// Request a manual state transition.
app.MapPost("/records/{id:guid}/transition", async (Guid id, StateTransitionDto dto, ApplicationDbContext db) =>
{
    var record = await db.Records.FindAsync(id);
    if (record is null)
    {
        return Results.NotFound();
    }
    // Validate against allowed transitions.
    if (!StateMachine.CanTransition(record.State, dto.NextState))
    {
        return Results.BadRequest(new { message = $"Invalid transition from '{record.State}' to '{dto.NextState}'" });
    }
    record.State = dto.NextState;
    record.UpdatedAt = DateTime.UtcNow;
    await db.SaveChangesAsync();
    return Results.Ok(record);
});

// Request an automatic transition to the next state based on conditions.  
app.MapPost("/records/{id:guid}/next", async (Guid id, ApplicationDbContext db) =>
{
    var record = await db.Records.FindAsync(id);
    if (record is null)
    {
        return Results.NotFound();
    }
    var nextState = StateMachine.GetNextState(record);
    if (string.IsNullOrEmpty(nextState))
    {
        return Results.BadRequest(new { message = $"No eligible transition from '{record.State}' based on defined conditions." });
    }
    record.State = nextState;
    record.UpdatedAt = DateTime.UtcNow;
    await db.SaveChangesAsync();
    return Results.Ok(record);
});

app.Run();
