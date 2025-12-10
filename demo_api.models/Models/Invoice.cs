using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace demo_api.models.Models;

public class Invoice
{
    public required Guid Id { get; set; } = Guid.NewGuid();
    public required Guid CompanyId { get; set; }
    [MaxLength(100)]
    public required string InvoiceNumber { get; set; }
    public required Guid ClientId { get; set; }
    [MaxLength(30)]
    public required string Status { get; set; }
    public Guid? ParentInvoiceId { get; set; }
    
    [NotMapped]
    public int TotalWeight => InvoiceItems.Sum(i => i.TotalWeight);
    [NotMapped]
    public int TotalQuantity => InvoiceItems.Sum(i => i.Quantity);
    public ICollection<InvoiceItem> InvoiceItems { get; set; } = [];
}