namespace MarketMinds.Shared.Models.DTOs
{
    public class BuyProductDTO
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public double Price { get; set; }
        public ConditionDTO Condition { get; set; }
        public CategoryDTO Category { get; set; }
        public List<ProductTagDTO> Tags { get; set; } = new List<ProductTagDTO>();
        public List<ImageDTO> Images { get; set; } = new List<ImageDTO>();
        public UserDTO Seller { get; set; }
    }
}