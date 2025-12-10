using System.Text;
using FluentValidation;
using demo_api.api.Data;
using demo_api.api.Endpoints;
using demo_api.api.Infrastructure;
using demo_api.api.Interfaces.Client;
using demo_api.api.Interfaces.Company;
using demo_api.api.Interfaces.Invoice;
using demo_api.api.Interfaces.InvoiceItem;
using demo_api.api.Interfaces.Product;
using demo_api.api.Interfaces.User;
using demo_api.api.Repositories.Client;
using demo_api.api.Repositories.Company;
using demo_api.api.Repositories.Invoice;
using demo_api.api.Repositories.InvoiceItem;
using demo_api.api.Repositories.Product;
using demo_api.api.Repositories.User;
using demo_api.models.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;

namespace demo_api.api;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        builder.Logging.ClearProviders();
        builder.Logging.AddConsole();

        builder.Services.AddOpenApi(options =>
        {
            options.AddDocumentTransformer<BearerSecuritySchemeTransformer>();
        });

        var connectionString = builder.Configuration.GetConnectionString("PGConnection");
        if (string.IsNullOrEmpty(connectionString))
        {
            throw new InvalidOperationException(
                "Connection string 'PGConnection' is not configured. " +
                "Set CONNECTIONSTRINGS__PGCONNECTION environment variable.");
        }
        builder.Services.AddDbContext<ApplicationDbContext>(options =>
        {
            options.UseNpgsql(connectionString);
        });
        builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
            .AddEntityFrameworkStores<ApplicationDbContext>();

        builder.Services.AddValidatorsFromAssembly(typeof(Program).Assembly, includeInternalTypes:true);

        builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
        builder.Services.AddProblemDetails();
        
        
        var jwtSettingsInfo = builder.Configuration.GetSection("JWT");
        var jwtSecret = jwtSettingsInfo["SecretKey"];
        if (string.IsNullOrEmpty(jwtSecret))
        {
            throw new InvalidOperationException(
                "JWT secret is not configured. " +
                "Set JWT__SECRETKEY environment variable.");
        }
        
        builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("Jwt"));
        var jwtSettings = builder.Configuration.GetSection("Jwt").Get<JwtSettings>();
        builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters.ValidIssuer = jwtSettings?.Issuer;
                options.TokenValidationParameters.ValidAudience = jwtSettings?.Audience;
                options.TokenValidationParameters.IssuerSigningKey =
                    new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings?.SecretKey!));
            });
        builder.Services.AddAuthorization();

        builder.Services.RegisterModules();
        builder.Services.AddScoped<IUserRepository, UserRepository>();
        builder.Services.AddScoped<IInvoiceRepository, InvoiceRepository>();
        builder.Services.AddScoped<IProductRepository, ProductRepository>();
        builder.Services.AddScoped<IClientRepository, ClientRepository>();
        builder.Services.AddScoped<IInvoiceItemRepository, InvoiceItemRepository>();
        builder.Services.AddScoped<ICompanyRepository, CompanyRepository>();

        var app = builder.Build();
        
        app.MapOpenApi();
        app.MapScalarApiReference(options =>
        {
            options.WithTitle("demo_api")
                .WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient);
            options.AddPreferredSecuritySchemes("Bearer");
        });

        using var scope = app.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        if (dbContext.Database.IsRelational())
        {
            dbContext.Database.Migrate();
        }

        app.UseHttpsRedirection();

        app.UseExceptionHandler();

        app.UseAuthentication();
        app.UseAuthorization();
        
        app.MapEndpoints();
        
        app.Run();
    }
}