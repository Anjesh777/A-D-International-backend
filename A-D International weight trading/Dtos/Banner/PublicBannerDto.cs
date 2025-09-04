namespace A_D_International_weight_trading.Dtos.Banner
{
    public class PublicBannerDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Subtitle { get; set; }
        public string ImageUrl { get; set; }
        public string ButtonText { get; set; }
        public string LinkType { get; set; }
        public int? ProductId { get; set; }
        public int? CategoryId { get; set; }
        public string ExternalUrl { get; set; }
        public int DisplayOrder { get; set; }
    }
}
