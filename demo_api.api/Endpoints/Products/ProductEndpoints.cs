using demo_api.api.Interfaces;
using demo_api.api.Interfaces.Product;
using demo_api.models.Models;

namespace demo_api.api.Endpoints.Products;

public class ProductEndpoints : IModule
{
    public void RegisterEndpoints(IEndpointRouteBuilder app)
    {
        var products = app.MapGroup(ApiRoutes.Products.GroupName)
            .WithTags("Products");

       products.MapPost("", async (
            CreateRequest request, 
            IProductRepository productRepository,
            CancellationToken token) =>
        {
            var result = await productRepository.CreateAsync(request, token);

            if (result.IsCancelled)
                return Results.StatusCode(499);

            if (result.IsValidationFailure)
            {
                var problems = new HttpValidationProblemDetails(result.ValidationErrors)
                {
                    Status = StatusCodes.Status400BadRequest,
                    Title = "Validation failed",
                    Detail = "Validation errors occurred",
                    Instance = "/products"
                };
                return Results.Problem(problems);
            }

            if (!result.IsSuccess)
                return Results.BadRequest(result.Errors);

            return Results.Ok(result.Value);
        }).RequireAuthorization(policy => policy.RequireRole(Roles.Admin));

        products.MapGet("", async (
                Guid? id, 
                Guid companyId,
                string? name, 
                int? weight, 
                double? price,
                int? page,
                int? pageSize,
                IProductRepository productRepository,
                CancellationToken token) =>
        {
            var request = new GetRequest(id, companyId, name, weight, price, new PaginationRequest(page, pageSize));
            var result = await productRepository.GetAsync(request, token);
            
            if (result.IsCancelled)
                return Results.StatusCode(499);

            if (result.IsValidationFailure)
            {
                var problem = new HttpValidationProblemDetails(result.ValidationErrors)
                {
                    Status = StatusCodes.Status400BadRequest,
                    Title = "Validation failed",
                    Detail = "Validation errors occurred",
                    Instance = $"/products"
                };
                return Results.Problem(problem);
            }
            
            if (result.Errors?.Any() == true)
                return Results.NotFound(result.Errors?.FirstOrDefault());

            return Results.Ok(result.Value);
        });

        products.MapPatch("/{id}", async (
            Guid id,
            UpdateRequest request,
            IProductRepository productRepository,
            CancellationToken token
            ) =>
        {
            var result = await productRepository.UpdateAsync(id, request, token);

            if (result.IsCancelled)
                return Results.StatusCode(499);

            if (result.IsValidationFailure)
            {
                var problem = new HttpValidationProblemDetails(result.ValidationErrors)
                {
                    Status = StatusCodes.Status400BadRequest,
                    Title = "Validation failed",
                    Detail = "Validation errors occurred",
                    Instance = $"/products/{id}"
                };
                return Results.Problem(problem);
            }

            if (result.Errors?.Any() == true)
                return Results.NotFound(result.Errors?.FirstOrDefault());

            return Results.Ok();
        }).RequireAuthorization(policy => policy.RequireRole(Roles.Admin, Roles.Manager));
        
        products.MapDelete("/{id}", async (
            Guid id,
            Guid companyId,
            IProductRepository productRepository,
            CancellationToken token
        ) =>
        {
            var result = await productRepository.DeleteAsync(id, companyId, token);

            if (result.IsCancelled)
                return Results.StatusCode(499);

            if (result.Errors?.Any() == true)
                return Results.NotFound(result.Errors?.FirstOrDefault());

            return Results.Ok();
        }).RequireAuthorization(policy => policy.RequireRole(Roles.Admin));
    }
}