using System.ComponentModel.DataAnnotations;

namespace A_D_International_weight_trading.Dtos.Meta
{
    public class UpdateMetaDataDto
    {
        [StringLength(250)]
        public string? CompanyName { get; set; }

        [StringLength(500)]
        public string? Address { get; set; }

        [StringLength(20)]
        [Phone]
        public string? Phone { get; set; }

        [StringLength(100)]
        [EmailAddress]
        public string? Email { get; set; }

        [StringLength(200)]
        public string? Hours { get; set; }

        [Range(-90, 90)]
        public double? Latitude { get; set; }

        [Range(-180, 180)]
        public double? Longitude { get; set; }

        [StringLength(2000)]
        public string? MapEmbedUrl { get; set; }

        [StringLength(500)]
        public string? LocationDescription { get; set; }
    }
}
