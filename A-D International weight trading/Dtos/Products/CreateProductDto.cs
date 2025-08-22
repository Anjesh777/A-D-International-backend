using System.ComponentModel.DataAnnotations;

namespace A_D_International_weight_trading.Dtos.Products
{
    public class CreateProductDto
    {
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

        [StringLength(20)]
        public string Status { get; set; } = "active";

        [StringLength(500)]
        public string Standards { get; set; }

        public List<IFormFile> Images { get; set; } = new List<IFormFile>();
    }
}
