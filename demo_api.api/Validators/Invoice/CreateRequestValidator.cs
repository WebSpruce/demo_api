using FluentValidation;
using demo_api.api.Data;
using demo_api.api.Endpoints.Invoice;
using Microsoft.EntityFrameworkCore;

namespace demo_api.api.Validators.Invoice;

public class CreateRequestValidator : AbstractValidator<CreateRequest>
{
    private readonly ApplicationDbContext _context;
    public CreateRequestValidator(ApplicationDbContext context)
    {
        _context = context;
        
        RuleFor(x => x.CompanyId)
            .NotNull().WithMessage("CompanyId cannot be null")
            .NotEqual(Guid.Empty).WithMessage("CompanyId cannot be empty")
            .MustAsync(async (companyId, cancellationToken) =>
            {
                return await _context.Companies.AnyAsync(v => v.Id == companyId, cancellationToken);
            })
            .WithMessage("The specified Company does not exist.");
        
        RuleFor(x => x.InvoiceNumber)
            .MinimumLength(2).WithMessage("InvoiceNumber must contain at least 2 characters");
        
        RuleFor(x => x.Status)
            .MinimumLength(2).WithMessage("Status must contain at least 2 characters");

        RuleFor(x => x.ClientId)
            .NotNull().WithMessage("ClientId cannot be null")
            .NotEqual(Guid.Empty).WithMessage("ClientId cannot be empty")
            .MustAsync(async (clientId, cancellationToken) =>
            {
                return await _context.Clients.AnyAsync(v => v.Id == clientId, cancellationToken);
            })
            .WithMessage("The specified Client does not exist.");
    }
}