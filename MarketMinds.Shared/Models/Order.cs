using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MarketMinds.Shared.Models
{
    [Table("Orders")]
    public class Order
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = null!;

        public string? Description { get; set; }

        [Required]
        public double Cost { get; set; }

        [Required]
        public int SellerId { get; set; }

        [Required]
        public int BuyerId { get; set; } // Required, default -1 handled by DB

        [ForeignKey("SellerId")]
        public virtual User Seller { get; set; } = null!; // Navigation property for the seller

        // Cannot map Buyer directly as FK if BuyerId can be -1
        // We can still query by BuyerId, just not use a direct navigation property easily.

        // Note: The 'Created' property used in the original client service is missing
        // from the DB schema. Sorting will be done by Id descending as a proxy.
    }
}