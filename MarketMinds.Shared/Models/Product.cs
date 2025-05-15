using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

// TODO: Define or import ProductCondition, ProductCategory, User if they are needed by the server
namespace MarketMinds.Shared.Models // Adjusted namespace to server.Models
{
    public abstract class Product
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("title")]
        public string Title { get; set; } = string.Empty;

        [Column("description")]
        public string Description { get; set; } = string.Empty;

        // Foreign keys
        [Column("seller_id")]
        public int SellerId { get; set; }

        [Column("condition_id")]
        public int ConditionId { get; set; }

        [Column("category_id")]
        public int CategoryId { get; set; }

        // merge-nicusor
        [Column("price")]
        public int Price { get; set; }

        [Column("stock")]
        public int Stock { get; set; }

        // Navigation properties
        [ForeignKey("SellerId")]
        public User? Seller { get; set; }

        [ForeignKey("ConditionId")]
        public Condition? Condition { get; set; }

        [ForeignKey("CategoryId")]
        public Category? Category { get; set; }

        // Tags property - Not mapped to a database column as each product type has its own tags relationship
        [NotMapped]
        public virtual IEnumerable<ProductTag> Tags { get; set; } = new List<ProductTag>();

        // Images property - Will be implemented differently in each derived class
        [NotMapped]
        public virtual IEnumerable<Image> Images { get; set; } = new List<Image>();

        // Default constructor for JSON serialization/deserialization
        protected Product()
        {
            Id = 0;
            Title = string.Empty;
            Description = string.Empty;
            Condition = null;
            Category = null;
            Seller = null;
        }
    }
}