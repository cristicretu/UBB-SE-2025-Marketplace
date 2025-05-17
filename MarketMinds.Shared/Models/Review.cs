using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MarketMinds.Shared.Models
{
    [Table("Reviews")]
    public class Review
    {
        // The review doesn't take into account the product for which the review has been made,
        // it can be mentioned in the description, and images
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("description")]
        public string Description { get; set; } = string.Empty;

        [NotMapped] // This property is not directly mapped to a column in the Reviews table
        public List<Image> Images { get; set; } = new List<Image>();

        [Column("rating")]
        public double Rating { get; set; }

        [Column("seller_id")]
        public int SellerId { get; set; }

        [Column("reviewer_id")]
        public int BuyerId { get; set; }

        [Column("score")]
        public int Score { get; set; }

        [NotMapped]
        public string SellerUsername { get; set; } = string.Empty;

        [NotMapped]
        public string BuyerUsername { get; set; } = string.Empty;

        // Navigation property for the join table
        [InverseProperty("Review")]
        public virtual ICollection<ReviewImage> ReviewImages { get; set; } = new List<ReviewImage>();

        public Review(int id, string description, List<Image> images, double rating, int sellerId, int buyerId)
        {
            this.Id = id;
            this.Description = description;
            this.Images = images ?? new List<Image>();
            this.Rating = rating;
            this.SellerId = sellerId;
            this.BuyerId = buyerId;
            this.SellerUsername = string.Empty;
            this.BuyerUsername = string.Empty;
        }

        // Default constructor for Entity Framework
        public Review()
        {
            Images = new List<Image>();
            ReviewImages = new List<ReviewImage>();
            SellerUsername = string.Empty;
            BuyerUsername = string.Empty;
        }

        // Helper method to sync Images and ReviewImages collections before saving to DB
        public void SyncImagesBeforeSave()
        {
            if (Images != null && Id > 0)
            {
                // Clear existing ReviewImages and recreate from Images
                ReviewImages.Clear();
                foreach (var image in Images)
                {
                    ReviewImages.Add(ReviewImage.FromImage(image, Id));
                }
            }
        }

        // Helper method to populate Images collection from ReviewImages after loading from DB
        public void LoadGenericImages()
        {
            if (ReviewImages != null)
            {
                Images.Clear();
                foreach (var reviewImage in ReviewImages)
                {
                    Images.Add(reviewImage.ToImage());
                }
            }
        }
    }
}