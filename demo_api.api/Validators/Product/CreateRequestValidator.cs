using FluentValidation;
using demo_api.api.Data;
using demo_api.api.Endpoints.Products;
using Microsoft.EntityFrameworkCore;

namespace demo_api.api.Validators.Product;

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
        
        RuleFor(x => x.Name)
            .MinimumLength(2).WithMessage("Name must contain at least 2 characters");

        RuleFor(x => x.Weight)
            .GreaterThanOrEqualTo(0).WithMessage("Weight must be greater or equal to 0");

        RuleFor(x => x.Price)
            .GreaterThanOrEqualTo(0).WithMessage("Price must be greater or equal to 0");
    }
}