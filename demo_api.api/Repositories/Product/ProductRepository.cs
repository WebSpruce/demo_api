using FluentValidation;
using demo_api.api.Data;
using demo_api.api.Endpoints.Products;
using demo_api.api.Infrastructure;
using demo_api.api.Interfaces.Product;
using demo_api.models.Models;
using Microsoft.EntityFrameworkCore;

namespace demo_api.api.Repositories.Product;

public class ProductRepository : IProductRepository
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IValidator<GetRequest> _validatorGet;
    private readonly IValidator<CreateRequest> _validatorCreate;
    private readonly IValidator<UpdateRequest> _validatorUpdate;
    public ProductRepository(ApplicationDbContext dbContext, IValidator<GetRequest> validatorGet, IValidator<CreateRequest> validatorCreate, IValidator<UpdateRequest> validatorUpdate)
    {
        _dbContext = dbContext;
        _validatorGet = validatorGet;
        _validatorCreate = validatorCreate;
        _validatorUpdate = validatorUpdate;
    }
    public async Task<Result<models.Models.Product>> CreateAsync(CreateRequest request, CancellationToken token)
    {
        if (token.IsCancellationRequested)
            return Result.Cancelled<models.Models.Product>();
            
        var validationResult = await _validatorCreate.ValidateAsync(request, token);
        if (!validationResult.IsValid)
            return Result.ValidationFailure<models.Models.Product>(new Dictionary<string, string[]>(validationResult.ToDictionary()));

        var product = new models.Models.Product()
        {
            Id = Guid.NewGuid(),
            CompanyId = request.CompanyId,
            Name = request.Name,
            Weight = request.Weight,
            Price = request.Price
        };
        
        await _dbContext.Products.AddAsync(product, token);
        await _dbContext.SaveChangesAsync(token);
        
        return Result.Success(product);
    }

    public async Task<Result<PagedList<models.Models.Product>>> GetAsync(GetRequest request, CancellationToken token)
    {
        if (token.IsCancellationRequested)
            return Result.Cancelled<PagedList<models.Models.Product>>();

        var validationResult = _validatorGet.Validate(request);
        if (!validationResult.IsValid)
            return Result.ValidationFailure<PagedList<models.Models.Product>>(new Dictionary<string, string[]>(validationResult.ToDictionary()));

        var products = _dbContext.Products
            .AsNoTracking()
            .Where(product => 
                product.CompanyId == request.CompanyId &&
                (request.Id == null || product.Id == request.Id) &&
                (string.IsNullOrEmpty(request.Name) || product.Name.ToLower() == request.Name.ToLower()) &&
                (request.Weight == null || product.Weight == request.Weight.Value) &&
                (request.Price == null || (int)product.Price == (int)request.Price.Value)
            ).AsQueryable();

        var result = await Pagination.Paginate(products, request.pagination?.pageNumber, request.pagination?.pageSize, token);

        return Result.Success(result);
    }

    public async Task<Result> UpdateAsync(Guid id, UpdateRequest request, CancellationToken token)
    {
        if (token.IsCancellationRequested)
            return Result.Cancelled();

        var validationResult = _validatorUpdate.Validate(request);
        if (!validationResult.IsValid)
            return Result.ValidationFailure(new Dictionary<string, string[]>(validationResult.ToDictionary()));

        var product = await _dbContext.Products.FirstOrDefaultAsync(x => x.Id == id && x.CompanyId == request.CompanyId, token);
        if (product is null)
            return Result.Failure(new List<object>() { "Product not found or you do not have access" });

        if (request.Name is not null)
            product.Name = request.Name;
        if (request.Weight is not null)
            product.Weight = (int)request.Weight;
        if (request.Price is not null)
            product.Price = (double)request.Price;


        await _dbContext.SaveChangesAsync(token);

        return Result.Success();
    }

    public async Task<Result> DeleteAsync(Guid id, Guid companyId, CancellationToken token)
    {
        if (token.IsCancellationRequested)
            return Result.Cancelled();
        
        var product = await _dbContext.Products.FirstOrDefaultAsync(x => x.Id == id && x.CompanyId == companyId, token);
        if (product is null)
            return Result.Failure(new List<object>() { "Product not found or you do not have access" });

        _dbContext.Products.Remove(product);

        await _dbContext.SaveChangesAsync(token);

        return Result.Success();
    }
}