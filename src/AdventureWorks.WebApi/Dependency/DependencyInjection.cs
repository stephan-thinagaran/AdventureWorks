using Carter;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using AdventureWorks.WebApi.Infrastructure.Database;
using AdventureWorks.WebApi.Infrastructure.Database.AdventureWorks;
using AdventureWorks.WebApi.Infrastructure.Messaging;
using AdventureWorks.WebApi.EndPoints.HumanResources.Employee;
using AdventureWorks.WebApi.Common.Middleware;
using AdventureWorks.WebApi.Common.Extensions;
using Serilog;

namespace AdventureWorks.WebApi.Dependency;

public static class DependencyInjection
{

    // App Builder Dependencies
    internal static WebApplicationBuilder CoreBuilder(this WebApplicationBuilder webApplicationBuilder, IConfiguration configuration)
    {
        webApplicationBuilder.ConfigureSerilog();
        webApplicationBuilder.Services.RegisterDependencies(configuration);
        webApplicationBuilder.AddFluentValidationEndpointFilter();
        return webApplicationBuilder;
    }

    // Configure Serilog
    internal static WebApplicationBuilder ConfigureSerilog(this WebApplicationBuilder builder)
    {
        builder.Host.UseSerilog((context, services, configuration) => configuration
            .ReadFrom.Configuration(context.Configuration)
            .ReadFrom.Services(services)
            .Enrich.FromLogContext());
        
        return builder;
    }

    internal static WebApplication MapServices(this WebApplication webApplication, IConfiguration configuration)
    {
        webApplication.MapCarter();
        webApplication.UseMiddleware<RequestContextLoggingMiddleware>();
        webApplication.UseRequestContextLogging();
        webApplication.UseExceptionHandler();
        webApplication.MapSwaggerEndpoints();
        return webApplication;
    }

    // All Dependecies
    internal static IServiceCollection RegisterDependencies(this IServiceCollection services, IConfiguration configuration)
    {
        services.RegisterCoreDependencies(configuration);
        services.RegisterInfraDependencies(configuration);
        services.RegisterUsecaseDependencies(configuration);
        return services;
    }

    // Core Dependencies
    internal static IServiceCollection RegisterCoreDependencies(this IServiceCollection services, IConfiguration configuration)
    {
        var assembly = typeof(Program).Assembly;
        services.AddEndpointsApiExplorer();
        services.AddCarter();
        services.AddValidatorsFromAssembly(assembly);
        services.AddSwaggerDefaults();

        services.AddExceptionHandler<GlobalExceptionHandler>();
        services.AddProblemDetails();

        services.AddScoped<IQueryDispatcher, QueryDispatcher>();
        services.AddScoped<ICommandDispatcher, CommandDispatcher>();

        //services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped(typeof(SqlRepository<>));

        return services;
    }

    internal static IServiceCollection RegisterInfraDependencies(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddMemoryCache();

        var connectionString = configuration.GetConnectionString("AdventureWorks");
        if (string.IsNullOrEmpty(connectionString))
        {
            // Handle missing connection string appropriately - throw exception or log error
            throw new InvalidOperationException("Connection string 'AdventureWorksConnection' not found.");
        }
        services.AddDbContext<AdventureWorksDbContext>(options =>
            options.UseSqlServer(connectionString)); // Or your specific provider

        services.AddScoped<DbContext>(provider => provider.GetRequiredService<AdventureWorksDbContext>());

        return services;
    }

    // Use Case Dependencies
    internal static IServiceCollection RegisterUsecaseDependencies(this IServiceCollection services, IConfiguration configuration)
    {
        services.RegisterGetEmployeeDependencies();
        return services;
    }
}
