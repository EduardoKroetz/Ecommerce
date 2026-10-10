using Ecommerce.Domain.Entities;
using Ecommerce.Infra.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Infra.Data;

public class AppDbContext : IdentityDbContext<ApplicationUser>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Product> Products { get; set; }
    public DbSet<ProductCategory> ProductCategories { get; set; }
    public DbSet<Cart> Carts { get; set; }
    public DbSet<CartItem> CartItems { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderItem> OrderItems { get; set; }

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

        builder.Entity<ProductCategory>()
           .Property(pc => pc.Name)
           .HasMaxLength(100)
           .IsRequired();

        builder.Entity<Cart>()
           .HasMany<CartItem>(x => x.Items)
           .WithOne(ci => ci.Cart)
           .HasForeignKey(ci => ci.CartId)
           .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<Cart>()
           .HasOne<ApplicationUser>()
           .WithOne()
           .HasForeignKey<Cart>(c => c.UserId)
           .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<CartItem>()
           .HasOne(ci => ci.Product)
           .WithMany()
           .HasForeignKey(ci => ci.ProductId)
           .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<CartItem>().HasIndex(ci => new { ci.CartId, ci.ProductId }).IsUnique();

        builder.Entity<Order>()
           .HasMany(o => o.Items)
           .WithOne()
           .HasForeignKey(oi => oi.OrderId)
           .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<OrderItem>()
           .HasOne(oi => oi.Product)
           .WithMany()
           .HasForeignKey(oi => oi.ProductId)
           .OnDelete(DeleteBehavior.Restrict);

        base.OnModelCreating(builder);
    }
}
