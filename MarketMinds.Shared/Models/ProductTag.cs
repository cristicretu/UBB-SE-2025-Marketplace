using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MarketMinds.Shared.Models
{
    [Table("ProductTags")]
    public class ProductTag
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("title")]
        public string Title { get; set; } = string.Empty;

        // DisplayTitle property for backward compatibility
        [NotMapped]
        public string DisplayTitle
        {
            get { return Title; }
            set { Title = value; }
        }

        // Constructors
        public ProductTag(int id, string displayTitle)
        {
            this.Id = id;
            this.Title = displayTitle;
        }

        // Default constructor for potential framework needs (e.g., deserialization)
        public ProductTag()
        {
            // Empty
        }
    }
}