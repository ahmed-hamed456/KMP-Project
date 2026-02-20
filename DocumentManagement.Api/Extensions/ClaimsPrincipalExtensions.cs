using System.Security.Claims;

namespace DocumentManagement.Api.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static Guid GetUserId(this ClaimsPrincipal user)
    {
        var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value 
                          ?? user.FindFirst("sub")?.Value;
        
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            return Guid.Empty;
        }
        
        return userId;
    }
    
    public static string GetUserRole(this ClaimsPrincipal user)
    {
        return user.FindFirst(ClaimTypes.Role)?.Value 
               ?? user.FindFirst("role")?.Value 
               ?? "Viewer";
    }
    
    public static Guid? GetDepartmentId(this ClaimsPrincipal user)
    {
        var departmentIdClaim = user.FindFirst("departmentId")?.Value;
        
        if (string.IsNullOrEmpty(departmentIdClaim) || !Guid.TryParse(departmentIdClaim, out var departmentId))
        {
            return null;
        }
        
        return departmentId;
    }
}
