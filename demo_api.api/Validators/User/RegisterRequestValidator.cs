using FluentValidation;
using demo_api.api.Endpoints.User;

namespace demo_api.api.Validators.User;

internal class RegisterRequestValidator : AbstractValidator<RegisterRequest>
{
    public RegisterRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is empty")
            .EmailAddress().WithMessage("Email address is invalid");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is empty")
            .MinimumLength(6).WithMessage("Password must contain at least 6 characters")
            .Must(c => c.Any(char.IsUpper)).WithMessage("Password must contain an uppercase letter")
            .Must(c => c.Any(char.IsLower)).WithMessage("Password must contain an lowercase letter")
            .Must(c => c.Any(char.IsDigit)).WithMessage("Password must contain a digit")
            .Matches(@"[][""!@$%^&*(){}:;<>,.?/+_=|'~\\-]").WithMessage("Password must contain one or more special characters.")
            .Matches("^[^£# “”]*$").WithMessage("Password must not contain the following characters £ # “” or spaces.");
        
        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("FirstName is empty")
            .MinimumLength(2).WithMessage("FirstName must contain at least 2 characters");
        
        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("LastName is empty")
            .MinimumLength(2).WithMessage("LastName must contain at least 2 characters");
        
        RuleFor(x => x.Role)
            .NotEmpty().WithMessage("Role is empty");
    }
}