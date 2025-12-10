using FluentValidation;
using demo_api.api.Data;
using demo_api.api.Endpoints.InvoiceItem;
using Microsoft.EntityFrameworkCore;

namespace demo_api.api.Validators.InvoiceItem;

public class GetRequestValidator : AbstractValidator<GetRequest>
{
    private readonly ApplicationDbContext _context;

    public GetRequestValidator(ApplicationDbContext context)
    {
        _context = context;

        RuleFor(x => x.Id)
            .MustAsync(async (id, ct) =>
            {
                if (id == null || id == Guid.Empty)
                    return true;
                
                return await _context.InvoiceItems.AnyAsync(ii => ii.Id == id, ct);
            })
            .WithMessage("The specified InvoiceItem does not exist")
            .When(x => x.Id.HasValue && x.Id != Guid.Empty);

        RuleFor(x => x.InvoiceId)
            .MustAsync(async (invoiceId, ct) =>
            {
                if (invoiceId == null || invoiceId == Guid.Empty)
                    return true;
                
                return await _context.Invoices.AnyAsync(i => i.Id == invoiceId, ct);
            })
            .WithMessage("The specified Invoice does not exist")
            .When(x => x.InvoiceId.HasValue && x.InvoiceId != Guid.Empty);

        RuleFor(x => x.ProductId)
            .MustAsync(async (productId, ct) =>
            {
                if (productId == null || productId == Guid.Empty)
                    return true;
                
                return await _context.Products.AnyAsync(p => p.Id == productId, ct);
            })
            .WithMessage("The specified Product does not exist")
            .When(x => x.ProductId.HasValue && x.ProductId != Guid.Empty);

        RuleFor(x => x.UnitPrice)
            .GreaterThan(0).WithMessage("Unit Price must be greater than 0")
            .When(x => x.UnitPrice.HasValue);

        RuleFor(x => x.Weight)
            .GreaterThan(0).WithMessage("Weight must be greater than 0")
            .When(x => x.Weight.HasValue);

        RuleFor(x => x.Quantity)
            .GreaterThan(0).WithMessage("Quantity must be greater than 0")
            .When(x => x.Quantity.HasValue);
    }
}