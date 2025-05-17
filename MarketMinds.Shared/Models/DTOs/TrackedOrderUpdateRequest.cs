namespace MarketMinds.Shared.Models.DTOs
{
    using MarketMinds.Shared.Models;

    /// <summary>
    /// Data Transfer Object for updating a TrackedOrder.
    /// </summary>
    public class TrackedOrderUpdateRequest
    {
        public DateOnly EstimatedDeliveryDate { get; set; }
        public OrderStatus CurrentStatus { get; set; }
    }
}