using System.ComponentModel.DataAnnotations;

namespace demo_api.models.Models;

public class Product
{
    public required Guid Id { get; set; } = Guid.NewGuid();
    public required Guid CompanyId { get; set; }
    [MaxLength(200)]
    public required string Name { get; set; }
    public required int Weight { get; set; }
    public required double Price { get; set; }
    public ICollection<InvoiceItem> InvoiceItems { get; set; } = [];
}