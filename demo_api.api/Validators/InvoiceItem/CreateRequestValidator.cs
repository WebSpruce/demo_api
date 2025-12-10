using FluentValidation;
using demo_api.api.Data;
using demo_api.api.Endpoints.InvoiceItem;
using Microsoft.EntityFrameworkCore;

namespace demo_api.api.Validators.InvoiceItem;

public class CreateRequestValidator : AbstractValidator<CreateRequest>
{
    private readonly ApplicationDbContext _context;

    public CreateRequestValidator(ApplicationDbContext context)
    {
        _context = context;

        RuleFor(x => x.InvoiceId)
            .NotEmpty().WithMessage("InvoiceId is required")
            .MustAsync(async (invoiceId, cancellationToken) =>
                await _context.Invoices.AnyAsync(i => i.Id == invoiceId, cancellationToken))
            .WithMessage("The specified Invoice does not exist");

        RuleFor(x => x.ProductId)
            .NotEmpty().WithMessage("ProductId is required")
            .MustAsync(async (productId, cancellationToken) =>
                await _context.Products.AnyAsync(p => p.Id == productId, cancellationToken))
            .WithMessage("The specified Product does not exist");

        RuleFor(x => x.UnitPrice)
            .GreaterThan(0).WithMessage("Unit Price must be greater than 0")
            .NotEmpty().WithMessage("Unit Price is required");

        RuleFor(x => x.Weight)
            .GreaterThan(0).WithMessage("Weight must be greater than 0")
            .NotEmpty().WithMessage("Weight is required");

        RuleFor(x => x.Quantity)
            .GreaterThan(0).WithMessage("Quantity must be greater than 0")
            .NotEmpty().WithMessage("Quantity is required");
    }
}