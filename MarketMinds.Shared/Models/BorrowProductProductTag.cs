using System.Text.Json.Serialization;

namespace MarketMinds.Shared.Models
{
    public class BorrowProductProductTag
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public int TagId { get; set; }

        // Navigation properties
        [JsonIgnore] // Break circular reference during serialization
        public BorrowProduct Product { get; set; }
        public ProductTag Tag { get; set; }

        public BorrowProductProductTag()
        {
            // Default constructor
        }

        public BorrowProductProductTag(int id, int productId, int tagId)
        {
            Id = id;
            ProductId = productId;
            TagId = tagId;
        }
    }
}