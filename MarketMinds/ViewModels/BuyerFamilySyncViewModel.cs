﻿// <copyright file="BuyerFamilySyncViewModel.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace MarketMinds.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Threading.Tasks;
    using MarketMinds.Server.Services;
    using MarketMinds.Shared.Models;
    using MarketMinds.Shared.Services;

    /// <summary>
    /// View model class for managing buyer family synchronization data and operations.
    /// </summary>
    /// <param name="service">The buyer service instance.</param>
    /// <param name="buyer">The current buyer.</param>
    /// <param name="linkageUpdatedCallback">Callback for linkage updates.</param>
    /// <param name="buyerLinkageService">The buyer linkage service.</param>
    public class BuyerFamilySyncViewModel(IBuyerService service, Buyer buyer, IOnBuyerLinkageUpdatedCallback linkageUpdatedCallback, IBuyerLinkageService buyerLinkageService) : IBuyerFamilySyncViewModel
    {
        private readonly Buyer currentBuyer = buyer;
        private readonly IBuyerService service = service;
        private readonly IOnBuyerLinkageUpdatedCallback linkageUpdatedCallback = linkageUpdatedCallback;
        private readonly IBuyerLinkageService buyerLinkageService = buyerLinkageService;
        private List<IBuyerLinkageViewModel>? allItems;
        private BuyerLinkageStatus status = BuyerLinkageStatus.None;

        /// <summary>
        /// Event that is raised when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <inheritdoc/>
        public ObservableCollection<IBuyerLinkageViewModel>? Items { get; set; } = new();

        /// <inheritdoc/>
        public BuyerLinkageStatus Status
        {
            get => this.status;
            private set
            {
                if (this.status != value)
                {
                    this.status = value;
                    this.OnPropertyChanged();
                }
            }
        }

        /// <inheritdoc/>
        public async Task LoadLinkages()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"[LoadLinkages] Starting to load linkages for buyer {this.currentBuyer.Id}");
                // Load the current buyer's linkages
                await this.service.LoadBuyer(this.currentBuyer, BuyerDataSegments.Linkages);

                // Get all possible linkages
                this.allItems = await this.LoadAllPossibleLinkages();
                System.Diagnostics.Debug.WriteLine($"[LoadLinkages] Loaded {this.allItems.Count} possible linkages");

                // Update the Items collection
                if (this.Items == null)
                {
                    this.Items = new ObservableCollection<IBuyerLinkageViewModel>();
                }

                this.Items.Clear();
                foreach (var item in this.allItems)
                {
                    this.Items.Add(item);
                    System.Diagnostics.Debug.WriteLine($"[LoadLinkages] Added item for buyer {item.LinkedBuyer.Id} with status {item.Status}");
                }

                // Update the status based on the first pending linkage
                var pendingSelf = this.allItems.FirstOrDefault(x => x.Status == BuyerLinkageStatus.PendingSent);
                var pendingOther = this.allItems.FirstOrDefault(x => x.Status == BuyerLinkageStatus.PendingReceived);

                if (pendingSelf != null)
                {
                    this.Status = BuyerLinkageStatus.PendingSent;
                    System.Diagnostics.Debug.WriteLine($"[LoadLinkages] Found pending self request, setting status to PendingSelf");
                }
                else if (pendingOther != null)
                {
                    this.Status = BuyerLinkageStatus.PendingReceived;
                    System.Diagnostics.Debug.WriteLine($"[LoadLinkages] Found pending other request, setting status to PendingOther");
                }
                else
                {
                    this.Status = BuyerLinkageStatus.None;
                    System.Diagnostics.Debug.WriteLine($"[LoadLinkages] No pending requests, setting status to Possible");
                }

                this.OnPropertyChanged(nameof(this.Items));
                this.OnPropertyChanged(nameof(this.Status));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[LoadLinkages] Error loading linkages: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"[LoadLinkages] Stack trace: {ex.StackTrace}");
            }
        }

        /// <inheritdoc/>
        public async Task Approve()
        {
            var pendingLinkage = this.allItems?.FirstOrDefault(x => x.Status == BuyerLinkageStatus.PendingSent);
            if (pendingLinkage != null)
            {
                await pendingLinkage.Accept();
                await this.LoadLinkages();
            }
        }

        /// <inheritdoc/>
        public async Task Reject()
        {
            var pendingLinkage = this.allItems?.FirstOrDefault(x => x.Status == BuyerLinkageStatus.PendingSent);
            if (pendingLinkage != null)
            {
                await pendingLinkage.Decline();
                await this.LoadLinkages();
            }
        }

        /// <inheritdoc/>
        public async Task Cancel()
        {
            var pendingLinkage = this.allItems?.FirstOrDefault(x => x.Status == BuyerLinkageStatus.PendingReceived);
            if (pendingLinkage != null)
            {
                await pendingLinkage.Cancel();
                await this.LoadLinkages();
            }
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
            try
            {
                System.Diagnostics.Debug.WriteLine($"[LoadAllPossibleLinkages] Starting for buyer {this.currentBuyer.Id} (User ID: {this.currentBuyer.User.Id})");

                // Get buyers with the same shipping address
                var household = (await this.service.FindBuyersWithShippingAddress(this.currentBuyer.ShippingAddress))
                    .Where(householdBuyer => householdBuyer.Id != this.currentBuyer.Id)
                    .ToList();

                System.Diagnostics.Debug.WriteLine($"[LoadAllPossibleLinkages] Found {household.Count} household members");

                // Get linked buyers using buyerLinkageService
                var linkedBuyers = await this.buyerLinkageService.GetLinkedBuyersAsync(this.currentBuyer.Id);
                // make sure to filter out the current buyer from the linked buyers
                linkedBuyers = linkedBuyers.Where(b => b.Id != this.currentBuyer.Id).ToList();
                // only keep unique buyers
                linkedBuyers = linkedBuyers.DistinctBy(b => b.Id).ToList();
                var linkedBuyersDict = linkedBuyers.ToDictionary(b => b.Id);

                var result = new List<IBuyerLinkageViewModel>();
                foreach (var buyer in household)
                {
                    if (linkedBuyersDict.ContainsKey(buyer.Id))
                    {
                        // Buyer is linked - show as confirmed
                        System.Diagnostics.Debug.WriteLine($"[LoadAllPossibleLinkages] Existing linkage for buyer {buyer.Id} ({buyer.FirstName} {buyer.LastName}): Confirmed");
                        result.Add(this.NewBuyerLinkageViewModel(buyer, BuyerLinkageStatus.Linked));
                    }
                    else
                    {
                        // Check if there's a pending linkage by examining the buyer's linkage status
                        var linkageStatus = await this.buyerLinkageService.GetLinkageStatusAsync(this.currentBuyer.Id, buyer.Id);

                        if (linkageStatus.IsLinked)
                        {
                            // Already linked
                            result.Add(this.NewBuyerLinkageViewModel(buyer, BuyerLinkageStatus.Linked));
                            System.Diagnostics.Debug.WriteLine($"[LoadAllPossibleLinkages] Buyer {buyer.Id} is linked, status: Confirmed");
                        }
                        else
                        {
                            // No linkage, possible
                            System.Diagnostics.Debug.WriteLine($"[LoadAllPossibleLinkages] No linkage for buyer {buyer.Id} ({buyer.FirstName} {buyer.LastName}), assigning status Possible");
                            result.Add(this.NewBuyerLinkageViewModel(buyer, BuyerLinkageStatus.None));
                        }
                    }
                }
                return result;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[LoadAllPossibleLinkages] Error: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"[LoadAllPossibleLinkages] Stack trace: {ex.StackTrace}");
                return new List<IBuyerLinkageViewModel>();
            }
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