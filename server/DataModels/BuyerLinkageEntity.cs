// <copyright file="BuyerLinkageEntity.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Server.DataModels
{
    using System.ComponentModel.DataAnnotations.Schema;

    /// <summary>
    /// Represents the BuyerLinkage table structure for EF Core mapping.
    /// Uses a composite key of RequestingBuyerId and ReceivingBuyerId.
    /// </summary>
    [Table("BuyerLinkages")] // Explicitly set table name
    public class BuyerLinkageEntity
    {
        /// <summary>
        /// Gets or sets the requesting buyer ID.
        /// </summary>
        public int RequestingBuyerId { get; set; }

        /// <summary>
        /// Gets or sets the requesting buyer.
        /// </summary>
        public int ReceivingBuyerId { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the linkage request has been approved.
        /// This maps directly to the database column used for status persistence.
        /// </summary>
        public bool IsApproved { get; set; }

        // Note: We don't include the BuyerLinkageStatus enum here directly.
        // The mapping between IsApproved and BuyerLinkageStatus will happen
        // in the repository when converting between BuyerLinkageEntity and BuyerLinkage.
    }
}
