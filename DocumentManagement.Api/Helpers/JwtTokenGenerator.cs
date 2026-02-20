using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace DocumentManagement.Api.Helpers;

public static class JwtTokenGenerator
{
    public static string GenerateToken(string userId, string role, string departmentId, IConfiguration configuration)
    {
        var jwtSettings = configuration.GetSection("Jwt");
        var secretKey = jwtSettings["SecretKey"] ?? "DefaultSecretKeyForDevelopment";
        var issuer = jwtSettings["Issuer"] ?? "DocumentManagementApi";
        var audience = jwtSettings["Audience"] ?? "DocumentManagementClient";
        
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
        
        var claims = new[]
        {
            new Claim("sub", userId),
            new Claim("role", role),
            new Claim("departmentId", departmentId),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.CreateVersion7().ToString())
        };
        
        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddDays(7),
            signingCredentials: credentials
        );
        
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
