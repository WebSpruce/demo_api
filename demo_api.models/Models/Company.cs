namespace demo_api.models.Models;

public class Company
{
    public required Guid Id { get; set; } = Guid.NewGuid();
    public required string Name { get; set; }
    public required string Slug { get; set; } // unique identifier for URL
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public string? OwnerId { get; set; }
    public ApplicationUser Owner { get; set; } = null!;
    
    public ICollection<ApplicationUser> Users { get; set; } = [];
    public ICollection<Client> Clients { get; set; } = [];
    public ICollection<Invoice> Invoices { get; set; } = [];
    public ICollection<Product> Products { get; set; } = [];
}