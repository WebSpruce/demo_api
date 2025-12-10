using demo_api.api.Interfaces;
using demo_api.api.Interfaces.InvoiceItem;
using demo_api.models.Models;

namespace demo_api.api.Endpoints.InvoiceItem;

public class InvoiceItemEndpoints : IModule
{
    public void RegisterEndpoints(IEndpointRouteBuilder app)
    {
        var invoiceitems = app.MapGroup(ApiRoutes.InvoiceItems.GroupName)
            .WithTags("InvoiceItems");

        invoiceitems.MapPost("", async (
            CreateRequest request,
            IInvoiceItemRepository invoiceItemRepository,
            CancellationToken token
            ) =>
        {
            var result = await invoiceItemRepository.CreateAsync(request, token);

            if (result.IsCancelled)
                return Results.StatusCode(499);
            
            if (result.IsValidationFailure)
            {
                var problems = new HttpValidationProblemDetails(result.ValidationErrors)
                {
                    Status = StatusCodes.Status400BadRequest,
                    Title = "Validation failed",
                    Detail = "Validation errors occurred",
                    Instance = "/invoiceitems"
                };
                return Results.Problem(problems);
            }
            
            if (!result.IsSuccess)
                return Results.BadRequest(result.Errors);

            return Results.Ok(result.Value);
        }).RequireAuthorization(policy => policy.RequireRole(Roles.Admin));
        
        invoiceitems.MapGet("", async (
            Guid? id, 
            Guid? invoiceId, 
            Guid? productId , 
            decimal? unitPrice, 
            int? weight,
            int? quantity,
            int? page,
            int? pageSize,
            IInvoiceItemRepository invoiceItemRepository,
            CancellationToken token) =>
        {
            var request = new GetRequest(id, invoiceId, productId, unitPrice, weight, quantity, new PaginationRequest(page, pageSize));
            var result = await invoiceItemRepository.GetAsync(request, token);
            
            if (result.IsCancelled)
                return Results.StatusCode(499);

            if (result.IsValidationFailure)
            {
                var problem = new HttpValidationProblemDetails(result.ValidationErrors)
                {
                    Status = StatusCodes.Status400BadRequest,
                    Title = "Validation failed",
                    Detail = "Validation errors occurred",
                    Instance = $"/invoiceitems"
                };
                return Results.Problem(problem);
            }
            
            if (result.Errors?.Any() == true)
                return Results.NotFound(result.Errors?.FirstOrDefault());

            return Results.Ok(result.Value);
        }).RequireAuthorization();
        
        invoiceitems.MapPatch("/{id}", async (
            Guid id,
            UpdateRequest request,
            IInvoiceItemRepository invoiceItemRepository,
            CancellationToken token
        ) =>
        {
            var result = await invoiceItemRepository.UpdateAsync(id, request, token);

            if (result.IsCancelled)
                return Results.StatusCode(499);

            if (result.IsValidationFailure)
            {
                var problem = new HttpValidationProblemDetails(result.ValidationErrors)
                {
                    Status = StatusCodes.Status400BadRequest,
                    Title = "Validation failed",
                    Detail = "Validation errors occurred",
                    Instance = $"/invoiceitems/{id}"
                };
                return Results.Problem(problem);
            }

            if (result.Errors?.Any() == true)
                return Results.NotFound(result.Errors?.FirstOrDefault());

            return Results.Ok();
        }).RequireAuthorization(policy => policy.RequireRole(Roles.Admin, Roles.Manager));
        
        invoiceitems.MapDelete("/{id}", async (
            Guid id,
            IInvoiceItemRepository invoiceItemRepository,
            CancellationToken token
        ) =>
        {
            var result = await invoiceItemRepository.DeleteAsync(id, token);

            if (result.IsCancelled)
                return Results.StatusCode(499);

            if (result.Errors?.Any() == true)
                return Results.NotFound(result.Errors?.FirstOrDefault());

            return Results.Ok();
        }).RequireAuthorization(policy => policy.RequireRole(Roles.Admin));
    }
}