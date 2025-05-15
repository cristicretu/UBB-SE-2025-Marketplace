// <copyright file="IBuyerAddressViewModel.cs" company="PlaceholderCompany">
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