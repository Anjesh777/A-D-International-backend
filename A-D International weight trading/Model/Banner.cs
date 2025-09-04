using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace A_D_International_weight_trading.Model
{
    public class Banner
    {
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; }

        [StringLength(500)]
        public string Subtitle { get; set; }

        [Required]
        [StringLength(500)]
        public string ImageUrl { get; set; }

        [StringLength(100)]
        public string PublicId { get; set; } 

        [StringLength(50)]
        public string ButtonText { get; set; } = "Shop Now";

        [StringLength(20)]
        public string LinkType { get; set; } = "product";
        public int? ProductId { get; set; }
        public int? CategoryId { get; set; }

        [StringLength(500)]
        public string ExternalUrl { get; set; }

        [Required]
        [StringLength(20)]
        public string Status { get; set; } = "active";

        public int DisplayOrder { get; set; } = 0;

        public bool IsActive { get; set; } = true;

        public DateTime StartDate { get; set; } = DateTime.UtcNow;

        public DateTime? EndDate { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey("ProductId")]
        public virtual Product? Product { get; set; }

        [ForeignKey("CategoryId")]
        public virtual Category? Category { get; set; }

    }
}
