namespace A_D_International_weight_trading.Dtos.Products
{
    public class ProductListDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Category { get; set; }
        public string Status { get; set; }
        public string Standards { get; set; }
        public DateTime CreatedAt { get; set; }
        public int ImageCount { get; set; }
    }
}
