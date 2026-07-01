using Microsoft.EntityFrameworkCore;
using MVideo.Api.Models;

namespace MVideo.Api.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Product> Products => Set<Product>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Category>(entity =>
        {
            entity.Property(category => category.Name)
                .HasMaxLength(100)
                .IsRequired();

            entity.HasIndex(category => category.Name)
                .IsUnique();

            entity.HasData(
                new Category { Id = 1, Name = "Телевизоры" },
                new Category { Id = 2, Name = "Смартфоны" },
                new Category { Id = 3, Name = "Ноутбуки" });
        });

        modelBuilder.Entity<Product>(entity =>
        {
            entity.Property(product => product.Name)
                .HasMaxLength(200)
                .IsRequired();

            entity.Property(product => product.Sku)
                .HasMaxLength(64)
                .IsRequired();

            entity.HasIndex(product => product.Sku)
                .IsUnique();

            entity.HasOne(product => product.Category)
                .WithMany(category => category.Products)
                .HasForeignKey(product => product.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasData(
                new Product
                {
                    Id = 1,
                    Name = "Samsung Galaxy S25",
                    Sku = "PHONE-001",
                    CategoryId = 2,
                    Status = ProductStatus.Active
                },
                new Product
                {
                    Id = 2,
                    Name = "Apple iPhone 16",
                    Sku = "PHONE-002",
                    CategoryId = 2,
                    Status = ProductStatus.Defective
                },
                new Product
                {
                    Id = 3,
                    Name = "LG OLED C4",
                    Sku = "TV-001",
                    CategoryId = 1,
                    Status = ProductStatus.Active
                },
                new Product
                {
                    Id = 4,
                    Name = "Samsung QLED Q80D",
                    Sku = "TV-002",
                    CategoryId = 1,
                    Status = ProductStatus.WriteOff
                },
                new Product
                {
                    Id = 5,
                    Name = "Lenovo ThinkPad E14",
                    Sku = "LAPTOP-001",
                    CategoryId = 3,
                    Status = ProductStatus.Defective
                });
        });
    }
}
