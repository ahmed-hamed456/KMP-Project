using System.Net;
using System.Text.Json;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using DocumentManagement.Domain.Exceptions;

namespace DocumentManagement.Api.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;
    private readonly IWebHostEnvironment _environment;
    
    public ExceptionHandlingMiddleware(
        RequestDelegate next, 
        ILogger<ExceptionHandlingMiddleware> logger,
        IWebHostEnvironment environment)
    {
        _next = next;
        _logger = logger;
        _environment = environment;
    }
    
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unhandled exception occurred: {Message}", ex.Message);
            await HandleExceptionAsync(context, ex);
        }
    }
    
    private Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";
        
        var (statusCode, message, errors) = exception switch
        {
            // FluentValidation exception
            ValidationException validationEx => (
                StatusCodes.Status422UnprocessableEntity,
                "One or more validation errors occurred.",
                validationEx.Errors.Select(e => new { e.PropertyName, e.ErrorMessage }).ToList<object>()
            ),
            
            // Custom exceptions
            NotFoundException notFoundEx => (
                StatusCodes.Status404NotFound,
                notFoundEx.Message,
                null
            ),
            
            ForbiddenException forbiddenEx => (
                StatusCodes.Status403Forbidden,
                forbiddenEx.Message,
                null
            ),
            
            // Built-in exceptions
            UnauthorizedAccessException => (
                StatusCodes.Status401Unauthorized,
                "Unauthorized access.",
                null
            ),
            
            KeyNotFoundException keyNotFoundEx => (
                StatusCodes.Status404NotFound,
                "The requested resource was not found.",
                null
            ),
            
            FileNotFoundException fileNotFoundEx => (
                StatusCodes.Status404NotFound,
                "The requested file was not found.",
                null
            ),
            
            InvalidOperationException invalidOpEx => (
                StatusCodes.Status400BadRequest,
                invalidOpEx.Message,
                null
            ),
            
            // ArgumentNullException must come before ArgumentException (inheritance)
            ArgumentNullException argNullEx => (
                StatusCodes.Status400BadRequest,
                $"Required parameter '{argNullEx.ParamName}' was null or missing.",
                null
            ),
            
            ArgumentException argEx => (
                StatusCodes.Status400BadRequest,
                argEx.Message,
                null
            ),
            
            DbUpdateException dbUpdateEx => (
                StatusCodes.Status409Conflict,
                "A database conflict occurred. The resource may be in use or have been modified.",
                null
            ),
            
            // Default for any other exception
            _ => (
                StatusCodes.Status500InternalServerError,
                "An error occurred while processing your request.",
                null
            )
        };
        
        context.Response.StatusCode = statusCode;
        
        var response = new
        {
            status = statusCode,
            message,
            detail = _environment.IsDevelopment() ? exception.Message : null,
            errors,
            stackTrace = _environment.IsDevelopment() ? exception.StackTrace : null
        };
        
        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = _environment.IsDevelopment()
        };
        
        return context.Response.WriteAsJsonAsync(response, options);
    }
}
