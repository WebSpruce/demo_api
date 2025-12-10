using demo_api.api.Endpoints.Products;
using demo_api.models.Models;

namespace demo_api.api.Interfaces.Product;

public interface IProductRepository
{
    Task<Result<models.Models.Product>> CreateAsync(CreateRequest request, CancellationToken token);
    Task<Result<PagedList<models.Models.Product>>>  GetAsync(GetRequest request, CancellationToken token);
    Task<Result> UpdateAsync(Guid id, UpdateRequest request, CancellationToken token);
    Task<Result> DeleteAsync(Guid id, Guid companyId, CancellationToken token);
}