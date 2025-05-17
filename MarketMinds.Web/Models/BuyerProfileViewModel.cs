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
        /// Gets or sets the phone number
        /// </summary>
        public string PhoneNumber { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the billing address street
        /// </summary>
        public string BillingStreet { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the billing address city
        /// </summary>
        public string BillingCity { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the billing address country
        /// </summary>
        public string BillingCountry { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the billing address postal code
        /// </summary>
        public string BillingPostalCode { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the shipping address street
        /// </summary>
        public string ShippingStreet { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the shipping address city
        /// </summary>
        public string ShippingCity { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the shipping address country
        /// </summary>
        public string ShippingCountry { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the shipping address postal code
        /// </summary>
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
    }
}
