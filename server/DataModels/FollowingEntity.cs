// <copyright file="FollowingEntity.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Server.DataModels
{
    using System.ComponentModel.DataAnnotations.Schema;

    /// <summary>
    /// Represents the Following table structure for EF Core mapping.
    /// </summary>
    [Table("Followings")]
    public class FollowingEntity
    {
        /// <summary>
        /// Gets or sets the buyer ID.
        /// </summary>
        public int BuyerId { get; set; }

        /// <summary>
        /// Gets or sets the seller ID.
        /// </summary>
        public int SellerId { get; set; }
    }
}