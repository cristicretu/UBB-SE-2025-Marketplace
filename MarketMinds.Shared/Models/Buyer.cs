// <copyright file="Buyer.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace SharedClassLibrary.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    /// <summary>
    /// Represents a buyer in the marketplace.
    /// This class extends the base User class with buyer-specific functionality.
    /// </summary>
    /// <seealso cref="User"/>
    /// <seealso cref="BuyerBadge"/>
    /// <seealso cref="BuyerWishlist"/>
    /// <seealso cref="BuyerLinkage"/>
    /// <seealso cref="Address"/>
    public class Buyer
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Buyer"/> class.
        /// </summary>
        public Buyer()
        {
            this.Badge = BuyerBadge.BRONZE;
            this.Wishlist = new BuyerWishlist();
            this.Linkages = new List<BuyerLinkage>();
            this.TotalSpending = 0.0m;
            this.NumberOfPurchases = 0;
            this.Discount = 0.0m;
            this.UseSameAddress = false;
            this.FollowingUsersIds = new List<int>();
            this.User = new User();
            this.FirstName = string.Empty;
            this.LastName = string.Empty;
            this.ShippingAddress = new Address();
            this.BillingAddress = new Address();
            this.SyncedBuyerIds = new List<Buyer>(); // in the application this is not used, so i will not fetch data about it in the server repo - Alex
        }

        /// <summary>
        /// Gets or sets the user associated with this buyer.
        /// </summary>
        public User User { get; set; }

        /// <summary>
        /// Gets or sets the buyer's phone number, hiding the base User.PhoneNumber property.
        /// </summary>
        public string PhoneNumber
        {
            get => this.User.PhoneNumber;
            set => this.User.PhoneNumber = value;
        }

        /// <summary>
        /// Gets or sets the buyer's email address, hiding the base User.Email property.
        /// </summary>
        public string Email
        {
            get => this.User.Email;
            set => this.User.Email = value;
        }

        /// <summary>
        /// Gets the unique identifier of the buyer.
        /// This property is derived from the associated User's ID.
        /// </summary>
        public int Id { get => this.User.UserId; set => this.User.UserId = value; }

        /// <summary>
        /// Gets or sets the buyer's first name.
        /// </summary>
        public string FirstName { get; set; }

        /// <summary>
        /// Gets or sets the buyer's last name.
        /// </summary>
        public string LastName { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the buyer uses the same address for shipping and billing.
        /// </summary>
        public bool UseSameAddress { get; set; }

        /// <summary>
        /// Gets or sets the buyer's current badge level.
        /// </summary>
        public BuyerBadge Badge { get; set; }

        /// <summary>
        /// Gets or sets the total amount spent by the buyer.
        /// </summary>
        public decimal TotalSpending { get; set; }

        /// <summary>
        /// Gets or sets the total number of purchases made by the buyer.
        /// </summary>
        public int NumberOfPurchases { get; set; }

        /// <summary>
        /// Gets or sets the discount rate applied to the buyer's purchases based on their badge level.
        /// </summary>
        public decimal Discount { get; set; }

        /// <summary>
        /// Gets or sets the buyer's shipping address.
        /// </summary>
        public Address ShippingAddress { get; set; }

        /// <summary>
        /// Gets or sets the buyer's billing address.
        /// </summary>
        public Address BillingAddress { get; set; }

        /// <summary>
        /// Gets or sets the list of buyers that are synced with this buyer's account.
        /// </summary>
        public List<Buyer> SyncedBuyerIds { get; set; }

        /// <summary>
        /// Gets or sets the buyer's wishlist containing products they are interested in.
        /// </summary>
        public BuyerWishlist Wishlist { get; set; }

        /// <summary>
        /// Gets or sets the list of linkages with other buyers.
        /// </summary>
        public List<BuyerLinkage> Linkages { get; set; }

        /// <summary>
        /// Gets or sets the list of user IDs that this buyer is following.
        /// BUYER FOLLOWS SELLER => this list represents the seller's ID that the buyer is following - Alex
        /// </summary>
        public List<int> FollowingUsersIds { get; set; }
    }
}