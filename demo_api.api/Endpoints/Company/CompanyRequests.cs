namespace demo_api.api.Endpoints.Company;

public record CreateRequest(string Name, string Slug, string OwnerId);
public record GetRequest(Guid? Id, string? Name, string? Slug, string? OwnerId, PaginationRequest? pagination);
public record UpdateRequest(string? Name, string? Slug, string? OwnerId);