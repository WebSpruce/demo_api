using FluentValidation;
using demo_api.api.Data;
using demo_api.api.Endpoints.Company;
using demo_api.api.Infrastructure;
using demo_api.api.Interfaces.Company;
using demo_api.models.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace demo_api.api.Repositories.Company;

public class CompanyRepository : ICompanyRepository
{
    private readonly ApplicationDbContext _dbContext;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IValidator<CreateRequest> _validatorCreate;
    private readonly IValidator<GetRequest> _validatorGet;
    private readonly IValidator<UpdateRequest> _validatorUpdate;
    public CompanyRepository(ApplicationDbContext dbContext, IValidator<CreateRequest> validatorCreate, IValidator<GetRequest> validatorGet, IValidator<UpdateRequest> validatorUpdate,
        UserManager<ApplicationUser> userManager)
    {
        _dbContext = dbContext;
        _validatorCreate = validatorCreate;
        _validatorGet = validatorGet;
        _validatorUpdate = validatorUpdate;
        _userManager = userManager;
    }
    public async Task<Result<models.Models.Company>> CreateAsync(CreateRequest request, CancellationToken token)
    {
        if(token.IsCancellationRequested)
            return Result.Cancelled<models.Models.Company>();    
        
        var validationResult = await _validatorCreate.ValidateAsync(request, token);
        if(!validationResult.IsValid)
            return Result.ValidationFailure<models.Models.Company>(new Dictionary<string, string[]>(validationResult.ToDictionary()));
        
        var existingCompany = await _dbContext.Companies
            .FirstOrDefaultAsync(c => c.Slug == request.Slug.ToLower(), token);
        
        if (existingCompany != null)
            return Result.Failure<models.Models.Company>(new List<object> { "Company slug already exists" });
        
        var company = new models.Models.Company()
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Slug = request.Slug,
            CreatedAt = DateTime.UtcNow,
            OwnerId = request.OwnerId
        };
        
        var owner = await _userManager.FindByIdAsync(request.OwnerId);
        if (owner != null)
        {
            company.Users.Add(owner);
            owner.CompanyId = company.Id;
        }
        
        await _dbContext.Companies.AddAsync(company, token);
        await _dbContext.SaveChangesAsync(token);
        
        return Result.Success(company);
    }
    
    public async Task<Result> AddUserAsync(Guid companyId, string userId, CancellationToken token)
    {
        var companyExists = await _dbContext.Companies
            .AnyAsync(c => c.Id == companyId, token);
        
        if (!companyExists)
            return Result.Failure(new List<object> { "Company not found" });

        var user = await _userManager.FindByIdAsync(userId);
        if (user is null)
            return Result.Failure(new List<object> { "User not found" });

        if (user.CompanyId.HasValue)
            return Result.Failure(new List<object> { "User is already assigned to a company" });

        user.CompanyId = companyId;
    
        var result = await _userManager.UpdateAsync(user);
    
        if (!result.Succeeded)
            return Result.Failure(new List<object> { "Failed to add user to company" });

        return Result.Success();
    }

    public async Task<Result<PagedList<models.Models.Company>>> GetAsync(GetRequest request, CancellationToken token)
    {
        if(token.IsCancellationRequested)
            return Result.Cancelled<PagedList<models.Models.Company>>();    
        
        var validationResult = _validatorGet.Validate(request);
        if(!validationResult.IsValid)
            return Result.ValidationFailure<PagedList<models.Models.Company>>(new Dictionary<string, string[]>(validationResult.ToDictionary()));
        
        var companies = _dbContext.Companies
            .AsNoTracking()
            .Where(company => 
                (request.Id == null || company.Id == request.Id) &&
                (string.IsNullOrEmpty(request.Name) || company.Name.ToLower() == request.Name.ToLower()) &&
                (string.IsNullOrEmpty(request.Slug) || company.Slug.ToLower() == request.Slug.ToLower()) &&
                (string.IsNullOrEmpty(request.OwnerId) || company.OwnerId.ToLower() == request.OwnerId.ToLower())
            ).AsQueryable();

        var result = await Pagination.Paginate(companies, request.pagination?.pageNumber, request.pagination?.pageSize, token);

        return Result.Success(result);
    }

    public async Task<Result> UpdateAsync(Guid id, UpdateRequest request, CancellationToken token)
    {
        if(token.IsCancellationRequested)
            return Result.Cancelled();    
        
        var validationResult = await _validatorUpdate.ValidateAsync(request, token);
        if(!validationResult.IsValid)
            return Result.ValidationFailure(new Dictionary<string, string[]>(validationResult.ToDictionary()));
        
        var company = await _dbContext.Companies.FirstOrDefaultAsync(x => x.Id == id, token);
        if (company is null)
            return Result.Failure(new List<object>() { "There is no company with the provided Id" });

        if (request.Name is not null)
            company.Name = request.Name;
        if (request.Slug is not null)
            company.Slug = request.Slug;
        if (request.OwnerId is not null)
            company.OwnerId = request.OwnerId;
        
        await _dbContext.SaveChangesAsync(token);

        return Result.Success();
    }

    public async Task<Result> DeleteAsync(Guid id, string userId, CancellationToken token)
    {
        if(token.IsCancellationRequested)
            return Result.Cancelled();    
        
        var company = await _dbContext.Companies.FirstOrDefaultAsync(x => x.Id == id, token);
        if (company is null)
            return Result.Failure(new List<object>() { "There is no company with the provided Id" });
        
        if (company.OwnerId != userId)
            return Result.Failure(new List<object> { "You are not authorized to delete this company." });
        

        _dbContext.Companies.Remove(company);

        await _dbContext.SaveChangesAsync(token);

        return Result.Success();
    }

    public async Task<Result> RemoveUserAsync(Guid id, string userId, CancellationToken token)
    {
        var user = await _userManager.FindByIdAsync(userId);
    
        if (user is null)
            return Result.Failure(new List<object> { "User not found" });
        
        if (user.CompanyId != id)
            return Result.Failure(new List<object> { "User is not in this company" });

        user.CompanyId = null;

        var result = await _userManager.UpdateAsync(user);
    
        if (!result.Succeeded)
            return Result.Failure(new List<object> { "Failed to remove user" });

        return Result.Success();
    }
}