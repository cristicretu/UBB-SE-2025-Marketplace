// <copyright file="IBuyerLinkageViewModel.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace MarketMinds.ViewModels
{
    using System.ComponentModel;
    using System.Threading.Tasks;
    using MarketMinds.Shared.Models;
    using MarketMinds.Shared.Services;
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
        /// Gets the visibility state of the cancel request button.
        /// </summary>
        Visibility CancelRequestVsbl { get; }

        /// <summary>
        /// Gets the visibility state of the accept button.
        /// </summary>
        Visibility AcceptVsbl { get; }

        /// <summary>
        /// Gets the visibility state of the decline button.
        /// </summary>
        Visibility DeclineVsbl { get; }

        /// <summary>
        /// Gets the visibility state of the unsync button.
        /// </summary>
        Visibility UnsyncVsbl { get; }

        /// <summary>
        /// Requests synchronization with the linked buyer.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task RequestSync();

        /// <summary>
        /// Accepts a synchronization request.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task Accept();

        /// <summary>
        /// Declines a synchronization request.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task Decline();

        /// <summary>
        /// Cancels a synchronization request.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task Cancel();

        /// <summary>
        /// Unsynchronizes with the linked buyer.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task Unsync();
    }
}
