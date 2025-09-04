using System.ComponentModel.DataAnnotations;

namespace A_D_International_weight_trading.Dtos.Banner
{
    public class CreateBannerDto
    {
        [Required]
        [StringLength(200)]
        public string Title { get; set; }

        [StringLength(500)]
        public string Subtitle { get; set; }

        [Required]
        public IFormFile Image { get; set; }

        [StringLength(50)]
        public string ButtonText { get; set; } = "Shop Now";

        [Required]
        [StringLength(20)]
        public string LinkType { get; set; } = "product";

        public int? ProductId { get; set; }

        public int? CategoryId { get; set; }

        [StringLength(500)]
        public string ExternalUrl { get; set; }

        [StringLength(20)]
        public string Status { get; set; } = "active";

        public int DisplayOrder { get; set; } = 0;

        public bool IsActive { get; set; } = true;

        public DateTime StartDate { get; set; } = DateTime.UtcNow;

        public DateTime? EndDate { get; set; }
    }
}
