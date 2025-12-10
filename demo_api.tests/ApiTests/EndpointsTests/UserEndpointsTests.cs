using System.Net;
using System.Text.Json;
using FluentAssertions;
using demo_api.api.Data;
using demo_api.api.Endpoints.User;
using demo_api.models.Models;
using Microsoft.EntityFrameworkCore;

namespace demo_api.tests.ApiTests.EndpointsTests;

public class UserEndpointsTests : IClassFixture<ApiFactory>
{
    private readonly ApiFactory _factory;
    private readonly HttpClient _client;
    private readonly ApplicationDbContext _dbContext;
    private const string BaseUrl = "/api/v1";
    public UserEndpointsTests(ApiFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
        _dbContext = factory.CreateDbContext();
    }

    [Fact]
    public async Task TokenCheck_ShouldReturnStatusOK_WhenAuth()
    {
        var response = await _client.GetAsync($"{BaseUrl}/users/token-check");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task PostUsers_ShouldReturnStatusOK_WhenUserCreated()
    {
        var request = new RegisterRequest("test@test.test", "test@L123", "test@test.test", "test@test.test", "Employee");
        var content = new StringContent(JsonSerializer.Serialize(request), 
            System.Text.Encoding.UTF8,
            "application/json");
        
        var response = await _client.PostAsync($"{BaseUrl}/users", content);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Theory]
    [InlineData("aaa", "ddd")]
    public async Task Login_ShouldReturnValidationFailedMessage_WhenValidationFailed(string email, string password)
    {
        var request = new LoginRequest(email, password);
        var content = new StringContent(JsonSerializer.Serialize(request),
            System.Text.Encoding.UTF8,
            "application/json");

        var response = await _client.PostAsync($"{BaseUrl}/users/login", content);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var responseBody = await response.Content.ReadAsStringAsync();
        responseBody.Should().Contain("Validation failed");
    }
    
    [Fact]
    public async Task Update_ShouldReturnOK_WhenUpdateSuccess()
    {
        await _dbContext.Users.AddAsync(
            new ApplicationUser()
            {
                FirstName = "test@test2.test",
                LastName = "test@test2.test",
                Email = "test@test2.test",
                CreatedAt = DateTime.UtcNow
            });
        await _dbContext.SaveChangesAsync();
        
        Guid companyId = Guid.NewGuid();
        var ownerId = await _dbContext.Users.FirstOrDefaultAsync(x => x.Email == "test@test2.test");
        await _dbContext.Companies.AddAsync(new Company()
        {
            Slug = "test",
            Name = "test",
            CreatedAt = DateTime.UtcNow,
            Id = companyId,
            OwnerId = ownerId.Id
        });
        await _dbContext.Users.AddAsync(
            new ApplicationUser()
            {
                FirstName = "test@test.test",
                LastName = "test@test.test",
                Email = "test@test.test",
                CreatedAt = DateTime.UtcNow,
                CompanyId = companyId
            }
        );
        await _dbContext.SaveChangesAsync();
        
        var user = await _dbContext.Users.FirstOrDefaultAsync(x => x.Email == "test@test.test");
        
        var request = new UpdateRequest(companyId, "testname", null, null, null);
        var content = new StringContent(JsonSerializer.Serialize(request),
            System.Text.Encoding.UTF8,
            "application/json");

        var response = await _client.PatchAsync($"{BaseUrl}/users/{user?.Id}", content);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        _dbContext.ChangeTracker.Clear();
        var updatedUser = await _dbContext.Users.FirstOrDefaultAsync(x => x.Email == "test@test.test");
        updatedUser.FirstName.Should().NotBeNull();
        updatedUser.FirstName.Should().Be("testname"); 
    }
}