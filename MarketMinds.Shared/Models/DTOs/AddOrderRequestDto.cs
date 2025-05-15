namespace SharedClassLibrary.DataTransferObjects
{
    public class AddOrderRequestDto
    {
        public int ProductId { get; set; }
        public int BuyerId { get; set; }
        public required string ProductType { get; set; }
        public required string PaymentMethod { get; set; }
        public int OrderSummaryId { get; set; }
        public DateTime OrderDate { get; set; }
    }
}