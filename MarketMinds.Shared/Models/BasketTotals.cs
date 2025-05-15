namespace MarketMinds.Shared.Models
{
    public class BasketTotals
    {
        public double Subtotal { get; set; }
        public double Discount { get; set; }
        public double TotalAmount { get; set; }

        public BasketTotals()
        {
            Subtotal = 0;
            Discount = 0;
            TotalAmount = 0;
        }
    }
}