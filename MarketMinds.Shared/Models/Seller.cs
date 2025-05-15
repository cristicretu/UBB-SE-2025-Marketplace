// -----------------------------------------------------------------------
// <copyright file="Seller.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
namespace SharedClassLibrary.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    /// <summary>
    /// Represents a seller in the marketplace.
    /// </summary>
    public class Seller
    {
        // Add this private parameterless constructor for Entity Framework Core
        public Seller() 
        {
            this.User = new User();
            this.StoreName = string.Empty;
            this.StoreDescription = string.Empty;
            this.StoreAddress = string.Empty;
            this.FollowersCount = 0;
            this.TrustScore = 0;
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
            this.User = user;
            this.StoreName = storeName;
            this.StoreDescription = storeDescription;
            this.StoreAddress = storeAddress;
            this.FollowersCount = followersCount;
            this.TrustScore = trustScore;
        }

        /// <summary>
        /// Gets or sets the user associated with the seller.
        /// </summary>
        public User User { get; set; }

        /// <summary>
        /// Gets the unique identifier of the seller, which is the same as the user's ID.
        /// </summary>
        public int Id { get => this.User.UserId; set => this.User.UserId = value; }

        /// <summary>
        /// Gets the email address of the seller.
        /// </summary>
        public string Email { get => this.User.Email; set => this.User.Email = value; }

        /// <summary>
        /// Gets the phone number of the seller.
        /// </summary>
        public string PhoneNumber { get => this.User.PhoneNumber; set => this.User.PhoneNumber = value; }

        /// <summary>
        /// Gets the username of the seller.
        /// </summary>
        public string Username { get => this.User.Username; set => this.User.Username = value; }

        /// <summary>
        /// Gets or sets the number of followers the seller has.
        /// </summary>
        public int FollowersCount { get; set; }

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
        /// Gets or sets the trust score of the seller.
        /// </summary>
        public double TrustScore { get; set; }
    }
}