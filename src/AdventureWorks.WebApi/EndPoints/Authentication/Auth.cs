using Microsoft.AspNetCore.Mvc;
using AdventureWorks.WebApi.Infrastructure.Authentication;
using AdventureWorks.WebApi.Infrastructure.Messaging;
using AdventureWorks.WebApi.Common.Models;

namespace AdventureWorks.WebApi.EndPoints.Authentication;

public static class Auth
{
    public class EndPoint : IEndPoint
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost("/api/auth/login", ([FromBody] LoginRequest request, [FromServices] IJwtTokenService jwtService) =>
            {
                // Simple hardcoded authentication for demo purposes
                // In production, you would validate against your user store
                if (string.IsNullOrEmpty(request.Username) || string.IsNullOrEmpty(request.Password))
                    return Results.BadRequest("Username and password are required");

                // Demo users with different roles
                var roles = request.Username.ToLower() switch
                {
                    "admin" when request.Password == "admin123" => new[] { Roles.Admin },
                    "manager" when request.Password == "manager123" => new[] { Roles.Manager },
                    "employee" when request.Password == "employee123" => new[] { Roles.Employee },
                    "readonly" when request.Password == "readonly123" => new[] { Roles.ReadOnly },
                    _ => Array.Empty<string>()
                };

                if (roles.Length == 0)
                    return Results.Unauthorized();

                var token = jwtService.GenerateToken(request.Username, roles);

                return Results.Ok(new LoginResponse
                {
                    Token = token,
                    Username = request.Username,
                    Roles = roles.ToList(),
                    ExpiresAt = DateTime.UtcNow.AddMinutes(60)
                });
            });

            // Test endpoint to verify JWT authentication
            app.MapGet("/api/auth/profile", (HttpContext context) =>
            {
                var user = context.User;
                if (user.Identity?.IsAuthenticated != true)
                    return Results.Unauthorized();

                return Results.Ok(new
                {
                    Username = user.Identity?.Name,
                    Roles = user.Claims.Where(c => c.Type == "http://schemas.microsoft.com/ws/2008/06/identity/claims/role")
                                      .Select(c => c.Value).ToList(),
                    Claims = user.Claims.Select(c => new { c.Type, c.Value }).ToList()
                });
            })
            .RequireAuthorization();
        }
    }

    public class LoginRequest
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class LoginResponse
    {
        public string Token { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public List<string> Roles { get; set; } = new();
        public DateTime ExpiresAt { get; set; }
    }
}