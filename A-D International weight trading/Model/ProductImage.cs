namespace A_D_International_weight_trading.Model
{
    public class ProductImage
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string ImageUrl { get; set; }
        public string PublicId { get; set; } 
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public virtual Product Product { get; set; }
    }
}
