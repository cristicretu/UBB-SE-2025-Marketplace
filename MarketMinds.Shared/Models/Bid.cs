using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MarketMinds.Shared.Models
{
    [Table("Bids")]
    public class Bid
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("bidder_id")]
        public int BidderId { get; set; }

        [Column("product_id")]
        public int ProductId { get; set; }

        [Column("price")]
        public double Price { get; set; }

        [Column("timestamp")]
        public DateTime Timestamp { get; set; }

        // Navigation property
        [ForeignKey("ProductId")]
        public AuctionProduct Product { get; set; } = null!;

        [ForeignKey("BidderId")]
        public User Bidder { get; set; } = null!;

        // Default constructor for EF Core
        public Bid()
        {
            // Default constructor
        }

        public Bid(int bidderId, int productId, double price)
        {
            BidderId = bidderId;
            ProductId = productId;
            Price = price;
            Timestamp = DateTime.UtcNow;
        }
    }
}