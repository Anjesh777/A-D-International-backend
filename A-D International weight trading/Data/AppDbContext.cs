using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using A_D_International_weight_trading.Model;
using System.Reflection.Emit;

namespace A_D_International_weight_trading.Data
{
    public class AppDbContext : IdentityDbContext<ApplicationUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Product> Products { get; set; }
        public DbSet<ProductImage> ProductImages { get; set; }
        public DbSet<Category> Categories { get; set; }

        public DbSet<Banner> Banners { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Category>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Description).HasMaxLength(500);
                entity.Property(e => e.Status).IsRequired().HasMaxLength(20).HasDefaultValue("active");
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP(6)");
                entity.Property(e => e.UpdatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6)");
                entity.HasIndex(e => e.Name).IsUnique(); // Ensure unique category names
            });

            builder.Entity<Product>(entity =>
            {
                entity.HasKey(p => p.Id);
                entity.Property(p => p.Name).IsRequired().HasMaxLength(200);
                entity.Property(p => p.Description).IsRequired().HasMaxLength(1000);
                entity.Property(p => p.CategoryId).IsRequired(); 
                entity.Property(p => p.Specifications).HasMaxLength(2000);
                entity.Property(p => p.Status).HasMaxLength(20).HasDefaultValue("active");
                entity.Property(p => p.Standards).HasMaxLength(500);
                entity.Property(p => p.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP(6)");
                entity.Property(p => p.UpdatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6)");

                // Foreign key relationship to Category
                entity.HasOne(p => p.Category)
                      .WithMany(c => c.Products)
                      .HasForeignKey(p => p.CategoryId)
                      .OnDelete(DeleteBehavior.Restrict); // Prevent deleting categories with products

                // Indexes
                entity.HasIndex(p => p.Name);
                entity.HasIndex(p => p.CategoryId);
                entity.HasIndex(p => p.Status);
            });

            builder.Entity<ProductImage>(entity =>
            {
                entity.HasKey(pi => pi.Id);
                entity.Property(pi => pi.ImageUrl).IsRequired();
                entity.Property(pi => pi.PublicId).IsRequired();
                entity.Property(pi => pi.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP(6)");

                entity.HasOne(pi => pi.Product)
                      .WithMany(p => p.Images)
                      .HasForeignKey(pi => pi.ProductId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            builder.Entity<Banner>(entity =>
            {
                // Primary key
                entity.HasKey(e => e.Id);

                // Required properties
                entity.Property(e => e.Title)
                    .IsRequired()
                    .HasMaxLength(200);

                entity.Property(e => e.ImageUrl)
                    .IsRequired()
                    .HasMaxLength(500);

                entity.Property(e => e.Status)
                    .IsRequired()
                    .HasMaxLength(20)
                    .HasDefaultValue("active");

                entity.Property(e => e.Subtitle).HasMaxLength(500);
                entity.Property(e => e.PublicId).HasMaxLength(100);
                entity.Property(e => e.ExternalUrl).HasMaxLength(500);

                // Properties with defaults
                entity.Property(e => e.ButtonText)
                    .HasMaxLength(50)
                    .HasDefaultValue("Shop Now");

                entity.Property(e => e.LinkType)
                    .HasMaxLength(20)
                    .HasDefaultValue("product");

                entity.Property(e => e.DisplayOrder).HasDefaultValue(0);
                entity.Property(e => e.IsActive).HasDefaultValue(true);

                // Timestamps - FIXED FOR MYSQL
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP(6)");
                entity.Property(e => e.UpdatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6)");

                // Foreign key relationships
                entity.HasOne(b => b.Product)
                    .WithMany()
                    .HasForeignKey(b => b.ProductId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(b => b.Category)
                    .WithMany()
                    .HasForeignKey(b => b.CategoryId)
                    .OnDelete(DeleteBehavior.SetNull);

                // Performance indexes
                entity.HasIndex(b => new { b.Status, b.IsActive });
                entity.HasIndex(b => b.DisplayOrder);
                entity.HasIndex(b => new { b.StartDate, b.EndDate });
                entity.HasIndex(b => b.LinkType);
            });
        }
    }
}