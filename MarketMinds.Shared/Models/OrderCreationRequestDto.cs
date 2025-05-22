using System.Collections.Generic;

namespace MarketMinds.Shared.Models
{
    public class OrderCreationRequestDto
    {
        public double Subtotal { get; set; }
        public double WarrantyTax { get; set; }
        public double DeliveryFee { get; set; }
        public double Total { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string Address { get; set; }
        public string ZipCode { get; set; }
        public string AdditionalInfo { get; set; }
        public string SelectedPaymentMethod { get; set; }
        // ProductList will be passed as a separate parameter to the service method
    }
} 