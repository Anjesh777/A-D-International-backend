using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace A_D_International_weight_trading.Model
{
    public class MetaData
    {
        public int Id { get; set; }

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

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        [NotMapped]
        public string DirectionsUrl => $"https://www.google.com/maps/dir/?api=1&destination={Latitude},{Longitude}";

        [NotMapped]
        public string PhoneCallUrl => $"tel:{Phone}";

        [NotMapped]
        public string EmailUrl => $"mailto:{Email}";
    }
}