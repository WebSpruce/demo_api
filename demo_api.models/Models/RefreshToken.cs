namespace demo_api.models.Models;

public class RefreshToken
{
    public Guid Id { get; set; }
    public string Token { get; set; }
    public string UserId { get; set; }
    public DateTime ExpiresOnUtc { get; set; }
    public ApplicationUser User { get; set; }
}