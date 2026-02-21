using System.Security.Claims;

namespace SearchService.Api.Middleware;

public class MockAuthenticationMiddleware
{
    private readonly RequestDelegate _next;

    public MockAuthenticationMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Extract user information from headers (matching DocumentManagement API pattern)
        var userId = context.Request.Headers["X-User-Id"].FirstOrDefault();
        var userRole = context.Request.Headers["X-User-Role"].FirstOrDefault();
        var departmentId = context.Request.Headers["X-Department-Id"].FirstOrDefault();

        if (!string.IsNullOrEmpty(userId))
        {
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, userId),
                new("userId", userId)
            };

            if (!string.IsNullOrEmpty(userRole))
            {
                claims.Add(new Claim(ClaimTypes.Role, userRole));
                claims.Add(new Claim("role", userRole));
            }

            if (!string.IsNullOrEmpty(departmentId))
            {
                claims.Add(new Claim("departmentId", departmentId));
            }

            var identity = new ClaimsIdentity(claims, "MockAuthentication");
            context.User = new ClaimsPrincipal(identity);
        }

        await _next(context);
    }
}
