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

        /// <summary>
        /// Gets or sets the street line of the address.
        /// </summary>
        string StreetLine { get; set; }

        /// <summary>
        /// Gets or sets the city of the address.
        /// </summary>
        string City { get; set; }

        /// <summary>
        /// Gets or sets the country of the address.
        /// </summary>
        string Country { get; set; }

        /// <summary>
        /// Gets or sets the postal code of the address.
        /// </summary>
        string PostalCode { get; set; }
    }
}