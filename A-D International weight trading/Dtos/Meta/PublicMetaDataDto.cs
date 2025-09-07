namespace A_D_International_weight_trading.Dtos.Meta
{
    public class PublicMetaDataDto
    {
        public string Name { get; set; }
        public string Address { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string Hours { get; set; }
        public CoordinatesDto Coordinates { get; set; }
        public string MapEmbedUrl { get; set; }
        public string LocationDescription { get; set; }

        // Computed URLs for frontend
        public string DirectionsUrl { get; set; }
        public string PhoneCallUrl { get; set; }
        public string EmailUrl { get; set; }
    }
}
