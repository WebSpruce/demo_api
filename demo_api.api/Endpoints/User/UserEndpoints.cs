using demo_api.api.Interfaces;
using demo_api.api.Interfaces.User;
using demo_api.models.Models;
using Microsoft.AspNetCore.Mvc;

namespace demo_api.api.Endpoints.User;

public class UserEndpoints : IModule
{
    public void RegisterEndpoints(IEndpointRouteBuilder app)
    {
        var users = app
            .MapGroup(ApiRoutes.Users.GroupName)
            .WithTags("Users");

        users.MapPost("", async (
            RegisterRequest request, 
            IUserRepository userRepository,
            CancellationToken token) =>
        {
            var result = await userRepository.RegisterUserAsync(request, token);

            if (result.IsCancelled)
                return Results.StatusCode(499);

            if (result.IsValidationFailure)
            {
                var problems = new HttpValidationProblemDetails(result.ValidationErrors)
                {
                    Status = StatusCodes.Status400BadRequest,
                    Title = "Validation failed",
                    Detail = "Validation errors occurred",
                    Instance = "/users"
                };
                return Results.Problem(problems);
            }

            if (!result.IsSuccess)
                return Results.BadRequest(result.Errors);

            return Results.Ok();
        });
        
        users.MapPost("login", async (
            LoginRequest request,
            IUserRepository userRepository,
        CancellationToken token) =>
        {
            var result = await userRepository.LoginAsync(request, token);
            if (result.IsCancelled)
                return Results.StatusCode(499);
            
            if (result.IsValidationFailure)
            {
                var problems = new HttpValidationProblemDetails(result.ValidationErrors)
                {
                    Status = StatusCodes.Status400BadRequest,
                    Title = "Validation failed",
                    Detail = "Validation errors occurred",
                    Instance = "/users/login"
                };
                return Results.Problem(problems);
            }

            if (result.IsFailure)
            {
                if (result.Errors?.Any(e => e.ToString() == "Unauthorized") == true)
                    return Results.Unauthorized();
        
                return Results.BadRequest(new { errors = result.Errors });
            }

            return Results.Ok(result.Value);
        });
        
        users.MapPost("refresh-token", async (
            string refreshToken,
            IUserRepository userRepository,
            CancellationToken token) =>
        {
            var result = await userRepository.LoginWithRefreshTokenAsync(refreshToken, token);
            if (result.IsCancelled)
                return Results.StatusCode(499);

            if (result.IsFailure)
                return Results.BadRequest(new { errors = result.Errors });
            

            return Results.Ok(result.Value);
        });

        users.MapGet("token-check", (CancellationToken token) =>
        {
            if (token.IsCancellationRequested)
                return Results.StatusCode(499);
            
            return Results.Ok();
        }).RequireAuthorization();

        users.MapGet("", async (
            string? id,
            Guid? companyId,
            string? email,
            string? firstName,
            string? lastName,
            string? userName,
            string? phoneNumber,
            string? roleName,
            DateTime? createdAt,
            Guid? clientId,
            int? page,
            int? pageSize,
            [FromServices] IUserRepository userRepository,
            CancellationToken token
        ) =>
        {
            var request = new GetRequest(
                Id: id,
                CompanyId: companyId,
                Email: email,
                FirstName: firstName,
                LastName: lastName,
                UserName: userName,
                PhoneNumber: phoneNumber,
                RoleName: roleName,
                CreatedAt: createdAt,
                ClientId: clientId,
                new PaginationRequest(page, pageSize)
            );
            var result = await userRepository.GetAllAsync(request, token);

            if (result.IsCancelled)
                return Results.StatusCode(499);

            if (result.IsValidationFailure)
            {
                var problem = new HttpValidationProblemDetails(result.ValidationErrors)
                {
                    Status = StatusCodes.Status400BadRequest,
                    Title = "Validation failed",
                    Detail = "Validation errors occurred",
                    Instance = $"/users"
                };
                return Results.Problem(problem);
            }

            if (result.Errors?.Any() == true)
                return Results.NotFound(result.Errors?.FirstOrDefault());

            return Results.Ok(result.Value);
        }).RequireAuthorization();

        users.MapPatch("/{id}", async (
            string id,
            UpdateRequest request,
            IUserRepository userRepository,
            CancellationToken token
            ) =>
        {
            var result = await userRepository.UpdateAsync(id, request, token);

            if (result.IsCancelled)
                return Results.StatusCode(499);

            if (result.IsValidationFailure)
            {
                var problem = new HttpValidationProblemDetails(result.ValidationErrors)
                {
                    Status = StatusCodes.Status400BadRequest,
                    Title = "Validation failed",
                    Detail = "Validation errors occurred",
                    Instance = $"/users/{id}"
                };
                return Results.Problem(problem);
            }
            
            if (result.Errors?.Any() == true && result.Errors.Contains("There is no user with provided Id"))
                return Results.NotFound("There is no user with provided Id");

            return Results.Ok();
        }).RequireAuthorization();

        users.MapDelete("/{id}", async (
            string id,
            Guid companyId,
            IUserRepository userRepository,
            CancellationToken token
        ) =>
        {
            var result = await userRepository.DeleteAsync(id, companyId, token);

            if (result.IsCancelled)
                return Results.StatusCode(499);

            if (result.Errors?.Any() == true && result.Errors.Contains("There is no user with provided Id"))
                return Results.NotFound("There is no user with provided Id");
            
            return Results.Ok();
        }).RequireAuthorization(policy => policy.RequireRole(Roles.Admin));
        
        users.MapDelete("/{id}/refresh-tokens", async (
            string id,
            IUserRepository userRepository,
            CancellationToken token
        ) =>
        {
            var result = await userRepository.RemoveUserRefreshTokens(id, token);

            if (result.IsCancelled)
                return Results.StatusCode(499);
            
            if (result.IsFailure)
                return Results.BadRequest(new { errors = result.Errors });
            
            return Results.Ok();
        }).RequireAuthorization();
    }
}