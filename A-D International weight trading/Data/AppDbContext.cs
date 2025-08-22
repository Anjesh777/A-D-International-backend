using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using A_D_International_weight_trading.Model;

namespace A_D_International_weight_trading.Data
{
    public class AppDbContext : IdentityDbContext<ApplicationUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Product> Products { get; set; }
        public DbSet<ProductImage> ProductImages { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Product>(entity => {
                entity.HasKey(p => p.Id);
                entity.Property(p => p.Name).IsRequired().HasMaxLength(200);
                entity.Property(p => p.Description).IsRequired().HasMaxLength(1000);
                entity.Property(p => p.Category).IsRequired().HasMaxLength(100);
                entity.Property(p => p.Specifications).HasMaxLength(2000);
                entity.Property(p => p.Status).HasMaxLength(20).HasDefaultValue("active");
                entity.Property(p => p.Standards).HasMaxLength(500);

                // Fixed: Use MySQL-compatible syntax
                entity.Property(p => p.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP(6)");
                entity.Property(p => p.UpdatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6)");

                entity.HasIndex(p => p.Name);
                entity.HasIndex(p => p.Category);
                entity.HasIndex(p => p.Status);
            });

            builder.Entity<ProductImage>(entity =>
            {
                entity.HasKey(pi => pi.Id);
                entity.Property(pi => pi.ImageUrl).IsRequired();
                entity.Property(pi => pi.PublicId).IsRequired();

                // Fixed: Use MySQL-compatible syntax instead of GETUTCDATE()
                entity.Property(pi => pi.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP(6)");

                entity.HasOne(pi => pi.Product)
                      .WithMany(p => p.Images)
                      .HasForeignKey(pi => pi.ProductId)
                      .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}