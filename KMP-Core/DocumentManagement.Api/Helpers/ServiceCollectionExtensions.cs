using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using FluentValidation;
using FluentValidation.AspNetCore;
using DocumentManagement.Api.Middleware;
using DocumentManagement.Application.Interfaces;
using DocumentManagement.Application.Services;
using DocumentManagement.Application.Validators;
using DocumentManagement.Domain.Common;
using DocumentManagement.Domain.Interfaces;
using DocumentManagement.Infrastructure.Data;
using DocumentManagement.Infrastructure.Repositories;
using DocumentManagement.Infrastructure.Services;

namespace DocumentManagement.Api.Helpers;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDatabaseContext(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));
        
        return services;
    }
    
    public static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        services.AddScoped<IDocumentRepository, DocumentRepository>();
        
        return services;
    }
    
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<IFileStorageService>(sp => 
            new FileStorageService(configuration["FileStorage:BasePath"]));
        services.AddScoped<IDocumentService, DocumentService>();
        
        // Add FluentValidation
        services.AddValidatorsFromAssemblyContaining<CreateDocumentValidator>();
        services.AddFluentValidationAutoValidation();
        
        return services;
    }
    
    /// <summary>
    /// Adds mock authentication using custom header-based authentication handler.
    /// This is for development and testing purposes only.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddMockAuthentication(this IServiceCollection services)
    {
        services.AddAuthentication("Mock")
            .AddScheme<AuthenticationSchemeOptions, MockAuthenticationHandler>("Mock", null);
        
        return services;
    }
    
    public static IServiceCollection AddAuthorizationPolicies(this IServiceCollection services)
    {
        services.AddAuthorization(options =>
        {
            options.AddPolicy("AdminOnly", policy => policy.RequireRole(Roles.Admin));
            options.AddPolicy("EditorOrAdmin", policy => policy.RequireRole(Roles.Admin, Roles.Editor));
            options.AddPolicy("ViewerAccess", policy => policy.RequireAuthenticatedUser());
        });
        
        return services;
    }
}
