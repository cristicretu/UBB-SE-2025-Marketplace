// <copyright file="IBuyerAddressViewModel.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace MarketMinds.ViewModels
{
    using System.ComponentModel;
    using MarketMinds.Shared.Models;

    /// <summary>
    /// Interface for managing buyer address view model operations.
    /// </summary>
    public interface IBuyerAddressViewModel : INotifyPropertyChanged
    {
        /// <summary>
        /// Gets or sets the address associated with the buyer.
        /// </summary>
        Address Address { get; set; }
    }
}