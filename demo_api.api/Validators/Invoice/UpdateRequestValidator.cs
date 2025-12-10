using FluentValidation;
using demo_api.api.Data;
using demo_api.api.Endpoints.Invoice;
using Microsoft.EntityFrameworkCore;

namespace demo_api.api.Validators.Invoice;

public class UpdateRequestValidator : AbstractValidator<UpdateRequest>
{
    private readonly ApplicationDbContext _context;
    public UpdateRequestValidator(ApplicationDbContext context)
    {
        _context = context;
        
        RuleFor(x => x.InvoiceNumber)
            .MinimumLength(2).WithMessage("InvoiceNumber must contain at least 2 characters")
            .When(x => !string.IsNullOrWhiteSpace(x.InvoiceNumber));
        
        RuleFor(x => x.Status)
            .MinimumLength(2).WithMessage("Status must contain at least 2 characters")
            .When(x => !string.IsNullOrWhiteSpace(x.Status));

        RuleFor(x => x.ClientId)
            .MustAsync(async (clientId, cancellationToken) =>
            {
                return await _context.Clients.AnyAsync(v => v.Id == clientId, cancellationToken);
            })
            .When(x => x.ClientId != null)
            .WithMessage("The specified Client does not exist.");;
    }
}