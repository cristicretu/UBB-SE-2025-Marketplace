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

        // Simple class for image URLs - matches anonymous type in service
        public class ImageInfo
        {
            [JsonPropertyName("url")]
            public string Url { get; set; }
        }
    }
}