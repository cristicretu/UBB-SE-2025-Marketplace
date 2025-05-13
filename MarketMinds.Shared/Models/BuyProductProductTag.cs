using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MarketMinds.Shared.Models
{
    [Table("BuyProductProductTags")]
    public class BuyProductProductTag
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("product_id")]
        public int ProductId { get; set; }

        [Column("tag_id")]
        public int TagId { get; set; }

        [ForeignKey("ProductId")]
        public virtual BuyProduct Product { get; set; }

        [ForeignKey("TagId")]
        public virtual ProductTag Tag { get; set; }

        public BuyProductProductTag()
        {
            // Default constructor
        }

        public BuyProductProductTag(int id, int productId, int tagId)
        {
            Id = id;
            ProductId = productId;
            TagId = tagId;
        }
    }
}