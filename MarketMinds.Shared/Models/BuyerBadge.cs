// <copyright file="BuyerBadge.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace SharedClassLibrary.Domain
{
    /// <summary>
    /// Represents the different badge levels that a buyer can achieve.
    /// This enum defines the various tiers of buyer status based on their purchase history.
    /// </summary>
    /// <seealso cref="Buyer"/>
    /// <seealso cref="BuyerConfiguration"/>
    public enum BuyerBadge
    {
        /// <summary>
        /// The initial badge level for new buyers.
        /// </summary>
        BRONZE = 0,

        /// <summary>
        /// The second badge level, achieved after meeting certain spending criteria.
        /// </summary>
        SILVER = 1,

        /// <summary>
        /// The third badge level, achieved after meeting higher spending criteria.
        /// </summary>
        GOLD = 2,

        /// <summary>
        /// The highest badge level, achieved after meeting the highest spending criteria.
        /// </summary>
        PLATINUM = 3,
    }
}