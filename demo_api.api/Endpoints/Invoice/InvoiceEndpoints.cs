using demo_api.api.Interfaces;
using demo_api.api.Interfaces.Invoice;
using demo_api.models.Models;

namespace demo_api.api.Endpoints.Invoice;

public class InvoiceEndpoints : IModule
{
    public void RegisterEndpoints(IEndpointRouteBuilder app)
    {
        var invoices = app.MapGroup(ApiRoutes.Invoices.GroupName)
            .WithTags("Invoices");

        invoices.MapPost("", async (
            CreateRequest request,
            IInvoiceRepository invoiceRepository,
            CancellationToken token
            ) =>
        {
            var result = await invoiceRepository.CreateAsync(request, token);

            if (result.IsCancelled)
                return Results.StatusCode(499);
            
            if (result.IsValidationFailure)
            {
                var problems = new HttpValidationProblemDetails(result.ValidationErrors)
                {
                    Status = StatusCodes.Status400BadRequest,
                    Title = "Validation failed",
                    Detail = "Validation errors occurred",
                    Instance = "/invoices"
                };
                return Results.Problem(problems);
            }
            
            if (!result.IsSuccess)
                return Results.BadRequest(result.Errors);

            return Results.Ok(result.Value);
        }).RequireAuthorization(policy => policy.RequireRole(Roles.Admin, Roles.Manager));
        
        invoices.MapGet("", async (
            Guid? id, 
            Guid companyId,
            string? invoiceNumber, 
            Guid? clientId, 
            string? status, 
            Guid? parentInvoiceId,
            int? page,
            int? pageSize,
            IInvoiceRepository invoiceRepository,
            CancellationToken token) =>
        {
            var request = new GetRequest(id, companyId, invoiceNumber, clientId, status, parentInvoiceId, new PaginationRequest(page, pageSize));
            var result = await invoiceRepository.GetAsync(request, token);
            
            if (result.IsCancelled)
                return Results.StatusCode(499);

            if (result.IsValidationFailure)
            {
                var problem = new HttpValidationProblemDetails(result.ValidationErrors)
                {
                    Status = StatusCodes.Status400BadRequest,
                    Title = "Validation failed",
                    Detail = "Validation errors occurred",
                    Instance = $"/invoices"
                };
                return Results.Problem(problem);
            }
            
            if (result.Errors?.Any() == true)
                return Results.NotFound(result.Errors?.FirstOrDefault());

            return Results.Ok(result.Value);
        }).RequireAuthorization();
        
        invoices.MapPatch("/{id}", async (
            Guid id,
            UpdateRequest request,
            IInvoiceRepository invoiceRepository,
            CancellationToken token
        ) =>
        {
            var result = await invoiceRepository.UpdateAsync(id, request, token);

            if (result.IsCancelled)
                return Results.StatusCode(499);

            if (result.IsValidationFailure)
            {
                var problem = new HttpValidationProblemDetails(result.ValidationErrors)
                {
                    Status = StatusCodes.Status400BadRequest,
                    Title = "Validation failed",
                    Detail = "Validation errors occurred",
                    Instance = $"/invoices/{id}"
                };
                return Results.Problem(problem);
            }

            if (result.Errors?.Any() == true)
                return Results.NotFound(result.Errors?.FirstOrDefault());

            return Results.Ok();
        }).RequireAuthorization(policy => policy.RequireRole(Roles.Admin, Roles.Manager));
        
        invoices.MapDelete("/{id}", async (
            Guid id,
            Guid companyId,
            IInvoiceRepository invoiceRepository,
            CancellationToken token
        ) =>
        {
            var result = await invoiceRepository.DeleteAsync(id, companyId, token);

            if (result.IsCancelled)
                return Results.StatusCode(499);

            if (result.Errors?.Any() == true)
                return Results.NotFound(result.Errors?.FirstOrDefault());

            return Results.Ok();
        }).RequireAuthorization(policy => policy.RequireRole(Roles.Admin, Roles.Manager));
    }
}