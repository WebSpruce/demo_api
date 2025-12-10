using FluentValidation;
using demo_api.api.Data;
using demo_api.api.Endpoints.Company;
using demo_api.models.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace demo_api.api.Validators.Company;

public class UpdateRequestValidator : AbstractValidator<UpdateRequest>
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public UpdateRequestValidator(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;

        RuleFor(x => x)
            .Must(x => !string.IsNullOrEmpty(x.Name) || !string.IsNullOrEmpty(x.Slug) || !string.IsNullOrEmpty(x.OwnerId))
            .WithMessage("At least one field (Name, Slug, or OwnerId) must be provided for update.");

        RuleFor(x => x.Name)
            .MaximumLength(100).WithMessage("Company Name must not exceed 100 characters")
            .When(x => !string.IsNullOrEmpty(x.Name));

        RuleFor(x => x.Slug)
            .MaximumLength(50).WithMessage("Slug must not exceed 50 characters")
            .Matches("^[a-z0-9-]+$").WithMessage("Slug can only contain lowercase letters, numbers, and hyphens")
            .MustAsync(async (slug, cancellationToken) =>
            {
                return !await _context.Companies.AnyAsync(c => c.Slug == slug, cancellationToken);
            })
            .WithMessage("This Slug is already taken.")
            .When(x => !string.IsNullOrEmpty(x.Slug));

        RuleFor(x => x.OwnerId)
            .MustAsync(async (ownerId, cancellationToken) =>
            {
                if (string.IsNullOrEmpty(ownerId)) return true;
                var user = await _userManager.FindByIdAsync(ownerId);
                return user != null;
            })
            .WithMessage("The specified Owner (User) does not exist.")
            .When(x => !string.IsNullOrEmpty(x.OwnerId));
    }
}