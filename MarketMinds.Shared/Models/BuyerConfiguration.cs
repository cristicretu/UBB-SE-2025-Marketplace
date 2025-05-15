// <copyright file="BuyerConfiguration.cs" company="PlaceholderCompany">
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
    /// Provides configuration settings and calculations for buyer-related features.
    /// This class contains constants and methods for managing buyer badges and discounts.
    /// </summary>
    /// <seealso cref="Buyer"/>
    /// <seealso cref="BuyerBadge"/>
    public static class BuyerConfiguration
    {
        /// <summary>
        /// The spending threshold required to achieve the Platinum badge.
        /// </summary>
        public const decimal PlatinumThreshold = 10000.0m;

        /// <summary>
        /// The spending threshold required to achieve the Gold badge.
        /// </summary>
        public const decimal GoldThreshold = 5000.0m;

        /// <summary>
        /// The spending threshold required to achieve the Silver badge.
        /// </summary>
        public const decimal SilverThreshold = 1000.0m;

        /// <summary>
        /// The discount rate applied to purchases for Platinum badge holders.
        /// </summary>
        public const decimal PlatinumDiscount = 0.15m;

        /// <summary>
        /// The discount rate applied to purchases for Gold badge holders.
        /// </summary>
        public const decimal GoldDiscount = 0.10m;

        /// <summary>
        /// The discount rate applied to purchases for Silver badge holders.
        /// </summary>
        public const decimal SilverDiscount = 0.05m;

        /// <summary>
        /// The discount rate applied to purchases for Bronze badge holders.
        /// </summary>
        public const decimal BronzeDiscount = 0.00m;
    }
}
