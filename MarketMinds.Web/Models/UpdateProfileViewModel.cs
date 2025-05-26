using System.ComponentModel.DataAnnotations;

namespace WebMarketplace.Models
{
    public class UpdateProfileViewModel
    {
        [Required(ErrorMessage = "Username is required")]
        [StringLength(50, ErrorMessage = "Username cannot exceed 50 characters")]
        public string Username { get; set; }

        [Required(ErrorMessage = "Store name is required")]
        [StringLength(100, ErrorMessage = "Store name cannot exceed 100 characters")]
        public string StoreName { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Phone number is required")]
        [Phone(ErrorMessage = "Please enter a valid phone number")]
        public string PhoneNumber { get; set; }

        [Required(ErrorMessage = "Address is required")]
        [StringLength(200, ErrorMessage = "Address cannot exceed 200 characters")]
        public string Address { get; set; }

        [Required(ErrorMessage = "Description is required")]
        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
        public string Description { get; set; }

        // Error messages
        public string StoreNameError { get; set; }
        public string EmailError { get; set; }
        public string PhoneNumberError { get; set; }
        public string AddressError { get; set; }
        public string DescriptionError { get; set; }
    }
}
