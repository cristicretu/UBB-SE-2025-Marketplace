using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MarketMinds.Shared.Models;

namespace MarketMinds.Shared.Models
{
    [Table("BuyProducts")]
    public class BuyProduct : Product
    {
        [Column("price")]
        public double Price { get; set; }

        // Additional navigation properties specific to BuyProduct
        public ICollection<BuyProductImage> Images { get; set; } = new List<BuyProductImage>();
        public ICollection<BuyProductProductTag> ProductTags { get; set; } = new List<BuyProductProductTag>();

        [NotMapped]
        public List<Image> NonMappedImages { get; set; }

        public BuyProduct() : base()
        {
            Price = 0;
            NonMappedImages = new List<Image>();
        }

        public BuyProduct(int id, string title, string description, User seller, Condition productCondition, Category productCategory,
            List<ProductTag> productTags, List<Image> images, double price) : base()
        {
            Id = id;
            Title = title;
            Description = description;
            Seller = seller;
            Condition = productCondition;
            Category = productCategory;
            Tags = productTags ?? new List<ProductTag>();
            NonMappedImages = images ?? new List<Image>();
            Price = price;
        }

        public BuyProduct(string title, string? description, int sellerId, int? conditionId,
            int? categoryId, double price) : base()
        {
            Title = title;
            Description = description ?? string.Empty;
            SellerId = sellerId;
            ConditionId = conditionId ?? 0;
            CategoryId = categoryId ?? 0;
            Price = price;
            NonMappedImages = new List<Image>();
        }
    }
}
