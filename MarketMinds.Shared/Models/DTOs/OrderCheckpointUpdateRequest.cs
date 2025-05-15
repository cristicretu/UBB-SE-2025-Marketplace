namespace SharedClassLibrary.DataTransferObjects
{
    using SharedClassLibrary.Domain;

    /// <summary>
    /// Data Transfer Object for updating an OrderCheckpoint.
    /// </summary>
    public class OrderCheckpointUpdateRequest
    {
        public DateTime Timestamp { get; set; }
        public string? Location { get; set; }
        public string Description { get; set; } = string.Empty;
        public OrderStatus Status { get; set; }
    }
}