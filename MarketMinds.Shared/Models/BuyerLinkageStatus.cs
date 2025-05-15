// <copyright file="BuyerLinkageStatus.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace SharedClassLibrary.Domain
{
    /// <summary>
    /// Represents the possible statuses of a linkage between buyers.
    /// This enum defines the different states that a buyer linkage can be in.
    /// </summary>
    /// <seealso cref="Buyer"/>
    /// <seealso cref="BuyerLinkage"/>
    public enum BuyerLinkageStatus
    {
        /// <summary>
        /// Indicates that a linkage is possible between the buyers.
        /// </summary>
        Possible = 0,

        /// <summary>
        /// Indicates that the current buyer has sent a linkage request.
        /// </summary>
        PendingSelf = 1,

        /// <summary>
        /// Indicates that another buyer has sent a linkage request.
        /// </summary>
        PendingOther = 2,

        /// <summary>
        /// Indicates that the linkage has been confirmed by both buyers.
        /// </summary>
        Confirmed = 3,
    }
}