using System.ComponentModel.DataAnnotations.Schema;

namespace demo_api.models.Models;

public class InvoiceItem
{
    public required Guid Id { get; set; } = Guid.NewGuid();
    public required Guid InvoiceId { get; set; }
    public required Guid ProductId { get; set; }
    public required decimal UnitPrice { get; set; }
    public required int Weight { get; set; }
    public required int Quantity { get; set; }
    [NotMapped]
    public decimal TotalPrice => Quantity * UnitPrice;
    
    [NotMapped]
    public int TotalWeight => Weight * Quantity;
}