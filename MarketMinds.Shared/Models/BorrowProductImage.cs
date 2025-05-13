using System.Text.Json.Serialization;

namespace MarketMinds.Shared.Models
{
    public class BorrowProductImage
    {
        public int Id { get; set; }
        public string Url { get; set; } = string.Empty;
        public int ProductId { get; set; }

        // Navigation property
        [JsonIgnore] // Break circular reference during serialization
        public BorrowProduct? Product { get; set; }

        public BorrowProductImage()
        {
            // Default constructor
        }

        public BorrowProductImage(int id, string url, int productId)
        {
            Id = id;
            Url = url;
            ProductId = productId;
        }
    }
}