// <copyright file="BuyerAddressViewModel.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace MarketPlace924.ViewModel
{
    using System.ComponentModel;
    using SharedClassLibrary.Domain;

    /// <summary>
    /// View model class for managing buyer address data and notifications.
    /// </summary>
    public sealed class BuyerAddressViewModel : IBuyerAddressViewModel
    {
        private Address address;

        /// <summary>
        /// Initializes a new instance of the <see cref="BuyerAddressViewModel"/> class.
        /// </summary>
        /// <param name="address">The initial address for the view model.</param>
        public BuyerAddressViewModel(Address address)
        {
            this.address = address;
            this.OnPropertyChanged(nameof(this.Address));
        }

        /// <summary>
        /// Event that is raised when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <inheritdoc/>
        public Address Address
        {
            get => this.address;
            set
            {
                if (this.address != value)
                {
                    this.address = value;
                    this.OnPropertyChanged(nameof(this.Address));
                }
            }
        }

        /// <summary>
        /// Raises the PropertyChanged event.
        /// </summary>
        /// <param name="propertyName">The name of the property that changed.</param>
        private void OnPropertyChanged(string propertyName)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
