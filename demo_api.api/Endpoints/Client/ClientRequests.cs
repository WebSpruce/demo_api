namespace demo_api.api.Endpoints.Client;

public record CreateRequest(Guid CompanyId, string City, string Address, string PostCode, string Location);
public record GetRequest(Guid? Id, Guid CompanyId, string? City, string? Address, string? PostCode, string? Location, string? UserId, PaginationRequest? pagination);
public record UpdateRequest(Guid CompanyId, string? City, string? Address, string? PostCode, string? Location);