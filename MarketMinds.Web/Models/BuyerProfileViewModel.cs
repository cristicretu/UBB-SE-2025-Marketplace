using System.ComponentModel.DataAnnotations;
using MarketMinds.Shared.Models;

namespace WebMarketplace.Models
{
    /// <summary>
    /// View model for the buyer profile
    /// </summary>
    public class BuyerProfileViewModel : IBuyerProfileViewModel
    {
        /// <summary>
        /// Gets or sets the buyer ID
        /// </summary>
        public int BuyerId { get; set; }

        /// <summary>
        /// Gets or sets the first name
        /// </summary>
        [Required(ErrorMessage = "First name is required")]
        [StringLength(50, ErrorMessage = "First name cannot exceed 50 characters")]
        public string FirstName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the last name
        /// </summary>
        [Required(ErrorMessage = "Last name is required")]
        [StringLength(50, ErrorMessage = "Last name cannot exceed 50 characters")]
        public string LastName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the email
        /// </summary>
        [EmailAddress(ErrorMessage = "Please enter a valid email address")]
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the phone number
        /// </summary>
        [Required(ErrorMessage = "Phone number is required")]
        [Phone(ErrorMessage = "Please enter a valid phone number")]
        public string PhoneNumber { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the billing address street
        /// </summary>
        [Required(ErrorMessage = "Billing street address is required")]
        [StringLength(100, ErrorMessage = "Street address cannot exceed 100 characters")]
        public string BillingStreet { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the billing address city
        /// </summary>
        [Required(ErrorMessage = "Billing city is required")]
        [StringLength(50, ErrorMessage = "City cannot exceed 50 characters")]
        public string BillingCity { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the billing address country
        /// </summary>
        [Required(ErrorMessage = "Billing country is required")]
        [StringLength(50, ErrorMessage = "Country cannot exceed 50 characters")]
        public string BillingCountry { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the billing address postal code
        /// </summary>
        [Required(ErrorMessage = "Billing postal code is required")]
        [StringLength(20, ErrorMessage = "Postal code cannot exceed 20 characters")]
        public string BillingPostalCode { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the shipping address street
        /// </summary>
        [StringLength(100, ErrorMessage = "Street address cannot exceed 100 characters")]
        public string ShippingStreet { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the shipping address city
        /// </summary>
        [StringLength(50, ErrorMessage = "City cannot exceed 50 characters")]
        public string ShippingCity { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the shipping address country
        /// </summary>
        [StringLength(50, ErrorMessage = "Country cannot exceed 50 characters")]
        public string ShippingCountry { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the shipping address postal code
        /// </summary>
        [StringLength(20, ErrorMessage = "Postal code cannot exceed 20 characters")]
        public string ShippingPostalCode { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets a value indicating whether shipping address is the same as billing address
        /// </summary>
        public bool UseSameAddress { get; set; }

        /// <summary>
        /// Gets or sets the buyer badge
        /// </summary>
        public string Badge { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the buyer discount
        /// </summary>
        public decimal Discount { get; set; }

        /// <summary>
        /// Gets or sets the list of linked buyers
        /// </summary>
        public List<LinkedBuyerInfo> LinkedBuyers { get; set; } = new List<LinkedBuyerInfo>();
    }

    /// <summary>
    /// Information about a linked buyer for display purposes
    /// </summary>
    public class LinkedBuyerInfo
    {
        /// <summary>
        /// Gets or sets the buyer ID
        /// </summary>
        public int BuyerId { get; set; }

        /// <summary>
        /// Gets or sets the first name
        /// </summary>
        public string FirstName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the last name
        /// </summary>
        public string LastName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the email
        /// </summary>
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the badge
        /// </summary>
        public string Badge { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the date when the link was created
        /// </summary>
        public DateTime LinkedDate { get; set; }
    }
}
