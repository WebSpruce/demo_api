using demo_api.api.Interfaces;
using demo_api.api.Interfaces.Client;
using demo_api.models.Models;

namespace demo_api.api.Endpoints.Client;

public class ClientEndpoints : IModule
{
    public void RegisterEndpoints(IEndpointRouteBuilder app)
    {
        var clients = app.MapGroup(ApiRoutes.Clients.GroupName)
            .WithTags("Clients");
        
        clients.MapPost("", async (
            CreateRequest request, 
            IClientRepository clientRepository,
            CancellationToken token) =>
        {
            var result = await clientRepository.CreateAsync(request, token);

            if (result.IsCancelled)
                return Results.StatusCode(499);

            if (result.IsValidationFailure)
            {
                var problems = new HttpValidationProblemDetails(result.ValidationErrors)
                {
                    Status = StatusCodes.Status400BadRequest,
                    Title = "Validation failed",
                    Detail = "Validation errors occurred",
                    Instance = "/clients"
                };
                return Results.Problem(problems);
            }

            if (!result.IsSuccess)
                return Results.BadRequest(result.Errors);

            return Results.Ok(result.Value);
        }).RequireAuthorization(policy => policy.RequireRole(Roles.Admin));

        clients.MapGet("", async (
                Guid? id, 
                Guid companyId,
                string? city, 
                string? address, 
                string? postcode,
                string? location,
                string? userId,
                int? page,
                int? pageSize,
                IClientRepository clientRepository,
                CancellationToken token) =>
        {
            var request = new GetRequest(id, companyId, city, address, postcode, location, userId, new PaginationRequest(page, pageSize));
            var result = await clientRepository.GetAsync(request, token);
            
            if (result.IsCancelled)
                return Results.StatusCode(499);

            if (result.IsValidationFailure)
            {
                var problem = new HttpValidationProblemDetails(result.ValidationErrors)
                {
                    Status = StatusCodes.Status400BadRequest,
                    Title = "Validation failed",
                    Detail = "Validation errors occurred",
                    Instance = $"/clients"
                };
                return Results.Problem(problem);
            }
            
            if (result.Errors?.Any() == true)
                return Results.NotFound(result.Errors?.FirstOrDefault());

            return Results.Ok(result.Value);
        }).RequireAuthorization();

        clients.MapPatch("/{id}", async (
            Guid id,
            UpdateRequest request,
            IClientRepository clientRepository,
            CancellationToken token
            ) =>
        {
            var result = await clientRepository.UpdateAsync(id, request, token);

            if (result.IsCancelled)
                return Results.StatusCode(499);

            if (result.IsValidationFailure)
            {
                var problem = new HttpValidationProblemDetails(result.ValidationErrors)
                {
                    Status = StatusCodes.Status400BadRequest,
                    Title = "Validation failed",
                    Detail = "Validation errors occurred",
                    Instance = $"/clients/{id}"
                };
                return Results.Problem(problem);
            }

            if (result.Errors?.Any() == true)
                return Results.NotFound(result.Errors?.FirstOrDefault());

            return Results.Ok();
        }).RequireAuthorization(policy => policy.RequireRole(Roles.Admin, Roles.Manager));
        
        clients.MapDelete("/{id}", async (
            Guid id,
            Guid companyId,
            IClientRepository clientRepository,
            CancellationToken token
        ) =>
        {
            var result = await clientRepository.DeleteAsync(id, companyId, token);

            if (result.IsCancelled)
                return Results.StatusCode(499);

            if (result.Errors?.Any() == true)
                return Results.NotFound(result.Errors?.FirstOrDefault());

            return Results.Ok();
        }).RequireAuthorization(policy => policy.RequireRole(Roles.Admin));
    }
}