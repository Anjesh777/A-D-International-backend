namespace A_D_International_weight_trading.Dtos.Products
{
    public class ProductListDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } 
        public string Status { get; set; }
        public string Standards { get; set; }
        public bool IsHot { get; set; } 

        public DateTime CreatedAt { get; set; }
        public int ImageCount { get; set; }
        public List<ProductImageDto> Images { get; set; } = new List<ProductImageDto>();

    }
}