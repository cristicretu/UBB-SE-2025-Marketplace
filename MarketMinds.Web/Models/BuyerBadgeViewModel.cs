using System.ComponentModel.DataAnnotations;

namespace WebMarketplace.Models
{
    public class BuyerBadgeViewModel
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "Badge Name is required")]
        [StringLength(50)]
        public string BadgeName { get; set; }
        [Required(ErrorMessage = "Discount is required")]
        [Range(0, 100, ErrorMessage = "Discount must be between 0 and 100")]
        public decimal Discount { get; set; }
        [Required(ErrorMessage = "Progress is required")]
        [Range(0, 100, ErrorMessage = "Progress must be between 0 and 100")]
        public int Progress { get; set; }
        [Required(ErrorMessage = "Image Source is required")]
        [StringLength(200)]
        public string ImageSource { get; set; }
    }
}
