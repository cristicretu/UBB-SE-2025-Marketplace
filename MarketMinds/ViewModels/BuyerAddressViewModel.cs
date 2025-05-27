// <copyright file="BuyerAddressViewModel.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace MarketMinds.ViewModels
{
    using System.ComponentModel;
    using MarketMinds.Shared.Models;

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
            this.address = address ?? new Address();
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
                    this.address = value ?? new Address();
                    this.OnPropertyChanged(nameof(this.Address));
                    // Notify all address property changes
                    this.OnPropertyChanged(nameof(this.StreetLine));
                    this.OnPropertyChanged(nameof(this.City));
                    this.OnPropertyChanged(nameof(this.Country));
                    this.OnPropertyChanged(nameof(this.PostalCode));
                }
            }
        }

        /// <summary>
        /// Gets or sets the street line of the address with proper change notification.
        /// </summary>
        public string StreetLine
        {
            get => this.address?.StreetLine ?? string.Empty;
            set
            {
                if (this.address != null && this.address.StreetLine != value)
                {
                    this.address.StreetLine = value ?? string.Empty;
                    this.OnPropertyChanged(nameof(this.StreetLine));
                }
            }
        }

        /// <summary>
        /// Gets or sets the city of the address with proper change notification.
        /// </summary>
        public string City
        {
            get => this.address?.City ?? string.Empty;
            set
            {
                if (this.address != null && this.address.City != value)
                {
                    this.address.City = value ?? string.Empty;
                    this.OnPropertyChanged(nameof(this.City));
                }
            }
        }

        /// <summary>
        /// Gets or sets the country of the address with proper change notification.
        /// </summary>
        public string Country
        {
            get => this.address?.Country ?? string.Empty;
            set
            {
                if (this.address != null && this.address.Country != value)
                {
                    this.address.Country = value ?? string.Empty;
                    this.OnPropertyChanged(nameof(this.Country));
                }
            }
        }

        /// <summary>
        /// Gets or sets the postal code of the address with proper change notification.
        /// </summary>
        public string PostalCode
        {
            get => this.address?.PostalCode ?? string.Empty;
            set
            {
                if (this.address != null && this.address.PostalCode != value)
                {
                    this.address.PostalCode = value ?? string.Empty;
                    this.OnPropertyChanged(nameof(this.PostalCode));
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
