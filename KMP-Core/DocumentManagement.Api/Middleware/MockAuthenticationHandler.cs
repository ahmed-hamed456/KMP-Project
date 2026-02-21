using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using DocumentManagement.Domain.Common;

namespace DocumentManagement.Api.Middleware;

/// <summary>
/// Custom authentication handler that reads user information from HTTP headers.
/// This is for development and testing purposes only.
/// </summary>
public class MockAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    private const string UserIdHeader = "X-User-Id";
    private const string UserRoleHeader = "X-User-Role";
    private const string DepartmentIdHeader = "X-Department-Id";

    public MockAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder)
        : base(options, logger, encoder)
    {
    }

    /// <summary>
    /// Authenticates the request by reading user information from HTTP headers.
    /// </summary>
    /// <returns>The authentication result.</returns>
    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        // Check if all required headers are present
        if (!Request.Headers.TryGetValue(UserIdHeader, out var userIdValue))
        {
            return Task.FromResult(AuthenticateResult.Fail($"Missing required header: {UserIdHeader}"));
        }

        if (!Request.Headers.TryGetValue(UserRoleHeader, out var userRoleValue))
        {
            return Task.FromResult(AuthenticateResult.Fail($"Missing required header: {UserRoleHeader}"));
        }

        if (!Request.Headers.TryGetValue(DepartmentIdHeader, out var departmentIdValue))
        {
            return Task.FromResult(AuthenticateResult.Fail($"Missing required header: {DepartmentIdHeader}"));
        }

        // Validate User ID is a valid GUID
        if (!Guid.TryParse(userIdValue.ToString(), out var userId))
        {
            return Task.FromResult(AuthenticateResult.Fail($"Invalid {UserIdHeader}: must be a valid GUID"));
        }

        // Validate Department ID is a valid GUID
        if (!Guid.TryParse(departmentIdValue.ToString(), out var departmentId))
        {
            return Task.FromResult(AuthenticateResult.Fail($"Invalid {DepartmentIdHeader}: must be a valid GUID"));
        }

        // Validate role is one of the allowed values
        var role = userRoleValue.ToString();
        if (role != Roles.Admin && role != Roles.Editor && role != Roles.Viewer)
        {
            return Task.FromResult(AuthenticateResult.Fail(
                $"Invalid {UserRoleHeader}: must be one of {Roles.Admin}, {Roles.Editor}, or {Roles.Viewer}"));
        }

        // Create claims
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            new Claim(ClaimTypes.Role, role),
            new Claim("departmentId", departmentId.ToString())
        };

        var identity = new ClaimsIdentity(claims, Scheme.Name);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, Scheme.Name);

        Logger.LogInformation(
            "Mock authentication succeeded for User ID: {UserId}, Role: {Role}, Department ID: {DepartmentId}",
            userId, role, departmentId);

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
