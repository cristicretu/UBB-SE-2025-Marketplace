using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MarketMinds.Shared.Models
{
    [Table("BuyProductImages")]
    public class BuyProductImage
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("url")]
        public string Url { get; set; } = string.Empty;

        [Column("product_id")]
        public int ProductId { get; set; }

        // Navigation property
        public BuyProduct? Product { get; set; }

        public BuyProductImage()
        {
            // Default constructor
        }

        public BuyProductImage(int id, string url, int productId)
        {
            Id = id;
            Url = url;
            ProductId = productId;
        }
    }
}