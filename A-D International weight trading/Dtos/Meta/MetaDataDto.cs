namespace A_D_International_weight_trading.Dtos.Meta
{
    public class MetaDataDto
    {
        public int Id { get; set; }
        public string CompanyName { get; set; }
        public string Address { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string Hours { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string MapEmbedUrl { get; set; }
        public string LocationDescription { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string DirectionsUrl { get; set; }
        public string PhoneCallUrl { get; set; }
        public string EmailUrl { get; set; }
    }
}
