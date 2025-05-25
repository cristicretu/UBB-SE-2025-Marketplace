// <copyright file="BuyerLinkageViewModel.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace MarketMinds.ViewModels
{
    using System.Windows; // Ensure this namespace is included for Visibility
    using System;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using System.Threading.Tasks;
    using MarketMinds.Shared.Models;
    using MarketMinds.Shared.Services;

    /// <summary>
    /// View model for managing buyer linkage operations and data between two buyers.
    /// </summary>
    public class BuyerLinkageViewModel : IBuyerLinkageViewModel
    {
        private BuyerLinkageStatus status;

        /// <summary>
        /// Event that is raised when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// Gets or sets the buyer service instance.
        /// </summary>
        public IBuyerService Service { get; set; } = null!;

        /// <summary>
        /// Gets or sets the current user's buyer profile.
        /// </summary>
        public Buyer UserBuyer { get; set; } = null!;

        /// <summary>
        /// Gets or sets the linked buyer profile.
        /// </summary>
        public Buyer LinkedBuyer { get; set; } = null!;

        /// <summary>
        /// Gets the display name for the linked buyer.
        /// </summary>
        public string DisplayName => $"{LinkedBuyer.FirstName} {LinkedBuyer.LastName}";

        /// <summary>
        /// Gets or sets the callback for linkage updates.
        /// </summary>
        public IOnBuyerLinkageUpdatedCallback LinkageUpdatedCallback { get; set; } = null!;

        /// <summary>
        /// Gets or sets the status of the linkage between buyers.
        /// </summary>
        public BuyerLinkageStatus Status
        {
            get => this.status;
            set
            {
                if (this.status != value)
                {
                    this.status = value;
                    this.OnPropertyChanged();
                    this.OnPropertyChanged(nameof(RequestSyncVsbl));
                    this.OnPropertyChanged(nameof(CancelRequestVsbl));
                    this.OnPropertyChanged(nameof(AcceptVsbl));
                    this.OnPropertyChanged(nameof(DeclineVsbl));
                    this.OnPropertyChanged(nameof(UnsyncVsbl));
                }
            }
        }

        /// <summary>
        /// Gets the visibility state of the request sync button.
        /// </summary>
        public System.Windows.Visibility RequestSyncVsbl => Status == BuyerLinkageStatus.Possible
            ? System.Windows.Visibility.Visible
            : System.Windows.Visibility.Collapsed;

        /// <summary>
        /// Gets the visibility state of the cancel request button.
        /// </summary>
        public System.Windows.Visibility CancelRequestVsbl => Status == BuyerLinkageStatus.PendingOther
            ? System.Windows.Visibility.Visible
            : System.Windows.Visibility.Collapsed;

        /// <summary>
        /// Gets the visibility state of the accept button.
        /// </summary>
        public System.Windows.Visibility AcceptVsbl => Status == BuyerLinkageStatus.PendingSelf
            ? System.Windows.Visibility.Visible
            : System.Windows.Visibility.Collapsed;

        /// <summary>
        /// Gets the visibility state of the decline button.
        /// </summary>
        public System.Windows.Visibility DeclineVsbl => Status == BuyerLinkageStatus.PendingSelf
            ? System.Windows.Visibility.Visible
            : System.Windows.Visibility.Collapsed;

        /// <summary>
        /// Gets the visibility state of the unsync button.
        /// </summary>
        public System.Windows.Visibility UnsyncVsbl => Status == BuyerLinkageStatus.Confirmed
            ? System.Windows.Visibility.Visible
            : System.Windows.Visibility.Collapsed;

        /// <summary>
        /// Gets the WinUI visibility state of the request sync button.
        /// </summary>
        Microsoft.UI.Xaml.Visibility IBuyerLinkageViewModel.RequestSyncVsbl => Status == BuyerLinkageStatus.Possible
            ? Microsoft.UI.Xaml.Visibility.Visible
            : Microsoft.UI.Xaml.Visibility.Collapsed;

        /// <summary>
        /// Gets the WinUI visibility state of the cancel request button.
        /// </summary>
        Microsoft.UI.Xaml.Visibility IBuyerLinkageViewModel.CancelRequestVsbl => Status == BuyerLinkageStatus.PendingOther
            ? Microsoft.UI.Xaml.Visibility.Visible
            : Microsoft.UI.Xaml.Visibility.Collapsed;

        /// <summary>
        /// Gets the WinUI visibility state of the accept button.
        /// </summary>
        Microsoft.UI.Xaml.Visibility IBuyerLinkageViewModel.AcceptVsbl => Status == BuyerLinkageStatus.PendingSelf
            ? Microsoft.UI.Xaml.Visibility.Visible
            : Microsoft.UI.Xaml.Visibility.Collapsed;

        /// <summary>
        /// Gets the WinUI visibility state of the decline button.
        /// </summary>
        Microsoft.UI.Xaml.Visibility IBuyerLinkageViewModel.DeclineVsbl => Status == BuyerLinkageStatus.PendingSelf
            ? Microsoft.UI.Xaml.Visibility.Visible
            : Microsoft.UI.Xaml.Visibility.Collapsed;

        /// <summary>
        /// Gets the WinUI visibility state of the unsync button.
        /// </summary>
        Microsoft.UI.Xaml.Visibility IBuyerLinkageViewModel.UnsyncVsbl => Status == BuyerLinkageStatus.Confirmed
            ? Microsoft.UI.Xaml.Visibility.Visible
            : Microsoft.UI.Xaml.Visibility.Collapsed;

        /// <summary>
        /// Requests synchronization with the linked buyer.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task RequestSync()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"[RequestSync] Creating linkage request from {UserBuyer.Id} to {LinkedBuyer.Id}");
                await Service.CreateLinkageRequest(UserBuyer, LinkedBuyer);
                Status = BuyerLinkageStatus.PendingOther;
                await NotifyLinkageUpdated();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[RequestSync] Error: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Accepts a synchronization request.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task Accept()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"[Accept] Accepting linkage request from {LinkedBuyer.Id} for {UserBuyer.Id}");
                await Service.CreateLinkageRequest(UserBuyer, LinkedBuyer);
                await Service.AcceptLinkageRequest(LinkedBuyer, UserBuyer);
                Status = BuyerLinkageStatus.Confirmed;
                await NotifyLinkageUpdated();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[Accept] Error: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Declines a synchronization request.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task Decline()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"[Decline] Refusing linkage request from {LinkedBuyer.Id} for {UserBuyer.Id}");
                await Service.RefuseLinkageRequest(UserBuyer, LinkedBuyer);
                Status = BuyerLinkageStatus.Possible;
                await NotifyLinkageUpdated();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[Decline] Error: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Cancels a synchronization request.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task Cancel()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"[Cancel] Canceling linkage request from {UserBuyer.Id} to {LinkedBuyer.Id}");
                await Service.CancelLinkageRequest(UserBuyer, LinkedBuyer);
                Status = BuyerLinkageStatus.Possible;
                await NotifyLinkageUpdated();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[Cancel] Error: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Unsynchronizes with the linked buyer.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task Unsync()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"[Unsync] Breaking linkage between {UserBuyer.Id} and {LinkedBuyer.Id}");
                await Service.BreakLinkage(UserBuyer, LinkedBuyer);
                Status = BuyerLinkageStatus.Possible;
                await NotifyLinkageUpdated();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[Unsync] Error: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Raises the PropertyChanged event.
        /// </summary>
        /// <param name="propertyName">The name of the property that changed. This parameter is optional
        /// and can be provided automatically when invoked from a property.</param>
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// Notifies that a linkage has been updated.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        private async Task NotifyLinkageUpdated()
        {
            if (LinkageUpdatedCallback != null)
            {
                await LinkageUpdatedCallback.OnBuyerLinkageUpdated();
            }
        }
    }
}