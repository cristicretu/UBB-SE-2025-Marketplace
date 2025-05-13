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
    [Table("Baskets")]
    public class Basket
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("buyer_id")]
        public int BuyerId { get; set; }

        [NotMapped]
        public List<BasketItem> Items { get; set; } = new List<BasketItem>();

        public Basket(int id)
        {
            this.Id = id;
            this.Items = new List<BasketItem>();
        }

        // Default constructor for Entity Framework
        public Basket()
        {
            this.Id = 0;
            this.Items = new List<BasketItem>();
        }
    }
}
