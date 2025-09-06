namespace A_D_International_weight_trading.Dtos.Banner
{
    public class BannerResponseDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Subtitle { get; set; }
        public string ImageUrl { get; set; }
        public string ButtonText { get; set; }
        public string LinkType { get; set; }
        public int? ProductId { get; set; }
        public string? ProductName { get; set; }
        public string ExternalUrl { get; set; }
        public string Status { get; set; }
        public int DisplayOrder { get; set; }
        public bool IsActive { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
