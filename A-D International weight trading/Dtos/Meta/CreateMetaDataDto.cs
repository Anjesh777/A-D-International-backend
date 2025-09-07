using System.ComponentModel.DataAnnotations;

namespace A_D_International_weight_trading.Dtos.Meta
{
    public class CreateMetaDataDto
    {
        [Required]
        [StringLength(250)]
        public string CompanyName { get; set; }

        [Required]
        [StringLength(500)]
        public string Address { get; set; }

        [Required]
        [StringLength(20)]
        [Phone]
        public string Phone { get; set; }

        [Required]
        [StringLength(100)]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [StringLength(200)]
        public string Hours { get; set; }

        [Required]
        [Range(-90, 90)]
        public double Latitude { get; set; }

        [Required]
        [Range(-180, 180)]
        public double Longitude { get; set; }

        [Required]
        [StringLength(2000)]
        public string MapEmbedUrl { get; set; }

        [StringLength(500)]
        public string LocationDescription { get; set; }
    }
}
