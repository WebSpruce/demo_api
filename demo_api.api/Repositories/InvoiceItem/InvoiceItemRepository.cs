using FluentValidation;
using demo_api.api.Data;
using demo_api.api.Endpoints.InvoiceItem;
using demo_api.api.Infrastructure;
using demo_api.api.Interfaces.InvoiceItem;
using demo_api.models.Models;
using Microsoft.EntityFrameworkCore;

namespace demo_api.api.Repositories.InvoiceItem;

public class InvoiceItemRepository : IInvoiceItemRepository
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IValidator<CreateRequest> _validatorCreate;
    private readonly IValidator<GetRequest> _validatorGet;
    private readonly IValidator<UpdateRequest> _validatorUpdate;
    public InvoiceItemRepository(ApplicationDbContext dbContext, IValidator<CreateRequest> validatorCreate, IValidator<GetRequest> validatorGet, IValidator<UpdateRequest> validatorUpdate)
    {
        _dbContext = dbContext;
        _validatorCreate = validatorCreate;
        _validatorGet = validatorGet;
        _validatorUpdate = validatorUpdate;
    }
    public async Task<Result<models.Models.InvoiceItem>> CreateAsync(CreateRequest request, CancellationToken token)
    {
        if(token.IsCancellationRequested)
            return Result.Cancelled<models.Models.InvoiceItem>();    
        
        var validationResult = await _validatorCreate.ValidateAsync(request, token);
        if(!validationResult.IsValid)
            return Result.ValidationFailure<models.Models.InvoiceItem>(new Dictionary<string, string[]>(validationResult.ToDictionary()));
        
        var invoiceItem = new models.Models.InvoiceItem()
        {
            Id = Guid.NewGuid(),
            InvoiceId = request.InvoiceId,
            ProductId = request.ProductId,
            Quantity = request.Quantity,
            UnitPrice = request.UnitPrice,
            Weight = request.Weight
        };
        
        await _dbContext.InvoiceItems.AddAsync(invoiceItem, token);
        await _dbContext.SaveChangesAsync(token);
        
        return Result.Success(invoiceItem);
    }

    public async Task<Result<PagedList<models.Models.InvoiceItem>>> GetAsync(GetRequest request, CancellationToken token)
    {
        if(token.IsCancellationRequested)
            return Result.Cancelled<PagedList<models.Models.InvoiceItem>>();    
        
        var validationResult = await _validatorGet.ValidateAsync(request);
        if(!validationResult.IsValid)
            return Result.ValidationFailure<PagedList<models.Models.InvoiceItem>>(new Dictionary<string, string[]>(validationResult.ToDictionary()));
        
        var invoices = _dbContext.InvoiceItems
            .AsNoTracking()
            .Where(invoiceItem => 
                (request.Id == null || invoiceItem.Id == request.Id) &&
                (request.InvoiceId == null || invoiceItem.InvoiceId == request.InvoiceId) &&
                (request.ProductId == null || invoiceItem.ProductId == request.ProductId) &&
                (request.UnitPrice == null || invoiceItem.UnitPrice == request.UnitPrice) &&
                (request.Weight == null || invoiceItem.Weight == request.Weight) &&
                (request.Quantity == null || invoiceItem.Quantity == request.Quantity)
            ).AsQueryable();

        var result = await Pagination.Paginate(invoices, request.pagination?.pageNumber, request.pagination?.pageSize, token);

        return Result.Success(result);
    }

    public async Task<Result> UpdateAsync(Guid id, UpdateRequest request, CancellationToken token)
    {
        if(token.IsCancellationRequested)
            return Result.Cancelled();    
        
        var validationResult = await _validatorUpdate.ValidateAsync(request, token);
        if(!validationResult.IsValid)
            return Result.ValidationFailure(new Dictionary<string, string[]>(validationResult.ToDictionary()));
        
        var invoiceItem = await _dbContext.InvoiceItems.FirstOrDefaultAsync(x => x.Id == id, token);
        if (invoiceItem is null)
            return Result.Failure(new List<object>() { "There is no invoice item with the provided Id" });

        if (request.InvoiceId is not null)
            invoiceItem.InvoiceId = (Guid)request.InvoiceId;
        if (request.ProductId is not null)
            invoiceItem.ProductId = (Guid)request.ProductId;
        if (request.UnitPrice is not null)
            invoiceItem.UnitPrice = (decimal)request.UnitPrice;
        if (request.Weight is not null)
            invoiceItem.Weight = (int)request.Weight;
        if (request.Quantity is not null)
            invoiceItem.Quantity = (int)request.Quantity;
        
        await _dbContext.SaveChangesAsync(token);

        return Result.Success();
    }

    public async Task<Result> DeleteAsync(Guid id, CancellationToken token)
    {
        if(token.IsCancellationRequested)
            return Result.Cancelled();    
        
        var invoiceItem = await _dbContext.InvoiceItems.FirstOrDefaultAsync(x => x.Id == id, token);
        if (invoiceItem is null)
            return Result.Failure(new List<object>() { "There is no invoice item with the provided Id" });

        _dbContext.InvoiceItems.Remove(invoiceItem);

        await _dbContext.SaveChangesAsync(token);

        return Result.Success();
    }
}