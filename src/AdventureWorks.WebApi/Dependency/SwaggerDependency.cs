using System;

namespace AdventureWorks.WebApi.Dependency;

internal static class SwaggerDependency
{
    internal static IServiceCollection AddSwaggerDefaults(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
            {
                Title = "WellnessPlan API",
                Version = "v1",
                Description = "WellnessPlan Web API"
            });
        });

        return services;
    }

    internal static WebApplication MapSwaggerEndpoints(this WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "WellnessPlan API V1");
                c.RoutePrefix = string.Empty; // Serve the Swagger UI at the app's root
            });
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "WellnessPlan");
            });
        }
        return app;
    }

}
