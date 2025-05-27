using System.Diagnostics.CodeAnalysis;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MarketMinds.Shared.Models
{
    [ExcludeFromCodeCoverage]
    [Table("OrderHistory")]
    public class OrderHistory
    {
        [Key]
        public int OrderID { get; set; }

        // Collection of order IDs associated with this order history
        [NotMapped]
        public List<int> OrderIDs { get; set; } = new List<int>();

        // User who placed the orders
        public int BuyerID { get; set; }

        // When the order history was created
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Additional metadata
        public string? Note { get; set; }

        // Shipping information
        public string? ShippingAddress { get; set; }

        // Payment information
        public string? PaymentMethod { get; set; }
    }
}
