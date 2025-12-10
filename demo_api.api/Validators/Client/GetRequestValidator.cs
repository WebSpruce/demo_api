using FluentValidation;
using demo_api.api.Data;
using demo_api.api.Endpoints.Client;
using demo_api.models.Models;
using Microsoft.AspNetCore.Identity;

namespace demo_api.api.Validators.Client;

public class GetRequestValidator : AbstractValidator<GetRequest>
{
    private readonly UserManager<ApplicationUser> _userManager;
    public GetRequestValidator(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
        
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
        
        RuleFor(x => x.UserId)
            .MustAsync(async (userId, ct) =>
            {
                if (string.IsNullOrEmpty(userId))
                    return true;
                var user = await _userManager.FindByIdAsync(userId);
                return user != null;
            })
            .WithMessage("The specified User does not exist")
            .When(x => !string.IsNullOrEmpty(x.UserId));
    }
}