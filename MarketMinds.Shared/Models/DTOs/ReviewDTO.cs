namespace MarketMinds.Shared.Models.DTOs
{
    public class ReviewDTO
    {
        public int Id { get; set; }
        public string Description { get; set; }
        public double Rating { get; set; }
        public int SellerId { get; set; }
        public int BuyerId { get; set; }
        public List<ImageDTO> Images { get; set; } = new List<ImageDTO>();
    }
}