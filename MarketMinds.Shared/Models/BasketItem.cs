using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MarketMinds.Shared.Models;

namespace MarketMinds.Shared.Models
{
    [Table("BasketItemsBuyProducts")]
    public class BasketItem
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("basket_id")]
        public int BasketId { get; set; }

        [Column("product_id")]
        public int ProductId { get; set; }

        // Navigation property is handled through Entity Framework configuration
        [NotMapped]
        public BuyProduct Product { get; set; }

        [Column("quantity")]
        public int Quantity { get; set; }

        [Column("price")]
        public double Price { get; set; }

        private const double DEFAULT_PRICE = 0;

        public BasketItem(int id, BuyProduct product, int quantity)
        {
            this.Id = id;
            this.Product = product;
            this.ProductId = product.Id;
            this.Quantity = quantity;
            this.Price = product.Price;
        }

        public BasketItem()
        {
            this.Id = 0;
            this.Product = null;
            this.ProductId = 0;
            this.Quantity = 0;
            this.Price = DEFAULT_PRICE;
        }

        public double GetPrice()
        {
            // Calculates the total price for this basket item (unit price × quantity)
            return (Price * Quantity);
        }
    }
}
