namespace demo_api.api.Endpoints.User;

public record RegisterRequest(string Email, string Password, string FirstName, string LastName, string Role);
public record LoginRequest(string Email, string Password);
public record GetRequest(string? Id, Guid? CompanyId, string? Email, string? FirstName, string? LastName, string? UserName, string? PhoneNumber, string? RoleName, DateTime? CreatedAt, Guid? ClientId, PaginationRequest? pagination);
public record UpdateRequest(Guid CompanyId, string? FirstName, string? LastName, string? UserName, string? PhoneNumber);