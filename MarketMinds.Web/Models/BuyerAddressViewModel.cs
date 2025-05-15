using System.ComponentModel.DataAnnotations;

namespace WebMarketplace.Models
{
    public class BuyerAddressViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Street Name is required")]
        [StringLength(100)]
        public string StreetLine { get; set; }

        [Required(ErrorMessage = "City is required")]
        [StringLength(50)]
        public string City { get; set; }

        [Required(ErrorMessage = "Country is required")]
        [StringLength(50)]
        public string Country { get; set; }

        [Required(ErrorMessage = "Postal Code is required")]
        [StringLength(20)]
        public string PostalCode { get; set; }
    }
}


//using SharedClassLibrary.Domain;

//namespace WebMarketplace.Models
//{
//    public class BuyerAddressViewModel
//    {
//        public Address ShippingAddress { get; set; }
//        public Address BillingAddress { get; set; }
//    }
//}
