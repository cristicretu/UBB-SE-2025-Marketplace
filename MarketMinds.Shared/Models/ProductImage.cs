using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MarketMinds.Shared.Models
{
    [Table("AuctionProductsImages")]
    public class ProductImage
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("product_id")]
        public int ProductId { get; set; }

        [Column("url")]
        [Required]
        public string Url { get; set; } = null!;

        // Navigation property
        [ForeignKey("ProductId")]
        public AuctionProduct? Product { get; set; }
    }
}