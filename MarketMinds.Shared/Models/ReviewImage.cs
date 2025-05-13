using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MarketMinds.Shared.Models
{
    [Table("ReviewsPictures")]
    public class ReviewImage
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("url")]
        public string Url { get; set; } = string.Empty;

        [Required]
        [Column("review_id")]
        public int ReviewId { get; set; }

        [ForeignKey("ReviewId")]
        public virtual Review Review { get; set; }

        public ReviewImage()
        {
        }

        public ReviewImage(int id, string url, int reviewId)
        {
            Id = id;
            Url = url;
            ReviewId = reviewId;
        }

        // Create a ReviewImage from an Image
        public static ReviewImage FromImage(Image image, int reviewId)
        {
            return new ReviewImage
            {
                Url = image.Url,
                ReviewId = reviewId
            };
        }

        // Convert to a generic Image
        public Image ToImage()
        {
            return new Image(Url) { Id = this.Id };
        }
    }
}