using demo_api.api.Endpoints.Client;
using demo_api.models.Models;

namespace demo_api.api.Interfaces.Client;

public interface IClientRepository
{
    Task<Result<models.Models.Client>> CreateAsync(CreateRequest request, CancellationToken token);
    Task<Result<PagedList<models.Models.Client>>>  GetAsync(GetRequest request, CancellationToken token);
    Task<Result> UpdateAsync(Guid id, UpdateRequest request, CancellationToken token);
    Task<Result> DeleteAsync(Guid id, Guid companyId, CancellationToken token);
}