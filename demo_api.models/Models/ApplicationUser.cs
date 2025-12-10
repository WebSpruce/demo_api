using Microsoft.AspNetCore.Identity;

namespace demo_api.models.Models;

public class ApplicationUser : IdentityUser
{
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public required DateTime CreatedAt { get; set; }
    public Guid? CompanyId { get; set; }
    public List<Client> Clients { get; set; } = [];
}