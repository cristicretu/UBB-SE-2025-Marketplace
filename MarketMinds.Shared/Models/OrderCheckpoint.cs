namespace MarketMinds.Shared.Models
{
    /// <summary>
    /// Represents the status of an order in the system.
    /// </summary>
    public enum OrderStatus
    {
        /// <summary>
        /// Order has been received but not yet processed.
        /// </summary>
        Pending = 0,
        
        /// <summary>
        /// Order has been processed and payment confirmed.
        /// </summary>
        PROCESSING = 1,
        
        /// <summary>
        /// Order has been shipped.
        /// </summary>
        SHIPPED = 2,
        
        /// <summary>
        /// Order is in warehouse awaiting shipment.
        /// </summary>
        IN_WAREHOUSE = 3,
        
        /// <summary>
        /// Order is being transported to its destination.
        /// </summary>
        IN_TRANSIT = 4,
        
        /// <summary>
        /// Order is out for delivery.
        /// </summary>
        OUT_FOR_DELIVERY = 5,
        
        /// <summary>
        /// Order has been successfully delivered to the customer.
        /// </summary>
        DELIVERED = 6,
        
        /// <summary>
        /// Order has been cancelled.
        /// </summary>
        Cancelled = 7,
        
        /// <summary>
        /// Order refund has been requested.
        /// </summary>
        RefundRequested = 8,
        
        /// <summary>
        /// Order has been refunded.
        /// </summary>
        Refunded = 9,
        
        /// <summary>
        /// Order has been rejected.
        /// </summary>
        Rejected = 10
    }
    
    public class OrderCheckpoint
    {
        public int CheckpointID { get; set; }
        public DateTime Timestamp { get; set; }
        public string? Location { get; set; }
        public string? Description { get; set; }
        public OrderStatus Status { get; set; }
        public int TrackedOrderID { get; set; }
    }
}
