using FluentValidation;
using demo_api.api.Endpoints.Client;

namespace demo_api.api.Validators.Client;

public class UpdateRequestValidator : AbstractValidator<UpdateRequest>
{
    public UpdateRequestValidator()
    {
        RuleFor(x => x.City)
            .MinimumLength(1)
            .WithMessage("City must contain at least 1 characters")
            .When(x => !string.IsNullOrEmpty(x.City));
        
        RuleFor(x => x.Address)
            .MinimumLength(2)
            .WithMessage("Address must contain at least 2 characters")
            .When(x => !string.IsNullOrEmpty(x.Address));
        
        RuleFor(x => x.PostCode)
            .MinimumLength(2)
            .WithMessage("Postcode must contain at least 2 characters")
            .When(x => !string.IsNullOrEmpty(x.PostCode));
        
        RuleFor(x => x.Location)
            .MinimumLength(5)
            .WithMessage("Location must contain at least 5 characters")
            .When(x => !string.IsNullOrEmpty(x.Location));
    }
}