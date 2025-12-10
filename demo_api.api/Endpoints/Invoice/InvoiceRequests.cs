namespace demo_api.api.Endpoints.Invoice;

public record CreateRequest(Guid CompanyId, string InvoiceNumber, Guid ClientId, string Status, Guid? ParentInvoiceId);
public record GetRequest(Guid? Id, Guid CompanyId, string? InvoiceNumber, Guid? ClientId, string? Status, Guid? ParentInvoiceId, PaginationRequest? pagination);
public record UpdateRequest(Guid CompanyId, string? InvoiceNumber, Guid? ClientId, string? Status, Guid? ParentInvoiceId);