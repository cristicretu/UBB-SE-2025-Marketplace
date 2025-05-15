// <copyright file="BuyerLinkage.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace SharedClassLibrary.Domain
{
    /// <summary>
    /// Represents a linkage between buyers in the marketplace.
    /// This class defines the relationship status between two buyers.
    /// </summary>
    /// <seealso cref="Buyer"/>
    /// <seealso cref="BuyerLinkageStatus"/>
    public class BuyerLinkage
    {
        /// <summary>
        /// Gets or sets the buyer associated with this linkage.
        /// </summary>
        required public Buyer Buyer
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the current status of the linkage between buyers.
        /// </summary>
        public BuyerLinkageStatus Status { get; set; }
    }
}