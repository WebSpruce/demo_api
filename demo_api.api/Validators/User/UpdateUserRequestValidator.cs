using FluentValidation;
using demo_api.api.Data;
using demo_api.api.Endpoints.User;
using Microsoft.EntityFrameworkCore;

namespace demo_api.api.Validators.User;

public class UpdateUserRequestValidator : AbstractValidator<UpdateRequest>
{
    private readonly ApplicationDbContext _context;
    public UpdateUserRequestValidator(ApplicationDbContext context)
    {
        _context = context;
        RuleFor(x => x.FirstName)
            .MinimumLength(2).WithMessage("FirstName must contain at least 2 characters")
            .When(x => !string.IsNullOrWhiteSpace(x.FirstName));
        
        RuleFor(x => x.LastName)
            .MinimumLength(2).WithMessage("LastName must contain at least 2 characters")
            .When(x => !string.IsNullOrWhiteSpace(x.LastName));
        
        RuleFor(x => x.UserName)
            .MinimumLength(2).WithMessage("UserName must contain at least 2 characters")
            .When(x => !string.IsNullOrWhiteSpace(x.UserName));

        RuleFor(x => x.PhoneNumber)
            .MinimumLength(9)
            .WithMessage("Phone number must contain at least 9 digits")
            .When(x => !string.IsNullOrWhiteSpace(x.PhoneNumber));
    }
}