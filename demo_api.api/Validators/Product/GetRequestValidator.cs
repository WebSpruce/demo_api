using FluentValidation;
using demo_api.api.Endpoints.Products;

namespace demo_api.api.Validators.Product;

public class GetRequestValidator : AbstractValidator<GetRequest>
{
    public GetRequestValidator()
    {
        RuleFor(x => x.Name)
            .MinimumLength(2).WithMessage("Name must contain at least 2 characters")
            .When(x => !string.IsNullOrWhiteSpace(x.Name));
        
        RuleFor(x => x.Weight)
            .GreaterThanOrEqualTo(0).WithMessage("Weight must be greater or equal to 0")
            .When(x => x.Weight != null);
        
        RuleFor(x => x.Price)
            .GreaterThanOrEqualTo(0).WithMessage("Price must be greater or equal to 0")
            .When(x => x.Price != null);
    }
}