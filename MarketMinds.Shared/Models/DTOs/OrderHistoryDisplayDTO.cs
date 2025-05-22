using System.Diagnostics.CodeAnalysis;

namespace MarketMinds.Shared.Models.DTOs
{
    /// <summary>
    /// DTO for displaying order history in a UI-friendly way.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class OrderHistoryDisplayDTO
    {
        /// <summary>
        /// Gets or sets the ID of the order history.
        /// </summary>
        public int OrderHistoryID { get; set; }
        
        /// <summary>
        /// Gets or sets the date the order was placed.
        /// </summary>
        public string OrderDate { get; set; }
        
        /// <summary>
        /// Gets or sets the total number of items in the order.
        /// </summary>
        public int TotalItems { get; set; }
        
        /// <summary>
        /// Gets or sets the total cost of the order.
        /// </summary>
        public double TotalCost { get; set; }
        
        /// <summary>
        /// Gets or sets a list of the most recent statuses for the orders.
        /// </summary>
        public List<OrderStatus> OrderStatuses { get; set; } = new List<OrderStatus>();
        
        /// <summary>
        /// Gets or sets a list of product names included in this order history.
        /// </summary>
        public List<string> ProductNames { get; set; } = new List<string>();
        
        /// <summary>
        /// Gets or sets the shipping address.
        /// </summary>
        public string ShippingAddress { get; set; }
        
        /// <summary>
        /// Gets or sets the payment method.
        /// </summary>
        public string PaymentMethod { get; set; }
    }
} 