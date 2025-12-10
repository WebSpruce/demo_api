using FluentValidation;
using demo_api.api.Data;
using demo_api.api.Endpoints.Client;
using Microsoft.EntityFrameworkCore;

namespace demo_api.api.Validators.Client;

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
        
        RuleFor(x => x.City)
            .NotNull()
            .NotEmpty()
            .WithMessage("City cannot be empty");
        
        RuleFor(x => x.Address)
            .NotNull()
            .NotEmpty()
            .MinimumLength(2)
            .WithMessage("Address must contain at least 2 characters");
        
        RuleFor(x => x.PostCode)
            .NotNull()
            .NotEmpty()
            .MinimumLength(2)
            .WithMessage("Postcode must contain at least 2 characters");
        
        RuleFor(x => x.Location)
            .NotNull()
            .NotEmpty()
            .WithMessage("Location cannot be empty");
    }
}