namespace demo_api.api.Endpoints.InvoiceItem;

public record CreateRequest(Guid InvoiceId, Guid ProductId, decimal UnitPrice , int Weight, int Quantity);
public record GetRequest(Guid? Id, Guid? InvoiceId, Guid? ProductId, decimal? UnitPrice , int? Weight, int? Quantity, PaginationRequest? pagination);
public record UpdateRequest(Guid? InvoiceId, Guid? ProductId, decimal? UnitPrice , int? Weight, int? Quantity);