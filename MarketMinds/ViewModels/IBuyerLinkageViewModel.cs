// <copyright file="IBuyerLinkageViewModel.cs" company="PlaceholderCompany">
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
    using SharedClassLibrary.Service;
    using Microsoft.UI.Xaml;

    /// <summary>
    /// Interface for managing buyer linkage view model operations.
    /// </summary>
    public interface IBuyerLinkageViewModel : INotifyPropertyChanged
    {
        /// <summary>
        /// Gets or sets the buyer service instance.
        /// </summary>
        IBuyerService Service { get; set; }

        /// <summary>
        /// Gets or sets the current user's buyer profile.
        /// </summary>
        Buyer UserBuyer { get; set; }

        /// <summary>
        /// Gets or sets the linked buyer profile.
        /// </summary>
        Buyer LinkedBuyer { get; set; }

        /// <summary>
        /// Gets the display name for the linked buyer.
        /// </summary>
        string DisplayName { get; }

        /// <summary>
        /// Gets or sets the callback for linkage updates.
        /// </summary>
        IOnBuyerLinkageUpdatedCallback LinkageUpdatedCallback { get; set; }

        /// <summary>
        /// Gets or sets the status of the linkage between buyers.
        /// </summary>
        BuyerLinkageStatus Status { get; set; }

        /// <summary>
        /// Gets the visibility state of the request sync button.
        /// </summary>
        Visibility RequestSyncVsbl { get; }

        /// <summary>
        /// Gets the visibility state of the unsync button.
        /// </summary>
        Visibility UnsyncVsbl { get; }

        /// <summary>
        /// Gets the visibility state of the accept button.
        /// </summary>
        Visibility AcceptVsbl { get; }

        /// <summary>
        /// Gets the visibility state of the decline button.
        /// </summary>
        Visibility DeclineVsbl { get; }

        /// <summary>
        /// Requests synchronization with another buyer.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task RequestSync();

        /// <summary>
        /// Removes synchronization with another buyer.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task Unsync();

        /// <summary>
        /// Accepts a synchronization request from another buyer.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task Accept();

        /// <summary>
        /// Declines a synchronization request from another buyer.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task Decline();
    }
}
