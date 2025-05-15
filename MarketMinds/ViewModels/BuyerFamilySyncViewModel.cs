// <copyright file="BuyerFamilySyncViewModel.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace MarketPlace924.ViewModel
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Threading.Tasks;
    using SharedClassLibrary.Domain;
    using SharedClassLibrary.Service;

    /// <summary>
    /// View model class for managing buyer family synchronization data and operations.
    /// </summary>
    /// <param name="service">The buyer service instance.</param>
    /// <param name="buyer">The current buyer.</param>
    /// <param name="linkageUpdatedCallback">Callback for linkage updates.</param>
    public class BuyerFamilySyncViewModel(IBuyerService service, Buyer buyer, IOnBuyerLinkageUpdatedCallback linkageUpdatedCallback) : IBuyerFamilySyncViewModel
    {
        private readonly Buyer currentBuyer = buyer;
        private readonly IBuyerService service = service;
        private readonly IOnBuyerLinkageUpdatedCallback linkageUpdatedCallback = linkageUpdatedCallback;
        private List<IBuyerLinkageViewModel>? allItems;

        /// <summary>
        /// Event that is raised when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <inheritdoc/>
        public ObservableCollection<IBuyerLinkageViewModel>? Items { get; set; } = new();

        /// <inheritdoc/>
        public async Task LoadLinkages()
        {
            this.allItems = await this.LoadAllPossibleLinkages();
            this.Items = new ObservableCollection<IBuyerLinkageViewModel>(this.allItems);
            this.OnPropertyChanged(nameof(this.Items));
        }

        /// <summary>
        /// Raises the PropertyChanged event.
        /// </summary>
        /// <param name="propertyName">The name of the property that changed. This parameter is optional
        /// and can be provided automatically when invoked from a property.</param>
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// Loads all possible linkages for the current buyer.
        /// </summary>
        /// <returns>A task containing a list of buyer linkage view models.</returns>
        private async Task<List<IBuyerLinkageViewModel>> LoadAllPossibleLinkages()
        {
            var household = (await this.service.FindBuyersWithShippingAddress(this.currentBuyer.ShippingAddress))
                .Where(householdBuyer => householdBuyer.Id != this.currentBuyer.Id).ToList();
            var linkages = this.currentBuyer.Linkages;
            var availableLinkages = household.Select(buyer => this.NewBuyerLinkageViewModel(buyer, BuyerLinkageStatus.Possible));

            var existingLinkages = linkages.Select(linkage => this.NewBuyerLinkageViewModel(linkage.Buyer, linkage.Status));
            return availableLinkages.Concat(existingLinkages)
                .GroupBy(link => link.LinkedBuyer.Id)
                .Select(group => group.OrderByDescending(linkage => linkage.Status).First())
                .ToList();
        }

        /// <summary>
        /// Creates a new buyer linkage view model.
        /// </summary>
        /// <param name="buyer">The buyer to link with.</param>
        /// <param name="status">The status of the linkage.</param>
        /// <returns>A new buyer linkage view model instance.</returns>
        private IBuyerLinkageViewModel NewBuyerLinkageViewModel(Buyer buyer, BuyerLinkageStatus status)
        {
            return new BuyerLinkageViewModel
            {
                LinkageUpdatedCallback = this.linkageUpdatedCallback,
                Service = this.service,
                UserBuyer = this.currentBuyer,
                LinkedBuyer = buyer,
                Status = status,
            };
        }
    }
}