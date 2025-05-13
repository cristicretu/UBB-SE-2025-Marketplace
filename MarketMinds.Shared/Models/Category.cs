using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MarketMinds.Shared.Models
{
    [Table("ProductCategories")]
    public class Category
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("title")]
        public string Name { get; set; } = null!;

        [Column("description")]
        public string? Description { get; set; }

        [NotMapped]
        public string DisplayTitle => Name;

        // Navigation properties
        public ICollection<AuctionProduct> Products { get; set; }

        public Category()
        {
            Products = new List<AuctionProduct>();
        }

        public Category(int id, string name, string? description = null)
        {
            Id = id;
            Name = name;
            Description = description;
            Products = new List<AuctionProduct>();
        }

        public Category(string name, string? description = null)
        {
            Name = name;
            Description = description;
            Products = new List<AuctionProduct>();
        }
    }
}