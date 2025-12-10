namespace demo_api.api.Endpoints;

public record PaginationRequest(int? pageNumber, int? pageSize);