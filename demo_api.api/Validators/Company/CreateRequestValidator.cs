using FluentValidation;
using demo_api.api.Data;
using demo_api.api.Endpoints.Company;
using demo_api.models.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace demo_api.api.Validators.Company;

public class CreateRequestValidator : AbstractValidator<CreateRequest>
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public CreateRequestValidator(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Company Name is required")
            .MaximumLength(100).WithMessage("Company Name must not exceed 100 characters");

        RuleFor(x => x.Slug)
            .NotEmpty().WithMessage("Slug is required")
            .MaximumLength(50).WithMessage("Slug must not exceed 50 characters")
            // regex ensures URL-friendly slug (alphanumeric and hyphens only)
            .Matches("^[a-z0-9-]+$").WithMessage("Slug can only contain lowercase letters, numbers, and hyphens")
            .MustAsync(async (slug, cancellationToken) =>
            {
                // ensure slug is unique across the system
                return !await _context.Companies.AnyAsync(c => c.Slug == slug, cancellationToken);
            }).WithMessage("This Slug is already taken.");

        RuleFor(x => x.OwnerId)
            .NotEmpty().WithMessage("OwnerId is required")
            .MustAsync(async (ownerId, cancellationToken) =>
            {
                var user = await _userManager.FindByIdAsync(ownerId);
                return user != null;
            }).WithMessage("The specified Owner (User) does not exist.");
    }
}