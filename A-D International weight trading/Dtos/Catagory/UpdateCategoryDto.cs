using System.ComponentModel.DataAnnotations;

namespace A_D_International_weight_trading.Dtos.Catagory
{
    public class UpdateCategoryDto
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [StringLength(500)]
        public string Description { get; set; }

        [StringLength(20)]
        public string Status { get; set; }
    }
}
