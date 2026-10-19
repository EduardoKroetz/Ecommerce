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
    public DbSet<Seller> Sellers { get; set; }
    public DbSet<Purchase> Purchases { get; set; }

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
           .HasMaxLength(Product.NameMaxLength)
           .IsRequired();

        builder.Entity<Product>()
           .Property(p => p.Description)
           .HasMaxLength(Product.DescriptionMaxLength);

        builder.Entity<ProductCategory>()
           .Property(pc => pc.Name)
           .HasMaxLength(ProductCategory.NameMaxLength)
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

        builder.Entity<Seller>()
           .HasOne<ApplicationUser>()
           .WithOne()
           .HasForeignKey<Seller>(s => s.UserId)
           .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Seller>().HasIndex(s => s.UserId).IsUnique();

        builder.Entity<Product>()
           .HasOne<Seller>()
           .WithMany()
           .HasForeignKey(p => p.SellerId)
           .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Order>()
           .HasOne<Seller>()
           .WithMany()
           .HasForeignKey(o => o.SellerId)
           .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Purchase>()
           .HasMany(p => p.Orders)
           .WithOne(o => o.Purchase)
           .HasForeignKey(o => o.PurchaseId)
           .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Purchase>()
           .HasOne<ApplicationUser>()
           .WithMany()
           .HasForeignKey(p => p.UserId)
           .OnDelete(DeleteBehavior.Restrict);

        base.OnModelCreating(builder);
    }
}
