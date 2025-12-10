using System.ComponentModel.DataAnnotations;

namespace demo_api.models.Models;

public class Client
{
    public required Guid Id { get; set; } = Guid.NewGuid();
    public required Guid CompanyId { get; set; }
    public required string City { get; set; }
    [MaxLength(255)]
    public required string Address { get; set; }
    [MaxLength(50)]
    public required string Postcode { get; set; }
    public required string Location { get; set; }
    public List<ApplicationUser> Users { get; set; } = [];
}