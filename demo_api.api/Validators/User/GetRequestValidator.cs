using FluentValidation;
using demo_api.api.Data;
using demo_api.api.Endpoints.User;
using Microsoft.EntityFrameworkCore;

namespace demo_api.api.Validators.User;

public class GetRequestValidator : AbstractValidator<GetRequest>
{
    private readonly ApplicationDbContext _context;
    public GetRequestValidator(ApplicationDbContext context)
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
        
        RuleFor(x => x.RoleName)
            .MinimumLength(2).WithMessage("RoleName must contain at least 2 characters")
            .When(x => !string.IsNullOrWhiteSpace(x.RoleName));

        RuleFor(x => x.PhoneNumber)
            .MinimumLength(9)
            .WithMessage("Phone number must contain at least 9 digits")
            .When(x => !string.IsNullOrWhiteSpace(x.PhoneNumber));
        
        RuleFor(x => x.ClientId)
            .MustAsync(async (clientId, ct) =>
            {
                if (!clientId.HasValue || clientId == Guid.Empty)
                    return true;
                return await _context.Clients.AnyAsync(c => c.Id == clientId, ct);
            })
            .WithMessage("The specified Client does not exist")
            .When(x => x.ClientId.HasValue && x.ClientId != Guid.Empty);
    }
}