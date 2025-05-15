// <copyright file="IBuyerBadgeViewModel.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace MarketPlace924.ViewModel
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using SharedClassLibrary.Domain;

    /// <summary>
    /// Interface for managing buyer badge view model operations.
    /// </summary>
    public interface IBuyerBadgeViewModel : INotifyPropertyChanged
    {
        /// <summary>
        /// Gets or sets the buyer associated with the badge.
        /// </summary>
        Buyer Buyer { get; set; }

        /// <summary>
        /// Gets the progress percentage towards the next badge level.
        /// </summary>
        int Progress { get; }

        /// <summary>
        /// Gets the formatted discount string.
        /// </summary>
        string Discount { get; }

        /// <summary>
        /// Gets the name of the current badge.
        /// </summary>
        string BadgeName { get; }

        /// <summary>
        /// Gets the image source path for the badge icon.
        /// </summary>
        string ImageSource { get; }

        /// <summary>
        /// Notifies that the badge properties have been updated.
        /// </summary>
        void Updated();
    }
}
