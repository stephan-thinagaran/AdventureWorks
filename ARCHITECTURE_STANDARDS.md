# AdventureWorks Project - Architecture Guide

## Table of Contents

### Architecture Standards
1. [Overview](#overview)
2. [Project Structure](#project-structure)
3. [Architectural Patterns](#architectural-patterns)
4. [Layer Architecture](#layer-architecture)
5. [Development Standards](#development-standards)
6. [Database Standards](#database-standards)
7. [API Standards](#api-standards)
8. [Dependency Injection](#dependency-injection)
9. [Error Handling](#error-handling)
10. [Logging Standards](#logging-standards)
11. [Caching Strategy](#caching-strategy)
12. [Validation Standards](#validation-standards)
13. [Container & Docker Standards](#container--docker-standards)
14. [Security Standards](#security-standards)
15. [Testing Standards](#testing-standards)
16. [Code Quality & Analysis](#code-quality--analysis)
17. [Deployment Standards](#deployment-standards)
18. [Business Domain Structure](#business-domain-structure)
19. [Configuration Management](#configuration-management)

### Architecture Decision Records
20. [Architecture Decision Records (ADR)](#architecture-decision-records-adr)
21. [Future Considerations](#future-considerations)

---

## Overview

The AdventureWorks project follows a Clean Architecture pattern with Domain-Driven Design principles, built using .NET 9 and Entity Framework Core. The architecture emphasizes separation of concerns, maintainability, and scalability.

### Core Technologies
- **.NET 9** - Primary framework
- **Entity Framework Core** - Data access layer
- **Carter** - Minimal API routing framework
- **FluentValidation** - Input validation
- **Serilog** - Structured logging
- **Docker** - Containerization
- **Redis** - Caching
- **Seq** - Log aggregation

---

## Project Structure

### Root Level Structure
```
AdventureWorks.sln              # Solution file
Directory.Build.props           # Global build properties
Directory.Packages.props        # Centralized package management
docker-compose.yml             # Multi-container orchestration
aspnetapp.pfx                  # Development certificate
nuget.config                   # NuGet configuration
README.md                      # Project documentation
src/                           # Source code directory
tests/                         # Test projects directory
```

### Application Structure
```
src/AdventureWorks.WebApi/
├── Common/                    # Cross-cutting concerns
│   ├── Extensions/           # Extension methods
│   └── Middleware/          # Custom middleware
├── Dependency/              # Dependency injection configuration
├── EndPoints/              # API endpoints organized by domain
│   └── HumanResources/
│       └── Employee/
├── Infrastructure/         # Infrastructure layer
│   ├── Database/          # Data access components
│   └── Messaging/         # CQRS messaging infrastructure
└── Properties/            # Application properties
```

---

## Architectural Patterns

### 1. Clean Architecture
- **Presentation Layer**: API endpoints and controllers
- **Application Layer**: Use cases, commands, queries
- **Infrastructure Layer**: Database, external services, messaging
- **Domain Layer**: Business entities and domain logic

### 2. CQRS (Command Query Responsibility Segregation)
- Separate models for read and write operations
- Commands for state-changing operations
- Queries for data retrieval operations

### 3. Repository Pattern
- Abstract data access through repository interfaces
- Generic `BaseRepository<T>` for common operations
- Specialized repositories for domain-specific operations

### 4. Mediator Pattern
- `IQueryDispatcher` for query handling
- `ICommandDispatcher` for command handling
- Decoupled request/response handling

---

## Layer Architecture

### EndPoints Layer
```csharp
// Standard endpoint structure
public class EndPoint : IEndPoint
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/resource/{id}", Handler);
    }
}
```

**Standards:**
- Use Carter's `IEndPoint` interface
- Group endpoints by domain (e.g., HumanResources, Sales)
- Follow RESTful naming conventions
- Include comprehensive validation

### Infrastructure Layer

#### Database Access
```csharp
// Repository implementation
public class SqlRepository<T> : BaseRepository<T> where T : class
{
    public SqlRepository(DbContext context) : base(context) {}
}
```

**Standards:**
- Inherit from `BaseRepository<T>`
- Use Entity Framework Core
- Implement query logging via interceptors
- Support both tracked and no-tracking queries

#### Messaging
```csharp
// Query handler implementation
public class QueryHandler : IQueryHandler<TQuery, TResponse>
{
    public async Task<TResponse?> Handle(TQuery query, CancellationToken cancellationToken)
    {
        // Implementation
    }
}
```

---

## Development Standards

### Target Framework
- **.NET 9** (configurable via `Directory.Build.props`)
- **C# Language Features**: Latest available
- **Nullable Reference Types**: Enabled

### Code Quality
```xml
<PropertyGroup>
    <TreatWarningsAsErrors>true</TreatWarningsAsErrors>
    <CodeAnalysisTreatWarningsAsErrors>true</CodeAnalysisTreatWarningsAsErrors>
    <EnforceCodeStyleInBuild>true</EnforceCodeStyleInBuild>
    <AnalysisLevel>latest-minimum</AnalysisLevel>
</PropertyGroup>
```

### Static Analysis
- **SonarAnalyzer.CSharp** for code quality analysis
- Warnings treated as errors in build process
- Code style enforcement during build

---

## Database Standards

### Entity Framework Configuration
```csharp
// DbContext registration
services.AddDbContext<AdventureWorksDbContext>((provider, options) =>
{
    var interceptor = provider.GetRequiredService<QueryLoggingInterceptor>();
    options.UseSqlServer(connectionString)
           .AddInterceptors(interceptor);
});
```

### Connection String Management
- Store in `appsettings.json` under `ConnectionStrings:AdventureWorks`
- Use configuration validation for missing connection strings
- Support for SQL Server with Trust Server Certificate

### Database Scaffolding
```bash
dotnet ef dbcontext scaffold 
  "connection_string" 
  Microsoft.EntityFrameworkCore.SqlServer 
  --output-dir Database/AdventureWorks/Entities 
  --context-dir Database/AdventureWorks/DBContext 
  --context AdventureWorksDbContext 
  --no-onconfiguring 
  --no-pluralize
```

---

## API Standards

### Endpoint Naming
- Use RESTful conventions: `/api/{domain}/{resource}`
- Example: `/api/employees/{employeeNo}`

### HTTP Methods
- **GET**: Data retrieval
- **POST**: Resource creation
- **PUT**: Full resource update
- **PATCH**: Partial resource update
- **DELETE**: Resource deletion

### Response Standards
```csharp
// Success responses
return Results.Ok(response);

// Not found responses
return Results.NotFound();

// Validation error responses
return Results.BadRequest(validationResult.Errors);
```

### Status Codes
- **200 OK**: Successful GET, PUT, PATCH
- **201 Created**: Successful POST
- **204 No Content**: Successful DELETE
- **400 Bad Request**: Validation errors
- **404 Not Found**: Resource not found
- **500 Internal Server Error**: Server errors

---

## Dependency Injection

### Registration Pattern
```csharp
// Organize dependencies by category
internal static IServiceCollection RegisterDependencies(this IServiceCollection services, IConfiguration configuration)
{
    services.RegisterCoreDependencies(configuration);
    services.RegisterInfraDependencies(configuration);
    services.RegisterUsecaseDependencies(configuration);
    return services;
}
```

### Service Lifetimes
- **Scoped**: DbContext, Repositories, Handlers
- **Singleton**: Configuration, Logging
- **Transient**: Validators, short-lived services

### Feature-Specific Registration
```csharp
// Each feature registers its own dependencies
public static IServiceCollection RegisterGetEmployeeDependencies(this IServiceCollection services)
{
    services.AddScoped<IQueryHandler<GetEmployeeQuery, GetEmployeeResponse>, GetEmployeeQueryHandler>();
    services.AddScoped<IValidator<GetEmployeeQuery>, Validator>();
    return services;
}
```

---

## Error Handling

### Global Exception Handler
```csharp
internal sealed class GlobalExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        logger.LogError(exception, "Unhandled exception occurred");
        
        var problemDetails = new ProblemDetails
        {
            Status = StatusCodes.Status500InternalServerError,
            Type = "https://datatracker.ietf.org/doc/html/rfc7231#section-6.6.1",
            Title = "Server failure"
        };
        
        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);
        return true;
    }
}
```

### Exception Handling Standards
- Use global exception handler for unhandled exceptions
- Log all exceptions with structured logging
- Return RFC 7807 Problem Details format
- Never expose internal exception details to clients

---

## Logging Standards

### Serilog Configuration
```csharp
builder.Host.UseSerilog((context, services, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration)
    .ReadFrom.Services(services)
    .Enrich.FromLogContext());
```

### Logging Levels
- **Information**: Normal application flow
- **Warning**: Unexpected situations that don't stop the application
- **Error**: Error events that don't stop the application
- **Critical**: Serious error events that may stop the application

### Structured Logging
```csharp
_logger.LogInformation("GetEmployeeQueryHandler method started");
_logger.LogError(exception, "Error occurred while processing {EmployeeId}", employeeId);
```

### Log Aggregation
- **Seq** for development environment log aggregation
- Accessible at `http://localhost:5341`
- Default admin password: `cloudiq@123`

---

## Caching Strategy

### Memory Cache Implementation
```csharp
// Cache key pattern
var cacheKey = $"{Constant.EmployeeIdCachePrefix}{query.NationalIDNumber}";

// Cache retrieval
if (_cache.TryGetValue(cacheKey, out string? cachedResponseJson))
{
    return JsonConvert.DeserializeObject<GetEmployeeResponse>(cachedResponseJson);
}

// Cache storage
var responseJson = JsonConvert.SerializeObject(response);
_cache.Set<string>(cacheKey, responseJson);
```

### Cache Standards
- Use meaningful cache key prefixes
- Implement cache-aside pattern
- Handle deserialization errors gracefully
- Consider cache expiration policies
- Use Redis for distributed caching in production

---

## Validation Standards

### FluentValidation Implementation
```csharp
public class Validator : AbstractValidator<GetEmployeeQuery>
{
    public Validator()
    {
        RuleFor(x => x.NationalIDNumber)
            .NotNull()
            .NotEmpty()
            .WithMessage("EmployeeId Cannot be null/empty");
    }
}
```

### Validation Standards
- Use FluentValidation for all input validation
- Register validators in dependency injection
- Validate before processing in endpoints
- Return structured validation errors
- Use meaningful error messages

---

## Container & Docker Standards

### Dockerfile Structure
```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
# Multi-stage build for optimized images
```

### Docker Compose Configuration
```yaml
services:
  webapi:
    image: adventureworks-webapi
    ports:
      - "5000:80"
      - "5001:443"
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
    depends_on:
      - redis
      - seq
```

### Container Standards
- Use multi-stage Docker builds
- Separate debug and release configurations
- Include health checks
- Use environment-specific configurations
- Implement proper secret management

---

## Security Standards

### HTTPS Configuration
- Generate development certificates: `dotnet dev-certs https -ep ./aspnetapp.pfx -p cloudiq@123`
- Configure Kestrel for HTTPS
- Use secure connection strings with TrustServerCertificate

### Authentication & Authorization
- Implement JWT token authentication (when required)
- Use role-based authorization
- Secure API endpoints appropriately
- Follow OWASP security guidelines

### Data Protection
- Never log sensitive information
- Use parameterized queries to prevent SQL injection
- Implement proper input validation
- Use HTTPS for all communications

---

## Testing Standards

### Test Structure
```
tests/
├── AdventureWorks.UnitTests/
├── AdventureWorks.IntegrationTests/
└── AdventureWorks.ArchitectureTests/
```

### Testing Categories
- **Unit Tests**: Individual component testing
- **Integration Tests**: Database and API testing
- **Architecture Tests**: Ensure architectural compliance

### Testing Guidelines
- Maintain high code coverage (>80%)
- Use AAA pattern (Arrange, Act, Assert)
- Mock external dependencies
- Test both success and failure scenarios

---

## Code Quality & Analysis

### Static Analysis Tools
- **SonarAnalyzer.CSharp**: Code quality and security
- **Microsoft CodeAnalysis**: Compiler-based analysis
- **EditorConfig**: Code style enforcement

### Quality Gates
- All warnings treated as errors
- Code analysis warnings treated as errors
- Code style enforced during build
- Minimum analysis level: `latest-minimum`

---

## Deployment Standards

### Environment Configuration
- **Development**: Docker Compose with local services
- **Production**: Container orchestration (Kubernetes/Docker Swarm)

### Build Pipeline
```bash
# Build
dotnet build AdventureWorks.sln

# Publish
dotnet publish AdventureWorks.sln

# Docker Build
docker-compose up --build -d
```

### Deployment Checklist
- [ ] Database migrations applied
- [ ] Environment variables configured
- [ ] Secrets properly managed
- [ ] Health checks implemented
- [ ] Monitoring configured
- [ ] Logging aggregation setup

---

## Business Domain Structure

### Supported Domains
Based on AdventureWorks database structure:
- **Person**: Customer and contact management
- **Production**: Product catalog and manufacturing
- **Purchasing**: Vendor and procurement management
- **Sales**: Order processing and customer relations
- **Warehouse**: Inventory and shipping management
- **Finance**: Financial transactions and accounting
- **HumanResources**: Employee and organizational management

### Domain Organization
- Each domain has its own endpoint folder structure
- Domain-specific entities and business logic
- Separate command/query handlers per domain
- Independent validation rules per domain

---

## Configuration Management

### appsettings.json Structure
```json
{
  "ConnectionStrings": {
    "AdventureWorks": "connection_string_here"
  },
  "Serilog": {
    "MinimumLevel": "Information",
    "WriteTo": [
      { "Name": "Console" },
      { "Name": "Seq", "Args": { "serverUrl": "http://localhost:5341" } }
    ]
  },
  "Redis": {
    "ConnectionString": "localhost:6379"
  }
}
```

### Environment-Specific Settings
- `appsettings.json`: Base configuration
- `appsettings.Development.json`: Development overrides
- `appsettings.Production.json`: Production overrides
- Use User Secrets for sensitive development data

---

This architecture standards document should be maintained and updated as the project evolves. Regular architecture reviews should be conducted to ensure compliance with these standards and to identify areas for improvement.

---

## Architecture Decision Records (ADR)

This section documents key architectural decisions for the AdventureWorks project, including context, choices, consequences, and status. It complements the Architecture Standards by recording why we made specific choices and how to evolve them.

- Status values: Proposed | Accepted | Deprecated | Superseded
- Scope: Repository-wide unless otherwise noted

### ADR-0001: Target .NET 9 (with .NET 8 fallback)
- **Status**: Accepted
- **Context**: We need latest runtime features and performance; repo includes comments to optionally target .NET 8.
- **Decision**: Target `net9.0` in `Directory.Build.props`; keep a commented `net8.0` pathway for compatibility.
- **Consequences**: Requires .NET 9 SDK in dev/build agents; Docker images should match the targeted runtime.

### ADR-0002: Minimal APIs with Carter Modules instead of MVC Controllers
- **Status**: Accepted
- **Context**: We prefer lightweight routing, discoverable endpoints, and feature-folder organization.
- **Decision**: Use Carter and implement endpoints via `IEndPoint : ICarterModule` modules (feature-based folders, e.g., `EndPoints/HumanResources/Employee`).
- **Consequences**: Leaner stack and fewer abstractions vs MVC; swagger and validation must be wired to minimal APIs.

### ADR-0003: CQRS with Lightweight Dispatchers
- **Status**: Accepted
- **Context**: We want clear separation between reads and writes and testable handlers.
- **Decision**: Define `IQueryHandler`, `ICommandHandler`, and dispatchers (`QueryDispatcher`, `CommandDispatcher`); feature endpoints call dispatchers.
- **Consequences**: More classes/boilerplate per use case; clearer responsibilities and easier testing.

### ADR-0004: EF Core + Repository Abstraction (SQL Server)
- **Status**: Accepted
- **Context**: SQL Server is the backing store; we need interception/logging and testable data access.
- **Decision**: Use EF Core `AdventureWorksDbContext` with SQL Server provider, a `QueryLoggingInterceptor`, and a generic repository (`BaseRepository<T>`, `SqlRepository<T>`).
- **Consequences**: Indirection over DbSet; common CRUD helps consistency; for complex queries, use EF Core LINQ directly within repositories/handlers.

### ADR-0005: Validation via FluentValidation
- **Status**: Accepted
- **Context**: Consistent, testable validation rules for request models.
- **Decision**: Add FluentValidation with assembly scanning; validate inputs at the edge (endpoints) before dispatching.
- **Consequences**: Separate validation classes per request; clear error messages returned as BadRequest.

### ADR-0006: Global Exception Handling with Problem Details
- **Status**: Accepted
- **Context**: We need consistent error responses and observability for failures.
- **Decision**: Implement `GlobalExceptionHandler` using `IExceptionHandler`; configure `ProblemDetails` in DI.
- **Consequences**: Uniform RFC 7807 responses; internal details not leaked; logging mandatory for diagnostics.

### ADR-0007: Structured Logging with Serilog + Seq
- **Status**: Accepted
- **Context**: Centralized, queryable logs locally and in containerized dev.
- **Decision**: Configure Serilog via `Host.UseSerilog` with enrichment; send logs to Console and Seq (docker-compose service `seq`).
- **Consequences**: Easy correlation and EF query logging; ensure PII not logged; Seq credentials/config managed via environment variables.

### ADR-0008: Caching Strategy – In-Memory First, Redis for Distributed
- **Status**: Accepted
- **Context**: Reduce read latency and database load; compose supports Redis.
- **Decision**: Use `IMemoryCache` for local caching in handlers; plan Redis for distributed caching scenarios (compose service `redis`).
- **Consequences**: Memory cache is per-instance; for multi-instance, switch to Redis-backed cache; define sensible expirations and invalidation rules per feature.

### ADR-0009: API Documentation with Swagger (Dev Only)
- **Status**: Accepted
- **Context**: Discoverability of endpoints for dev/test environments.
- **Decision**: Enable Swagger/SwaggerUI only when `ASPNETCORE_ENVIRONMENT=Development`; serve UI at root.
- **Consequences**: Reduce attack surface in production; dev convenience maintained; ensure OpenAPI descriptions kept accurate.

### ADR-0010: Containerization via Multi-Stage Docker + Docker Compose
- **Status**: Accepted
- **Context**: Local dev requires app + infra services (Seq, Redis) and HTTPS.
- **Decision**: Multi-stage Dockerfiles in WebApi; compose defines `webapi`, `redis`, and `seq` with port mappings and dev cert mounting.
- **Consequences**: Reproducible environment; ensure dev cert present; keep images updated and small; add health checks where appropriate.

### ADR-0011: Configuration & Secrets
- **Status**: Accepted
- **Context**: Secure management of connection strings and secrets across environments.
- **Decision**: Use appsettings hierarchy with `UserSecrets` in dev; mount secrets in containers via `${APPDATA}/Microsoft/UserSecrets` volume; never commit secrets.
- **Consequences**: Local dev convenience; production secrets managed by environment variables or secret stores (e.g., Key Vault) – out of scope for current repo.

### ADR-0012: Code Quality Gates and Analyzers
- **Status**: Accepted
- **Context**: Maintain high code quality and consistency across the repo.
- **Decision**: Enable `TreatWarningsAsErrors`, `CodeAnalysisTreatWarningsAsErrors`, `EnforceCodeStyleInBuild`; include `SonarAnalyzer.CSharp`.
- **Consequences**: Builds fail on warnings; higher upfront rigor; fewer defects reaching main branch.

### ADR-0013: Feature Folder Organization
- **Status**: Accepted
- **Context**: Improve cohesion and discoverability of feature code.
- **Decision**: Organize code by domain/feature (e.g., `EndPoints/HumanResources/Employee`), with co-located validator, handler, DTOs, and DI registration.
- **Consequences**: Less scattering of feature code; simpler navigation; follow consistent naming and folder patterns.

### ADR-0014: Endpoint Contract Conventions
- **Status**: Accepted
- **Context**: Consistent REST design and status handling across endpoints.
- **Decision**: Use `/api/{domain}/{resource}`; return `200 OK` for success with payload, `404 NotFound` for missing resources, `400 BadRequest` for validation failures; prefer ProblemDetails for errors.
- **Consequences**: Predictable API behavior; easier client integration.

### ADR-0015: Database Scaffolding Strategy (AdventureWorks)
- **Status**: Accepted
- **Context**: Generate EF Core entities from existing AdventureWorks schema.
- **Decision**: Use `dotnet ef dbcontext scaffold` with `--no-onconfiguring` and `--no-pluralize`; separate `Entities` and `DBContext` folders under `Infrastructure/Database/AdventureWorks`.
- **Consequences**: Clear separation of generated code; regeneration requires attention to customizations; prefer partial classes for extensions.

---

### How to Propose a New ADR
1. Create a new ADR section with the next sequential number and a clear title:
   - Format: `ADR-00NN: Short Decision Title`
   - Include: Status, Context, Decision, Consequences, (optional) Alternatives
2. Submit a PR that updates this file and references related code changes.
3. During review, discuss trade-offs; on merge, set Status to `Accepted` (or `Deprecated/Superseded`).

### ADR Template
```
### ADR-00NN: Title
- **Status**: Proposed | Accepted | Deprecated | Superseded
- **Context**: What problem are we solving? Constraints?
- **Decision**: What choice was made and why?
- **Consequences**: Positive/negative outcomes, operational impact
- **Alternatives**: (Optional) Why other options were not chosen
```

---

## Future Considerations
- **Distributed caching**: Promote Redis to default for multi-instance deployments.
- **AuthN/AuthZ**: Introduce JWT or federated identity; define role-based policies per endpoint.
- **Observability**: Add request tracing and correlation IDs; wire Application Insights if needed.
- **Health/Readiness Probes**: Add `/health` endpoints and container healthchecks.
- **Resilience**: Add Polly-based retries/circuit breakers around external dependencies.

---

**Maintainers**: Keep this architecture guide up to date as architectural decisions evolve to ensure clarity and consistency across the team.