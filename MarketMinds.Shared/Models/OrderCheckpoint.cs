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
        PROCESSING = 0,
        
        /// <summary>
        /// Order has been shipped.
        /// </summary>
        SHIPPED = 1,
        
        /// <summary>
        /// Order is in warehouse awaiting shipment.
        /// </summary>
        IN_WAREHOUSE = 2,
        
        /// <summary>
        /// Order is being transported to its destination.
        /// </summary>
        IN_TRANSIT = 3,
        
        /// <summary>
        /// Order is out for delivery.
        /// </summary>
        OUT_FOR_DELIVERY = 4,
        
        /// <summary>
        /// Order has been successfully delivered to the customer.
        /// </summary>
        DELIVERED = 5,
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
