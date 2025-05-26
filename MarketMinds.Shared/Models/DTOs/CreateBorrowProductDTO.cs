using System.Text.Json.Serialization;

namespace MarketMinds.Shared.Models.DTOs
{
    public class CreateBorrowProductDTO
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public int SellerId { get; set; }
        public int? ConditionId { get; set; }
        public int? CategoryId { get; set; }
        public DateTime TimeLimit { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public double DailyRate { get; set; }
        public bool IsBorrowed { get; set; }

        // Serialize images as simple objects with URL property
        [JsonPropertyName("images")]
        public List<ImageInfo> Images { get; set; } = new List<ImageInfo>();

        // Add Tags property to support tag creation
        [JsonPropertyName("tags")]
        public List<ProductTagInfo> Tags { get; set; } = new List<ProductTagInfo>();

        // Simple class for image URLs - matches anonymous type in service
        public class ImageInfo
        {
            [JsonPropertyName("url")]
            public string Url { get; set; }
        }

        // Simple class for tag information
        public class ProductTagInfo
        {
            [JsonPropertyName("id")]
            public int Id { get; set; }

            [JsonPropertyName("title")]
            public string Title { get; set; }
        }
    }
}