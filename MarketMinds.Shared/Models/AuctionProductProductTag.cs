using System;

namespace MarketMinds.Shared.Models
{
    public class AuctionProductProductTag
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public int TagId { get; set; }

        // Navigation properties (commented out to avoid circular reference during compilation)
        // public AuctionProduct Product { get; set; }
        public ProductTag Tag { get; set; }

        public AuctionProductProductTag()
        {
        }

        public AuctionProductProductTag(int id, int productId, int tagId)
        {
            Id = id;
            ProductId = productId;
            TagId = tagId;
        }
    }
}