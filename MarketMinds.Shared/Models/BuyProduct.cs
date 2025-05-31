using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace MarketMinds.Shared.Models
{
    [Table("BuyProducts")]
    public class BuyProduct : Product
    {
        [Column("price")]
        [JsonPropertyName("price")]
        public override double Price { get; set; }

        [Column("stock")]
        [JsonPropertyName("stock")]
        public int Stock { get; set; }

        // Additional navigation properties specific to BuyProduct
        [JsonPropertyName("images")]
        public ICollection<BuyProductImage> Images { get; set; } = new List<BuyProductImage>();

        [JsonPropertyName("productTags")]
        public ICollection<BuyProductProductTag> ProductTags { get; set; } = new List<BuyProductProductTag>();

        [NotMapped]
        [JsonIgnore]
        public List<Image> NonMappedImages { get; set; }

        public BuyProduct() : base()
        {
            Price = 0;
            Stock = 1;
            NonMappedImages = new List<Image>();
        }

        public BuyProduct(int id, string title, string description, User seller, Condition productCondition, Category productCategory,
            List<ProductTag> productTags, List<Image> images, double price, int stock = 1) : base()
        {
            Id = id;
            Title = title;
            Description = description;
            Seller = seller;
            SellerId = seller.Id;
            Condition = productCondition;
            Category = productCategory;
            Tags = productTags ?? new List<ProductTag>();
            NonMappedImages = images ?? new List<Image>();
            Price = price;
            Stock = stock;
        }

        public BuyProduct(string title, string? description, int sellerId, int? conditionId,
            int? categoryId, double price, int stock = 1) : base()
        {
            Title = title;
            Description = description ?? string.Empty;
            SellerId = sellerId;
            ConditionId = conditionId ?? 0;
            CategoryId = categoryId ?? 0;
            Price = price;
            Stock = stock;
            NonMappedImages = new List<Image>();
        }
    }
}
