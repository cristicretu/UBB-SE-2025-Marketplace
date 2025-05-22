using MarketMinds.Shared.Models;
using System.Collections.Generic;
using System.Linq;
using System;

namespace WebMarketplace.Models
{
    public class FinalizePurchaseViewModel
    {
        public int OrderHistoryID { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string Address { get; set; }
        public string PaymentMethod { get; set; }
        public string OrderStatus { get; set; } = "Pending";

        public List<Product> ProductList { get; set; } = new List<Product>();
        public double Subtotal { get; set; }
        public double DeliveryFee { get; set; }
        public double Total { get; set; }

        public FinalizePurchaseViewModel()
        {
        }

        public FinalizePurchaseViewModel(int orderHistoryID) : this()
        {
            OrderHistoryID = orderHistoryID;
        }

        public void CalculateOrderTotal()
        {
            if (ProductList == null || !ProductList.Any())
            {
                Total = 0;
                Subtotal = 0;
                DeliveryFee = 0;
                return;
            }

            double subtotalProducts = 0;
            foreach (var product in ProductList)
            {
                if (product is BuyProduct buyProduct)
                {
                    subtotalProducts += buyProduct.Price;
                }
                else if (product is BorrowProduct borrowProduct)
                {
                    subtotalProducts += product.Price;
                }
                else
                {
                    subtotalProducts += product.Price;
                }
            }

            Subtotal = subtotalProducts;

            bool hasSpecialType = ProductList.Any(p =>
                (p is BorrowProduct) ||
                (p.GetType().Name.Contains("Refill")) ||
                (p.GetType().Name.Contains("Auction")));

            if (subtotalProducts >= 200 || hasSpecialType)
            {
                DeliveryFee = 0;
                Total = subtotalProducts;
            }
            else
            {
                DeliveryFee = 13.99;
                Total = subtotalProducts + DeliveryFee;
            }
        }
    }
}