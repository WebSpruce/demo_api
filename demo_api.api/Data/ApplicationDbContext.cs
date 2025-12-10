using demo_api.models.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace demo_api.api.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : IdentityDbContext<ApplicationUser>(options)
{
    public DbSet<Client> Clients { get; set; }
    public DbSet<Invoice> Invoices { get; set; }
    public DbSet<InvoiceItem> InvoiceItems { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<Company> Companies { get; set; }
    public DbSet<RefreshToken> RefreshTokens { get; set; }
    
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        var roleAdminId = "00000000-1111-0000-0000-000000000001";
        var roleManagerId = "00000000-2222-0000-0000-000000000002";
        var roleEmployeeId = "00000000-3333-0000-0000-000000000003";
        
        builder.Entity<IdentityRole>().HasData(
            new IdentityRole { Id = roleAdminId, Name = models.Models.Roles.Admin, NormalizedName = "ADMIN" },
            new IdentityRole { Id = roleManagerId, Name = models.Models.Roles.Manager, NormalizedName = "MANAGER" },
            new IdentityRole { Id = roleEmployeeId, Name = models.Models.Roles.Employee, NormalizedName = "EMPLOYEE" }
        );
        
        var tenantId = Guid.Parse("11111111-1111-1111-1111-111111111111"); // Przykładowa Firma
        var clientId = Guid.Parse("22222222-2222-2222-2222-222222222222");
        var invoiceId = Guid.Parse("44444444-4444-4444-4444-444444444444");
        var productId = Guid.Parse("55555555-5555-5555-5555-555555555555");
        var invoiceItemId = Guid.Parse("66666666-6666-6666-6666-666666666666");
        var userAId = "71775555-7777-7777-7777-777555555555";
        var userMId = "72775555-7777-7777-7777-777555555555";
        var userEId = "73775555-7777-7777-7777-777555555555";
        
        var adminSecurityStamp = "11111111-1111-1111-1111-111111111111";
        var adminConcurrencyStamp = "aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa";
        var managerSecurityStamp = "22222222-2222-2222-2222-222222222222";
        var managerConcurrencyStamp = "bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb";
        var employeeSecurityStamp = "33333333-3333-3333-3333-333333333333";
        var employeeConcurrencyStamp = "cccccccc-cccc-cccc-cccc-cccccccccccc";
        
        
        var staticDate = new DateTime(2024, 12, 10, 0, 0, 0, DateTimeKind.Utc);
        
        builder.Entity<Company>().HasData(new Company
        {
            Id = tenantId,
            CreatedAt = staticDate,
            Name = "Test Dev",
            Slug = "TDev"
        });

        var adminUser = new ApplicationUser
        {
            Id = userAId,
            CompanyId = tenantId,
            FirstName = "John",
            LastName = "Smith",
            CreatedAt = staticDate,
            NormalizedUserName = "JOHN@TEST.TEST",
            Email = "john@test.test",
            NormalizedEmail = "JOHN@TEST.TEST",
            EmailConfirmed = true,
            SecurityStamp = adminSecurityStamp,
            ConcurrencyStamp = adminConcurrencyStamp,
            UserName = "john@test.test"
        };
        //"Secret123!"
        adminUser.PasswordHash = "AQAAAAIAAYagAAAAENET13W7ee8AwVMikLQeK2yAH+BczAAJbk+xrkhmJxKohr2Z1ShcdDF7idl4qyJaGA==";
        builder.Entity<ApplicationUser>().HasData(adminUser);
        
        var managerUser = new ApplicationUser
        {
            Id = userMId,
            CompanyId = tenantId,
            FirstName = "Sven",
            LastName = "Sky",
            CreatedAt = staticDate,
            NormalizedUserName = "SVEN@TEST.TEST",
            Email = "sven@test.test",
            NormalizedEmail = "SVEN@TEST.TEST",
            EmailConfirmed = true,
            SecurityStamp = managerSecurityStamp,
            ConcurrencyStamp = managerConcurrencyStamp,
            UserName = "sven@test.test"
        };
        managerUser.PasswordHash = "AQAAAAIAAYagAAAAENET13W7ee8AwVMikLQeK2yAH+BczAAJbk+xrkhmJxKohr2Z1ShcdDF7idl4qyJaGA==";
        builder.Entity<ApplicationUser>().HasData(managerUser);
        
        var employeeUser = new ApplicationUser
        {
            Id = userEId,
            CompanyId = tenantId,
            FirstName = "Odin",
            LastName = "Cheese",
            CreatedAt = staticDate,
            NormalizedUserName = "ODIN@TEST.TEST",
            Email = "odin@test.test",
            NormalizedEmail = "ODIN@TEST.TEST",
            EmailConfirmed = true,
            SecurityStamp = employeeSecurityStamp,
            ConcurrencyStamp = employeeConcurrencyStamp,
            UserName = "odin@test.test"
        };
        employeeUser.PasswordHash = "AQAAAAIAAYagAAAAENET13W7ee8AwVMikLQeK2yAH+BczAAJbk+xrkhmJxKohr2Z1ShcdDF7idl4qyJaGA==";
        builder.Entity<ApplicationUser>().HasData(employeeUser);
        
        builder.Entity<IdentityUserRole<string>>().HasData(new IdentityUserRole<string>
        {
            RoleId = roleAdminId, 
            UserId = userAId
        });
        
        builder.Entity<IdentityUserRole<string>>().HasData(new IdentityUserRole<string>
        {
            RoleId = roleManagerId,
            UserId = userMId
        });
        
        builder.Entity<IdentityUserRole<string>>().HasData(new IdentityUserRole<string>
        {
            RoleId = roleManagerId,
            UserId = userEId
        });
        
        builder.Entity<Client>().HasData(new Client
        {
            Id = clientId,
            CompanyId = tenantId,
            Address = "Baker Street 3",
            Postcode = "333-333",
            City = "London",
            Location = "51.5205664,-0.159379"
        });
    
        builder.Entity<Product>().HasData(new Product
        {
            Id = productId,
            Name = "screwdriver",
            Price = 150.00,
            CompanyId = tenantId,
            Weight = 10
        });
    
        builder.Entity<Invoice>().HasData(new Invoice
        {
            Id = invoiceId,
            ClientId = clientId,
            CompanyId = tenantId,
            InvoiceNumber = "AAA/111/B",
            Status = "Pending"
        });
    
        builder.Entity<InvoiceItem>().HasData(new InvoiceItem
        {
            Id = invoiceItemId,
            InvoiceId = invoiceId,
            ProductId = productId,
            Quantity = 2,
            UnitPrice = 150.00m,
            Weight = 20
        });

        builder.Entity<Invoice>()
            .HasMany(i => i.InvoiceItems)
            .WithOne()
            .HasForeignKey(ii => ii.InvoiceId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<Invoice>()
            .HasOne<Client>()
            .WithMany()
            .HasForeignKey(i => i.ClientId)
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.Entity<Product>()
            .HasMany(p => p.InvoiceItems)
            .WithOne()
            .HasForeignKey(ii => ii.ProductId)
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.Entity<InvoiceItem>()
            .HasOne<Invoice>()
            .WithMany(i => i.InvoiceItems)
            .HasForeignKey(ii => ii.InvoiceId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<InvoiceItem>()
            .HasOne<Product>()
            .WithMany(p => p.InvoiceItems)
            .HasForeignKey(ii => ii.ProductId)
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.Entity<Company>()
            .HasOne(c => c.Owner)
            .WithMany()  
            .HasForeignKey(c => c.OwnerId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Entity<Company>()
            .HasMany(c => c.Users)
            .WithOne()  
            .HasForeignKey(u => u.CompanyId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Entity<Company>()
            .HasMany(c => c.Clients)
            .WithOne()
            .HasForeignKey(cl => cl.CompanyId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<Company>()
            .HasMany(c => c.Invoices)
            .WithOne()
            .HasForeignKey(i => i.CompanyId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<Company>()
            .HasMany(c => c.Products)
            .WithOne()
            .HasForeignKey(p => p.CompanyId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}