using System.Collections.Generic;

namespace MarketMinds.Shared.Models.DTOs
{
    public class BasketDTO
    {
        public int Id { get; set; }
        public int BuyerId { get; set; }
        public List<BasketItemDTO> Items { get; set; } = new List<BasketItemDTO>();
    }

    public class BasketItemDTO
    {
        public int Id { get; set; }
        public int BasketId { get; set; }
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public double Price { get; set; }
        public ProductDTO Product { get; set; }

        public double GetPrice()
        {
            return Price * Quantity;
        }
    }

    public class ProductDTO
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public double Price { get; set; }
        public UserDTO Seller { get; set; }
        public ConditionDTO Condition { get; set; }
        public CategoryDTO Category { get; set; }
        public List<ProductTagDTO> Tags { get; set; } = new List<ProductTagDTO>();
        public List<ImageDTO> Images { get; set; } = new List<ImageDTO>();
    }

    public class BasketTotalsDTO
    {
        public double Subtotal { get; set; }
        public double Discount { get; set; }
        public double TotalAmount { get; set; }
    }
}