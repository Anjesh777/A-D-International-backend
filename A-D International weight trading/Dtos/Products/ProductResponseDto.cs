namespace A_D_International_weight_trading.Dtos.Products
{
    public class ProductResponseDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } 
        public string Specifications { get; set; }
        public string Status { get; set; }
        public string Standards { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public List<ProductImageDto> Images { get; set; } = new List<ProductImageDto>();
    }
}
