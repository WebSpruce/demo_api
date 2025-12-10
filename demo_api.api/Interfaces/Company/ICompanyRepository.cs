using demo_api.api.Endpoints.Company;
using demo_api.models.Models;

namespace demo_api.api.Interfaces.Company;

public interface ICompanyRepository
{
    Task<Result<models.Models.Company>> CreateAsync(CreateRequest request, CancellationToken token);
    Task<Result> AddUserAsync(Guid companyId, string userId, CancellationToken token);
    Task<Result<PagedList<models.Models.Company>>> GetAsync(GetRequest request, CancellationToken token);
    Task<Result> UpdateAsync(Guid id, UpdateRequest request, CancellationToken token);
    Task<Result> DeleteAsync(Guid id, string userId, CancellationToken token);
    Task<Result> RemoveUserAsync(Guid id, string userId, CancellationToken token);
}