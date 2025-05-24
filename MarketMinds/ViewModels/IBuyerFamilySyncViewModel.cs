// <copyright file="IBuyerFamilySyncViewModel.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace MarketMinds.ViewModels
{
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Threading.Tasks;
    using MarketMinds.Shared.Models;

    /// <summary>
    /// Interface for managing buyer family synchronization view model operations.
    /// </summary>
    public interface IBuyerFamilySyncViewModel : INotifyPropertyChanged
    {
        /// <summary>
        /// Gets or sets the collection of buyer linkage view models.
        /// </summary>
        ObservableCollection<IBuyerLinkageViewModel>? Items { get; set; }

        /// <summary>
        /// Gets the current status of the family sync.
        /// </summary>
        BuyerLinkageStatus Status { get; }

        /// <summary>
        /// Loads the linkages between buyers in the same household.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task LoadLinkages();

        /// <summary>
        /// Approves a linkage request.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task Approve();

        /// <summary>
        /// Rejects a linkage request.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task Reject();

        /// <summary>
        /// Cancels a linkage request.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task Cancel();
    }
}
