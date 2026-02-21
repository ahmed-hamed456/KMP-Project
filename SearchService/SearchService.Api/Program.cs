using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.EntityFrameworkCore;
using SearchService.Api.Middleware;
using SearchService.Application.Interfaces;
using SearchService.Application.Validators;
using SearchService.Domain.Interfaces;
using SearchService.Infrastructure.BackgroundServices;
using SearchService.Infrastructure.Data;
using SearchService.Infrastructure.HttpClients;
using SearchService.Infrastructure.Repositories;
using AppSearchService = SearchService.Application.Services.SearchService;
using AppDocumentSyncService = SearchService.Application.Services.DocumentSyncService;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();

// Add FluentValidation
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddFluentValidationClientsideAdapters();
builder.Services.AddValidatorsFromAssemblyContaining<SearchRequestValidator>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Database Context
builder.Services.AddDbContext<SearchDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Repositories
builder.Services.AddScoped<ISearchRepository, SearchRepository>();

// Application Services
builder.Services.AddScoped<ISearchService, AppSearchService>();
builder.Services.AddScoped<IDocumentSyncService, AppDocumentSyncService>();

// HttpClient for DocumentManagement API
builder.Services.AddHttpClient<IDocumentManagementClient, DocumentManagementClient>(client =>
{
    var documentManagementUrl = builder.Configuration.GetValue<string>("DocumentManagementApi:BaseUrl")
        ?? "http://localhost:5297";
    client.BaseAddress = new Uri(documentManagementUrl);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
    
    // Add authentication headers for service-to-service communication
    // Using a system admin account for sync operations
    client.DefaultRequestHeaders.Add("X-User-Id", "00000000-0000-0000-0000-000000000001");
    client.DefaultRequestHeaders.Add("X-User-Role", "Admin");
    client.DefaultRequestHeaders.Add("X-Department-Id", "00000000-0000-0000-0000-000000000001");
});

// Background Service for Document Sync
builder.Services.AddHostedService<DocumentSyncBackgroundService>();

// CORS (optional - configure as needed)
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Mock Authentication
app.UseMiddleware<MockAuthenticationMiddleware>();

app.UseCors();

app.UseAuthorization();

app.MapControllers();

// Apply migrations on startup (for development)
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<SearchDbContext>();
    dbContext.Database.Migrate();
}

app.Run();
