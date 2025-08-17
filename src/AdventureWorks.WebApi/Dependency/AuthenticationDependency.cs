using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using AdventureWorks.WebApi.Infrastructure.Authentication;
using AdventureWorks.WebApi.Common.Models;

namespace AdventureWorks.WebApi.Dependency;

public static class AuthenticationDependency
{
    public static IServiceCollection AddAuthenticationServices(this IServiceCollection services, IConfiguration configuration)
    {
        // JWT Configuration
        var jwtSettings = configuration.GetSection("JwtSettings");
        var secretKey = jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey is not configured");
        var issuer = jwtSettings["Issuer"];
        var audience = jwtSettings["Audience"];

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
                ValidateIssuer = !string.IsNullOrEmpty(issuer),
                ValidIssuer = issuer,
                ValidateAudience = !string.IsNullOrEmpty(audience),
                ValidAudience = audience,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };
        });

        // Authorization Configuration with simple role-based policies
        services.AddAuthorization(options =>
        {
            options.AddPolicy(Policies.AdminOnly, policy =>
                policy.RequireRole(Roles.Admin));

            options.AddPolicy(Policies.ManagerOnly, policy =>
                policy.RequireRole(Roles.Manager, Roles.Admin));

            options.AddPolicy(Policies.ReadEmployees, policy =>
                policy.RequireRole(Roles.Admin, Roles.Manager, Roles.Employee));

            options.AddPolicy(Policies.CreateEmployees, policy =>
                policy.RequireRole(Roles.Admin, Roles.Manager));

            options.AddPolicy(Policies.UpdateEmployees, policy =>
                policy.RequireRole(Roles.Admin, Roles.Manager));

            options.AddPolicy(Policies.DeleteEmployees, policy =>
                policy.RequireRole(Roles.Admin));
        });

        // Register JWT service
        services.AddScoped<IJwtTokenService, JwtTokenService>();

        return services;
    }

    public static WebApplication UseAuthenticationMiddleware(this WebApplication app)
    {
        app.UseAuthentication();
        app.UseAuthorization();
        return app;
    }
}