using FluentValidation;
using demo_api.api.Endpoints.Company;

namespace demo_api.api.Validators.Company;

public class GetRequestValidator : AbstractValidator<GetRequest>
{
    public GetRequestValidator()
    {
        RuleFor(x => x.Name)
            .MaximumLength(100).When(x => !string.IsNullOrEmpty(x.Name));

        RuleFor(x => x.Slug)
            .MaximumLength(50).When(x => !string.IsNullOrEmpty(x.Slug));
    }
}