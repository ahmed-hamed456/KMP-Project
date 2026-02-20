using Microsoft.EntityFrameworkCore;
using DocumentManagement.Api.Helpers;
using DocumentManagement.Infrastructure.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services using extension methods
builder.Services.AddDatabaseContext(builder.Configuration);
builder.Services.AddRepositories();
builder.Services.AddApplicationServices(builder.Configuration);
builder.Services.AddMockAuthentication();
builder.Services.AddAuthorizationPolicies();

// Add Controllers
builder.Services.AddControllers();

// Add Swagger services
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Seed database on startup
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<AppDbContext>();
        var logger = services.GetRequiredService<ILogger<Program>>();
        
        logger.LogInformation("Applying database migrations...");
        await context.Database.MigrateAsync();
        
        // Seed data if database is empty
        if (!context.Departments.Any())
        {
            logger.LogInformation("Seeding database...");
            var seeder = new DataSeeder(logger);
            await seeder.SeedAsync(context);
            logger.LogInformation("Database seeding completed successfully.");
        }
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while migrating or seeding the database.");
    }
}

// Exception handling middleware
app.UseCustomExceptionHandling();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Document Management API v1");
        options.RoutePrefix = string.Empty; // Set Swagger UI at root (https://localhost:7000/)
        options.DocumentTitle = "Document Management API - Swagger UI";
        options.DisplayRequestDuration();
    });
}

// Comment out HTTPS redirection for local testing
// app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();