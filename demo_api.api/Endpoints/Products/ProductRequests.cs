namespace demo_api.api.Endpoints.Products;

public record CreateRequest(Guid CompanyId, string Name, int Weight, double Price);
public record GetRequest(Guid? Id, Guid CompanyId, string? Name, int? Weight, double? Price, PaginationRequest? pagination);
public record UpdateRequest(Guid CompanyId, string? Name, int? Weight, double? Price);