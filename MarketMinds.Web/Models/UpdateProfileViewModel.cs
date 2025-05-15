using System.ComponentModel.DataAnnotations;

namespace WebMarketplace.Models
{
    public class UpdateProfileViewModel
    {
        public string Username { get; set; }

        public string StoreName { get; set; }

        [EmailAddress(ErrorMessage = "Please enter a valid email address")]
        public string Email { get; set; }

        [Phone(ErrorMessage = "Please enter a valid phone number")]
        public string PhoneNumber { get; set; }

        public string Address { get; set; }

        public string Description { get; set; }

        // Error messages
        public string StoreNameError { get; set; }
        public string EmailError { get; set; }
        public string PhoneNumberError { get; set; }
        public string AddressError { get; set; }
        public string DescriptionError { get; set; }
    }
}
