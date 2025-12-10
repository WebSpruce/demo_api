using System.Security.Claims;
using System.Text;
using FluentValidation;
using demo_api.api.Data;
using demo_api.api.Endpoints.User;
using demo_api.api.Infrastructure;
using demo_api.api.Interfaces.User;
using demo_api.models.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace demo_api.api.Repositories.User;

public class UserRepository : IUserRepository
{
    private readonly ApplicationDbContext _dbContext;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IValidator<RegisterRequest> _validator;
    private readonly IValidator<LoginRequest> _validatorLogin;
    private readonly IValidator<UpdateRequest> _validatorUpdate;
    private readonly IValidator<GetRequest> _validatorGet;
    private readonly IOptions<JwtSettings> _jwtSettings;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public UserRepository(ApplicationDbContext dbContext, UserManager<ApplicationUser> userManager, 
        IValidator<RegisterRequest> validator, IValidator<LoginRequest> validatorLogin, IValidator<UpdateRequest> validatorUpdate, 
        IValidator<GetRequest> validatorGet, IOptions<JwtSettings> jwtSettings, IHttpContextAccessor httpContextAccessor)
    {
        _dbContext = dbContext;
        _userManager = userManager;
        _validator = validator;
        _validatorLogin = validatorLogin;
        _validatorUpdate = validatorUpdate;
        _validatorGet = validatorGet;
        _jwtSettings = jwtSettings;
        _httpContextAccessor = httpContextAccessor;
    }
    public async Task<Result> RegisterUserAsync(RegisterRequest request, CancellationToken token)
    {
        if (token.IsCancellationRequested)
            return Result.Cancelled();
            
        var validationResult = _validator.Validate(request);
        if (!validationResult.IsValid)
            return Result.ValidationFailure(new Dictionary<string, string[]>(validationResult.ToDictionary()));
            
        await using var transaction = await _dbContext.Database.BeginTransactionAsync(token);
        var user = new ApplicationUser
        {
            UserName = request.Email,
            Email = request.Email,
            FirstName = request.FirstName,
            LastName = request.LastName,
            CreatedAt = DateTime.UtcNow,
        };
            
        IdentityResult identityResult = await _userManager.CreateAsync(user, request.Password);
        if (!identityResult.Succeeded)
        {
            return Result.Failure(identityResult.Errors);
        }

        IdentityResult roleResult = await _userManager.AddToRoleAsync(user, request.Role);
        if (!roleResult.Succeeded)
        {
            return Result.Failure(roleResult.Errors);
        }

        await transaction.CommitAsync(token);
        
        return Result.Success();
    }

    public async Task<Result<UserLoginResponse>> LoginAsync(LoginRequest request, CancellationToken token)
    {
        if (token.IsCancellationRequested)
            return Result.Cancelled<UserLoginResponse>();

        var validationResult = _validatorLogin.Validate(request);
        if (!validationResult.IsValid)
            return Result.ValidationFailure<UserLoginResponse>(new Dictionary<string, string[]>(validationResult.ToDictionary()));

        var config = _jwtSettings.Value;
        var user = await _userManager.FindByEmailAsync(request.Email);

        if (user is null || !await _userManager.CheckPasswordAsync(user, request.Password))
        {
            List<string> errors = new List<string>() { "Unauthorized" };
            return Result.Failure<UserLoginResponse>(errors);
        }

        var roles = await _userManager.GetRolesAsync(user);

        string accessToken = JwtProvider.GenerateAccessToken(user, roles, config);
        RefreshToken refreshToken = new RefreshToken()
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            Token = JwtProvider.GenerateRefreshToken(),
            ExpiresOnUtc = DateTime.UtcNow.AddDays(7)
        };
        await _dbContext.RefreshTokens.AddAsync(refreshToken, token);
        await _dbContext.SaveChangesAsync(token);

        return Result.Success(new UserLoginResponse(accessToken, refreshToken.Token));
    }
    
    public async Task<Result<UserLoginResponse>> LoginWithRefreshTokenAsync(string rToken, CancellationToken token)
    {
        if (token.IsCancellationRequested)
            return Result.Cancelled<UserLoginResponse>();

        RefreshToken? refreshToken = await _dbContext.RefreshTokens
            .Include(r => r.User)
            .FirstOrDefaultAsync(r => r.Token == rToken, token);

        if (refreshToken is null || refreshToken.ExpiresOnUtc < DateTime.UtcNow)
        {
            List<string> errors = new List<string>() { "The refresh token has expired" };
            return Result.Failure<UserLoginResponse>(errors);
        }
        
        var roles = await _userManager.GetRolesAsync(refreshToken.User);
        var config = _jwtSettings.Value;
        string accessToken = JwtProvider.GenerateAccessToken(refreshToken.User, roles, config);

        refreshToken.Token = JwtProvider.GenerateRefreshToken();
        refreshToken.ExpiresOnUtc = DateTime.UtcNow.AddDays(7);

        await _dbContext.SaveChangesAsync(token);
        
        return Result.Success(new UserLoginResponse(accessToken, refreshToken.Token));
    }

    public async Task<Result> UpdateAsync(string id, UpdateRequest request, CancellationToken token)
    {
        if (token.IsCancellationRequested)
            return Result.Cancelled();

        var validationResult = await _validatorUpdate.ValidateAsync(request, token);
        if (!validationResult.IsValid)
            return Result.ValidationFailure(new Dictionary<string, string[]>(validationResult.ToDictionary()));

        var user = await _dbContext.Users.FirstOrDefaultAsync(x => x.Id == id && x.CompanyId == request.CompanyId, token);

        if (user is null)
            return Result.Failure(new List<object>() { "User not found or you do not have access" });

        if (request.FirstName is not null)
            user.FirstName = request.FirstName;
        if (request.LastName is not null)
            user.LastName = request.LastName;
        if (request.UserName is not null)
            user.UserName = request.UserName;
        if (request.PhoneNumber is not null)
            user.PhoneNumber = request.PhoneNumber;

        await _dbContext.SaveChangesAsync(token);
        
        return Result.Success();
    }

    public async Task<Result<PagedList<ApplicationUser>>> GetAllAsync(GetRequest request, CancellationToken token)
    {
        if (token.IsCancellationRequested)
            return Result.Cancelled<PagedList<ApplicationUser>>();

        var validationResult = await _validatorGet.ValidateAsync(request, token);
        if (!validationResult.IsValid)
            return Result.ValidationFailure<PagedList<ApplicationUser>>(new Dictionary<string, string[]>(validationResult.ToDictionary()));

        IQueryable<ApplicationUser> users = _userManager.Users;
        
        if (!string.IsNullOrEmpty(request.RoleName))
        {
            var roleUsers = await _userManager.GetUsersInRoleAsync(request.RoleName);
            var roleUserIds = roleUsers.Select(u => u.Id).ToList();
            users = users.Where(user => roleUserIds.Contains(user.Id));
        }
        
        if (request.ClientId.HasValue && request.ClientId != Guid.Empty)
            users = users.Where(user => user.Clients.Any(c => c.Id == request.ClientId));
        
        users = users
            .Where(user =>
                (request.CompanyId == null || user.CompanyId == request.CompanyId) &&
                (string.IsNullOrEmpty(request.Id) || user.Id == request.Id) &&
                (string.IsNullOrEmpty(request.UserName) || user.UserName.ToLower() == request.UserName.ToLower()) &&
                (string.IsNullOrEmpty(request.FirstName) || user.FirstName.ToLower() == request.FirstName.ToLower()) &&
                (string.IsNullOrEmpty(request.LastName) || user.LastName.ToLower() == request.LastName.ToLower()) &&
                (string.IsNullOrEmpty(request.Email) || user.Email.ToLower() == request.Email.ToLower()) &&
                (string.IsNullOrEmpty(request.PhoneNumber) || user.PhoneNumber == request.PhoneNumber) && 
                (!request.CreatedAt.HasValue || (request.CreatedAt.HasValue && user.CreatedAt.Date == request.CreatedAt.Value.Date) )
            )
            .AsQueryable();
        
        var result = await Pagination.Paginate(users, request.pagination?.pageNumber, request.pagination?.pageSize, token);

        return Result.Success(result);
    }
    
    public async Task<Result> DeleteAsync(string id, Guid companyId, CancellationToken token)
    {
        if (token.IsCancellationRequested)
            return Result.Cancelled();
        
        var user = await _dbContext.Users.FirstOrDefaultAsync(x => x.Id == id && x.CompanyId == companyId, token);

        if (user is null)
            return Result.Failure(new List<object>() { "User not found or you do not have access" });

        _dbContext.Users.Remove(user);

        await _dbContext.SaveChangesAsync(token);

        return Result.Success();
    }

    public async Task<Result> RemoveUserRefreshTokens(string userId, CancellationToken token)
    {
        if (userId != GetCurrentUserId())
            return Result.Failure(new List<object>() { "You can't do this" });
        
        await _dbContext.RefreshTokens.Where(r => r.UserId == userId)
            .ExecuteDeleteAsync(token);

        return Result.Success();
    }

    private string? GetCurrentUserId()
    {
        return _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
    }
}