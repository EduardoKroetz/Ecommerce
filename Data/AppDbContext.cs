using Ecommerce.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Data;

public class AppDbContext : IdentityDbContext<User>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Product> Products { get; set; }
    public DbSet<ProductCategory> ProductCategories { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.Entity<Product>()
            .HasMany(p => p.Categories)
            .WithMany()
            .UsingEntity<Dictionary<string, object>>("ProductsCategories");

        builder.Entity<Product>()
           .Property(p => p.Price)
           .HasPrecision(18, 2);

        builder.Entity<Product>()
           .Property(p => p.Name)
           .HasMaxLength(100)
           .IsRequired();

        builder.Entity<Product>()
           .Property(p => p.Description)
           .HasMaxLength(500);

        base.OnModelCreating(builder);
    }
}
