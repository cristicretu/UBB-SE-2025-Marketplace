namespace MarketMinds.Shared.Models.DTOs
{
    public class AuctionProductDTO
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime StartAuctionDate { get; set; }
        public DateTime EndAuctionDate { get; set; }
        public double StartingPrice { get; set; }
        public double CurrentPrice { get; set; }
        public List<BidDTO> BidHistory { get; set; } = new List<BidDTO>();
        public ConditionDTO Condition { get; set; }
        public CategoryDTO Category { get; set; }
        public List<ProductTagDTO> Tags { get; set; } = new List<ProductTagDTO>();
        public List<ImageDTO> Images { get; set; } = new List<ImageDTO>();
        public UserDTO Seller { get; set; }
    }

    public class BidDTO
    {
        public int Id { get; set; }
        public double Price { get; set; }
        public DateTime Timestamp { get; set; }
        public UserDTO Bidder { get; set; }
    }

    public class ConditionDTO
    {
        public int Id { get; set; }
        public string DisplayTitle { get; set; }
        public string Description { get; set; }
    }

    public class CategoryDTO
    {
        public int Id { get; set; }
        public string DisplayTitle { get; set; }
        public string Description { get; set; }
    }

    public class ProductTagDTO
    {
        public int Id { get; set; }
        public string DisplayTitle { get; set; }
    }

    public class ImageDTO
    {
        public string Url { get; set; }
    }

    public class UserDTO
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public int UserType { get; set; }
        public double Balance { get; set; }
        public double Rating { get; set; }
        public int Password { get; set; }
    }

    public class CreateBidDTO
    {
        public int ProductId { get; set; }
        public int BidderId { get; set; }
        public double Amount { get; set; }
        public DateTime Timestamp { get; set; }
    }
}