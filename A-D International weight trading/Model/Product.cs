using System.ComponentModel.DataAnnotations;

namespace A_D_International_weight_trading.Model
{
    public class Product
    {
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Name { get; set; }

        [Required]
        [StringLength(1000)]
        public string Description { get; set; }

        [Required]
        [StringLength(100)]
        public string Category { get; set; }

        [StringLength(2000)]
        public string Specifications { get; set; }

        [Required]
        [StringLength(20)]
        public string Status { get; set; } = "active";

        [StringLength(500)]
        public string Standards { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public virtual ICollection<ProductImage> Images { get; set; } = new List<ProductImage>();
    }
}
