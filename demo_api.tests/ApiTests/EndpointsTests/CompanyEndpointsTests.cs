using System.Net;
using System.Text.Json;
using FluentAssertions;
using demo_api.api.Data;
using demo_api.models.Models;
using Microsoft.EntityFrameworkCore;

namespace demo_api.tests.ApiTests.EndpointsTests;

public class CompanyEndpointsTests : IClassFixture<ApiFactory>
{
    private readonly ApiFactory _factory;
    private readonly HttpClient _client;
    private readonly ApplicationDbContext _dbContext;
    private const string BaseUrl = "/api/v1";
    public CompanyEndpointsTests(ApiFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
        _dbContext = factory.CreateDbContext();
    }

    [Fact]
    public async Task PostCompany_ShouldReturnStatusOK_WhenCompanyCreated()
    {
        string userId = "asdlkjfalksjdflkjasdlfj";
        _dbContext.Users.Add(new ApplicationUser()
        {
            FirstName = "test",
            LastName = "test",
            CreatedAt = DateTime.UtcNow,
            Id = userId
        });
        _dbContext.SaveChangesAsync();
        var request = new demo_api.api.Endpoints.Company.CreateRequest("TestCompany", "tcomp", userId);
        var content = new StringContent(JsonSerializer.Serialize(request), 
            System.Text.Encoding.UTF8,
            "application/json");
        
        var response = await _client.PostAsync($"{BaseUrl}/companies", content);
        var sss = await response.Content.ReadAsStringAsync();
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
    [Fact]
    public async Task PostCompany_ShouldReturnBadRequest_WhenUserDoesntExists()
    {
        var request = new demo_api.api.Endpoints.Company.CreateRequest("TestCompany", "tcomp", "asdlkjfalksjdflkjasdlfj");
        var content = new StringContent(JsonSerializer.Serialize(request), 
            System.Text.Encoding.UTF8,
            "application/json");
        
        var response = await _client.PostAsync($"{BaseUrl}/companies", content);
        var sss = await response.Content.ReadAsStringAsync();
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
    
    [Fact]
    public async Task AddUserToCompany_ShouldReturnOk_WhenUserAndCompanyExist()
    {
        var (companyId, ownerId) = await SetupCompanyAndUserAsync();
        
        var newUserId = Guid.NewGuid().ToString();
        await _dbContext.Users.AddAsync(new ApplicationUser
        {
            Id = newUserId,
            FirstName = "Employee",
            LastName = "One",
            Email = "employee@example.com",
            UserName = "employee1",
            CreatedAt = DateTime.UtcNow
        });
        await _dbContext.SaveChangesAsync();
        var response = await _client.PostAsync($"{BaseUrl}/companies/{companyId}/users?userId={newUserId}", null);
    
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    
        _dbContext.ChangeTracker.Clear();
        var company = await _dbContext.Companies
            .Include(c => c.Users)
            .FirstOrDefaultAsync(c => c.Id == companyId);
    
        company.Should().NotBeNull();
        company.Users.Should().Contain(u => u.Id == newUserId);
    }
    
    [Fact]
    public async Task AddUserToCompany_ShouldReturnBadRequest_WhenUserDoesNotExist()
    {
        var (companyId, _) = await SetupCompanyAndUserAsync();
        var nonExistentUserId = Guid.NewGuid().ToString();
    
        var response = await _client.PostAsync($"{BaseUrl}/companies/{companyId}/users?userId={nonExistentUserId}", null);
    
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
    
    [Fact]
    public async Task RemoveUserFromCompany_ShouldReturnOk_WhenUserIsRemoved()
    {
        var (companyId, ownerId) = await SetupCompanyAndUserAsync();

        var employeeId = Guid.NewGuid().ToString();
        var employee = new ApplicationUser
        {
            Id = employeeId,
            FirstName = "Employee",
            LastName = "To Remove",
            Email = "remove@example.com",
            UserName = "remove1",
            CreatedAt = DateTime.UtcNow,
            CompanyId = companyId
        };
    
        await _dbContext.Users.AddAsync(employee);
        
        var company = await _dbContext.Companies.FindAsync(companyId);
        if (company.Users == null) company.Users = new List<ApplicationUser>();
        company.Users.Add(employee);
    
        await _dbContext.SaveChangesAsync();

        var response = await _client.DeleteAsync($"{BaseUrl}/companies/{companyId}/users/{employeeId}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        _dbContext.ChangeTracker.Clear();
        var userInDb = await _dbContext.Users.FindAsync(employeeId);
        
        userInDb.CompanyId.Should().BeNull();
    }

    [Fact]
    public async Task RemoveUserFromCompany_ShouldReturnNotFound_WhenUserIsNotInCompany()
    {
        var (companyId, _) = await SetupCompanyAndUserAsync();
        var randomUserId = Guid.NewGuid().ToString(); 

        var response = await _client.DeleteAsync($"{BaseUrl}/companies/{companyId}/users/{randomUserId}");
        
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
    
    private async Task<(Guid CompanyId, string UserId)> SetupCompanyAndUserAsync()
    {
        var userId = Guid.NewGuid().ToString();
        var user = new ApplicationUser
        {
            Id = userId,
            FirstName = "Test",
            LastName = "User",
            Email = $"test-{Guid.NewGuid()}@example.com",
            UserName = $"test-{Guid.NewGuid()}",
            CreatedAt = DateTime.UtcNow
        };
        await _dbContext.Users.AddAsync(user);

        var companyId = Guid.NewGuid();
        var company = new Company
        {
            Id = companyId,
            Name = "Test Company",
            Slug = $"test-{Guid.NewGuid()}", 
            OwnerId = userId,
            CreatedAt = DateTime.UtcNow
        };
        await _dbContext.Companies.AddAsync(company);
    
        await _dbContext.SaveChangesAsync();
    
        return (companyId, userId);
    }
}