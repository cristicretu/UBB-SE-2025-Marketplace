namespace SharedClassLibrary.DataTransferObjects
{
    public class UpdateOrderRequestDto
    {
        public required string ProductType { get; set; }
        public required string PaymentMethod { get; set; }
        public DateTime OrderDate { get; set; }
    }
}