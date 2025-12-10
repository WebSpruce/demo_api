using demo_api.api.Endpoints.User;
using demo_api.models.Models;

namespace demo_api.api.Interfaces.User;

public interface IUserRepository
{
    Task<Result> RegisterUserAsync(RegisterRequest request, CancellationToken token);
    Task<Result<UserLoginResponse>> LoginAsync(LoginRequest request, CancellationToken token);
    Task<Result<UserLoginResponse>> LoginWithRefreshTokenAsync(string rToken, CancellationToken token);
    Task<Result<PagedList<ApplicationUser>>> GetAllAsync(GetRequest request, CancellationToken token);
    Task<Result> UpdateAsync(string id, UpdateRequest request, CancellationToken token);
    Task<Result> DeleteAsync(string id, Guid companyId, CancellationToken token);
    Task<Result> RemoveUserRefreshTokens(string userId, CancellationToken token);
}