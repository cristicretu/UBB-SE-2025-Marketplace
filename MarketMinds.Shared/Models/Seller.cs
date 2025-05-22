// -----------------------------------------------------------------------
// <copyright file="Seller.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
namespace MarketMinds.Shared.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Diagnostics;
    /// <summary>
    /// Represents a seller in the marketplace.
    /// </summary>
    public class Seller
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Seller"/> class.
        /// </summary>
        public Seller()
        {
            Debug.WriteLine("Seller default constructor called");
            // Initialize collections
            AuctionProducts = new List<AuctionProduct>();
            BuyProducts = new List<BuyProduct>();
            BorrowProducts = new List<BorrowProduct>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Seller"/> class with the specified user and store details.
        /// </summary>
        /// <param name="user">The user associated with the seller.</param>
        /// <param name="storeName">The name of the seller's store.</param>
        /// <param name="storeDescription">The description of the seller's store.</param>
        /// <param name="storeAddress">The address of the seller's store.</param>
        /// <param name="followersCount">The number of followers the seller has.</param>
        /// <param name="trustScore">The trust score of the seller.</param>
        public Seller(User user, string storeName = "", string storeDescription = "", string storeAddress = "", int followersCount = 0, double trustScore = 0)
        {
            Debug.WriteLine($"Seller constructor called with User ID: {user?.Id ?? -1}");
            this.User = user ?? throw new ArgumentNullException(nameof(user));
            this.Id = user.Id; // Set seller ID to match user ID
            this.StoreName = storeName;
            this.StoreDescription = storeDescription;
            this.StoreAddress = storeAddress;
            this.FollowersCount = followersCount;
            this.TrustScore = trustScore;

            // Initialize collections
            AuctionProducts = new List<AuctionProduct>();
            BuyProducts = new List<BuyProduct>();
            BorrowProducts = new List<BorrowProduct>();
        }

        /// <summary>
        /// Gets or sets the user associated with the seller.
        /// </summary>
        public User User { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier of the seller, which is the same as the user's ID.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the name of the seller's store.
        /// </summary>
        public string StoreName { get; set; }

        /// <summary>
        /// Gets or sets the description of the seller's store.
        /// </summary>
        public string StoreDescription { get; set; }

        /// <summary>
        /// Gets or sets the address of the seller's store.
        /// </summary>
        public string StoreAddress { get; set; }

        /// <summary>
        /// Gets or sets the number of followers the seller has.
        /// </summary>
        public int FollowersCount { get; set; }

        /// <summary>
        /// Gets or sets the trust score of the seller.
        /// </summary>
        public double TrustScore { get; set; }

        /// <summary>
        /// Gets or sets the collection of auction products created by this seller.
        /// </summary>
        public ICollection<AuctionProduct> AuctionProducts { get; set; }

        /// <summary>
        /// Gets or sets the collection of buy products created by this seller.
        /// </summary>
        public ICollection<BuyProduct> BuyProducts { get; set; }

        /// <summary>
        /// Gets or sets the collection of borrow products created by this seller.
        /// </summary>
        public ICollection<BorrowProduct> BorrowProducts { get; set; }

        /// <summary>
        /// Gets or sets the email address of the seller.
        /// </summary>
        [NotMapped]
        public string Email
        {
            get
            {
                Debug.WriteLine($"Getting Email. User is {(User == null ? "NULL" : "NOT NULL")}");
                return User?.Email ?? string.Empty;
            }
            set
            {
                Debug.WriteLine($"Setting Email to '{value}'");
                if (User != null)
                    User.Email = value;
                else
                    Debug.WriteLine("WARNING: Cannot set Email, User is NULL");
            }
        }

        /// <summary>
        /// Gets or sets the phone number of the seller.
        /// </summary>
        public string PhoneNumber
        {
            get
            {
                Debug.WriteLine($"Getting PhoneNumber. User is {(User == null ? "NULL" : "NOT NULL")}");
                return User?.PhoneNumber ?? string.Empty;
            }
            set
            {
                Debug.WriteLine($"Setting PhoneNumber to '{value}'");
                if (User != null)
                    User.PhoneNumber = value;
                else
                    Debug.WriteLine("WARNING: Cannot set PhoneNumber, User is NULL");
            }
        }

        /// <summary>
        /// Gets or sets the username of the seller.
        /// </summary>
        [NotMapped]
        public string Username
        {
            get
            {
                Debug.WriteLine($"Getting Username. User is {(User == null ? "NULL" : "NOT NULL")}");
                return User?.Username ?? string.Empty;
            }
            set
            {
                Debug.WriteLine($"Setting Username to '{value}'");
                if (User != null)
                    User.Username = value;
                else
                    Debug.WriteLine("WARNING: Cannot set Username, User is NULL");
            }
        }
    }
}