namespace WebMarketplace.Models
{
    /// <summary>
    /// Interface for the buyer profile view model
    /// </summary>
    public interface IBuyerProfileViewModel
    {
        /// <summary>
        /// Gets or sets the buyer ID
        /// </summary>
        int BuyerId { get; set; }

        /// <summary>
        /// Gets or sets the first name
        /// </summary>
        string FirstName { get; set; }

        /// <summary>
        /// Gets or sets the last name
        /// </summary>
        string LastName { get; set; }

        /// <summary>
        /// Gets or sets the email
        /// </summary>
        string Email { get; set; }

        /// <summary>
        /// Gets or sets the phone number
        /// </summary>
        string PhoneNumber { get; set; }

        /// <summary>
        /// Gets or sets the billing address street
        /// </summary>
        string BillingStreet { get; set; }

        /// <summary>
        /// Gets or sets the billing address city
        /// </summary>
        string BillingCity { get; set; }

        /// <summary>
        /// Gets or sets the billing address country
        /// </summary>
        string BillingCountry { get; set; }

        /// <summary>
        /// Gets or sets the billing address postal code
        /// </summary>
        string BillingPostalCode { get; set; }

        /// <summary>
        /// Gets or sets the shipping address street
        /// </summary>
        string ShippingStreet { get; set; }

        /// <summary>
        /// Gets or sets the shipping address city
        /// </summary>
        string ShippingCity { get; set; }

        /// <summary>
        /// Gets or sets the shipping address country
        /// </summary>
        string ShippingCountry { get; set; }

        /// <summary>
        /// Gets or sets the shipping address postal code
        /// </summary>
        string ShippingPostalCode { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether shipping address is the same as billing address
        /// </summary>
        bool UseSameAddress { get; set; }

        /// <summary>
        /// Gets or sets the buyer badge
        /// </summary>
        string Badge { get; set; }

        /// <summary>
        /// Gets or sets the buyer discount
        /// </summary>
        decimal Discount { get; set; }
    }
}
