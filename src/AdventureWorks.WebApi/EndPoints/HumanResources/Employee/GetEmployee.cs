using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using AdventureWorks.WebApi.Infrastructure.Database;
using Entity = AdventureWorks.WebApi.Infrastructure.Database.AdventureWorks.Entities;
using AdventureWorks.WebApi.Infrastructure.Messaging;
using AdventureWorks.WebApi.Common.Models;

namespace AdventureWorks.WebApi.EndPoints.HumanResources.Employee;

public static class GetEmployee
{
    public class GetEmployeeQueryHandler : IQueryHandler<GetEmployeeQuery, GetEmployeeResponse>
    {
        private readonly IMemoryCache _cache;
        private readonly SqlRepository<Entity.Employee> _employeeRepo;
        private readonly ILogger<GetEmployeeQueryHandler> _logger;

        public GetEmployeeQueryHandler(IMemoryCache cache, SqlRepository<Entity.Employee> employeeRepo, ILogger<GetEmployeeQueryHandler> logger)
        {
            _cache = cache;
            _employeeRepo = employeeRepo;
            _logger = logger;
        }

        public async Task<GetEmployeeResponse?> Handle(GetEmployeeQuery query, CancellationToken cancellationToken)
        {
            _logger.LogInformation("GetEmployeeQueryHandler method started");

            // Return cached response if available
            var cacheKey = $"{Constant.EmployeeIdCachePrefix}{query.NationalIDNumber}";
            var cachedGetEmployeeResponse = GetEmployeeResponseFromCache(cacheKey);
            if(cachedGetEmployeeResponse != null)
            {
                return cachedGetEmployeeResponse;
            }

            var employees = await _employeeRepo.FindAsync(x => x.NationalIdnumber == query.NationalIDNumber, asNoTracking: true)
                                               .ConfigureAwait(false);
            var employee = employees.FirstOrDefault();
            if (employee == null)
                return null;

            // Create response and set cache
            var response = MapExtension.ToGetEmployeeResponse(employee);
            SetCache(cacheKey, response);

            _logger.LogInformation("GetEmployeeQueryHandler method completed");

            return response;
        }

        private GetEmployeeResponse? GetEmployeeResponseFromCache(string cacheKey)
        {
            if (_cache.TryGetValue(cacheKey, out string? cachedResponseJson) && !string.IsNullOrWhiteSpace(cachedResponseJson))
            {
                try
                {
                    return JsonConvert.DeserializeObject<GetEmployeeResponse>(cachedResponseJson);
                }
                catch (JsonSerializationException ex)
                {
                    // Log deserialization error, cache might be corrupt
                    Console.WriteLine($"Error deserializing cached employee data for key {cacheKey}: {ex.Message}");
                }
            }

            return null;
        }
        
        private void SetCache(string cacheKey, GetEmployeeResponse response)
        {
            var responseJson = JsonConvert.SerializeObject(response);
            _cache.Set<string>(cacheKey, responseJson);
        }
    }

    public class EndPoint : IEndPoint
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet("/api/employees/{employeeNo}", async ([FromRoute] string employeeNo, 
                                                                      [FromServices] IQueryDispatcher queryDispatcher, 
                                                                      [FromServices] IValidator<GetEmployeeQuery> validator, 
                                                                      CancellationToken cancellationToken) =>
            {
                var query = new GetEmployeeQuery { NationalIDNumber = employeeNo };
                var validationResult = await validator.ValidateAsync(query, cancellationToken);
                if(!validationResult.IsValid)
                    return Results.BadRequest(validationResult.Errors);

                var response = await queryDispatcher.Dispatch<GetEmployeeQuery, GetEmployeeResponse>(query, cancellationToken);
                return response is null ? Results.NotFound() : Results.Ok(response);
            })
            .RequireAuthorization(Policies.ReadEmployees); // Require permission to read employees
        }
    }

    public class Validator : AbstractValidator<GetEmployeeQuery>
    {
        public Validator()
        {
            RuleFor(x=> x.NationalIDNumber).NotNull().NotEmpty().WithMessage("EmployeeId Cannot be null/empty");
        }
    }

    public static IServiceCollection RegisterGetEmployeeDependencies(this IServiceCollection services)
    {
        services.AddScoped<IQueryHandler<GetEmployeeQuery, GetEmployeeResponse>, GetEmployeeQueryHandler>();
        services.AddScoped<IValidator<GetEmployeeQuery>, Validator>();
        return services;
    }

    public class GetEmployeeQuery : IQuery
    {
        public string NationalIDNumber { get; set; } = string.Empty;
    }

    public class GetEmployeeResponse : IQueryResult
    {
        public int BusinessEntityId { get; set; }

        public string NationalIdnumber { get; set; } = null!;

        public string LoginId { get; set; } = null!;

        public short? OrganizationLevel { get; set; }

        public string JobTitle { get; set; } = null!;

        public DateOnly BirthDate { get; set; }

        public string MaritalStatus { get; set; } = null!;

        public string Gender { get; set; } = null!;

        public DateOnly HireDate { get; set; }

        public bool SalariedFlag { get; set; }

        public short VacationHours { get; set; }

        public short SickLeaveHours { get; set; }

        public bool CurrentFlag { get; set; }

        public Guid Rowguid { get; set; }

        public DateTime ModifiedDate { get; set; }
    }

    public static class Constant
    {
        public const string EmployeeIdCachePrefix = "NationalIDNumber";
    }

    public static class MapExtension
    {
        public static GetEmployeeResponse ToGetEmployeeResponse(Entity.Employee emp) => new GetEmployeeResponse
        {
            BusinessEntityId = emp.BusinessEntityId,
            NationalIdnumber = emp.NationalIdnumber,
            LoginId = emp.LoginId,
            OrganizationLevel = emp.OrganizationLevel,
            JobTitle = emp.JobTitle,
            BirthDate = emp.BirthDate,
            MaritalStatus = emp.MaritalStatus,
            Gender = emp.Gender,
            HireDate = emp.HireDate,
            SalariedFlag = emp.SalariedFlag,
            VacationHours = emp.VacationHours,
            SickLeaveHours = emp.SickLeaveHours,
            CurrentFlag = emp.CurrentFlag,
            Rowguid = emp.Rowguid,
            ModifiedDate = emp.ModifiedDate
        };
    }
}
