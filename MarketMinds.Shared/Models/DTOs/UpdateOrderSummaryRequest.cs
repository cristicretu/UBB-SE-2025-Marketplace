namespace SharedClassLibrary.DataTransferObjects
{
    /// <summary>
    /// DTO for updating an order summary.
    /// </summary>
    public class UpdateOrderSummaryRequest
    {
        public int Id { get; set; }

        public double Subtotal { get; set; }

        public double WarrantyTax { get; set; }

        public double DeliveryFee { get; set; }

        public double FinalTotal { get; set; }

        public required string FullName { get; set; }

        public required string Email { get; set; }

        public required string PhoneNumber { get; set; }

        public required string Address { get; set; }

        public required string PostalCode { get; set; }

        public required string AdditionalInfo { get; set; }

        public required string ContractDetails { get; set; }
    }
}
