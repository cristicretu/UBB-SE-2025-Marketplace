// <copyright file="BuyerLinkageViewModel.cs" company="PlaceholderCompany">
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
    /// View model class for managing buyer linkage operations and UI state.
    /// </summary>
    public class BuyerLinkageViewModel : IBuyerLinkageViewModel
    {
        private BuyerLinkageStatus status = BuyerLinkageStatus.Possible;
        private Buyer linkedBuyer = null!;

        /// <summary>
        /// Event that is raised when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <inheritdoc/>
        public IBuyerService Service { get; set; } = null!;

        /// <inheritdoc/>
        public Buyer UserBuyer { get; set; } = null!;

        /// <inheritdoc/>
        public Buyer LinkedBuyer
        {
            get => this.linkedBuyer;
            set
            {
                if (this.linkedBuyer != value)
                {
                    this.linkedBuyer = value;
                    this.UpdateDisplayName();
                    this.OnPropertyChanged(nameof(this.LinkedBuyer));
                }
            }
        }

        /// <inheritdoc/>
        public string DisplayName { get; private set; } = null!;

        /// <inheritdoc/>
        public IOnBuyerLinkageUpdatedCallback LinkageUpdatedCallback { get; set; } = null!;

        /// <inheritdoc/>
        public BuyerLinkageStatus Status
        {
            get => this.status;
            set
            {
                if (this.status != value)
                {
                    this.status = value;
                    System.Diagnostics.Debug.WriteLine($"[BuyerLinkageViewModel] Status for buyer {this.LinkedBuyer.Id} ({this.LinkedBuyer.FirstName} {this.LinkedBuyer.LastName}) set to {this.status}");
                    this.UpdateDisplayName();
                    this.OnPropertyChanged(nameof(this.Status));
                    this.OnPropertyChanged(nameof(this.RequestSyncVsbl));
                    this.OnPropertyChanged(nameof(this.CancelRequestVsbl));
                    this.OnPropertyChanged(nameof(this.AcceptVsbl));
                    this.OnPropertyChanged(nameof(this.DeclineVsbl));
                    this.OnPropertyChanged(nameof(this.UnsyncVsbl));
                }
            }
        }

        /// <inheritdoc/>
        public Visibility RequestSyncVsbl
        {
            get
            {
                // Show Request Sync button only when there's no existing linkage
                var visibility = this.Status == BuyerLinkageStatus.Possible ? Visibility.Visible : Visibility.Collapsed;
                System.Diagnostics.Debug.WriteLine($"[RequestSyncVsbl] For buyer {this.LinkedBuyer.Id} ({this.LinkedBuyer.FirstName}): Status={this.Status}, Visibility={visibility}");
                return visibility;
            }
        }

        /// <inheritdoc/>
        public Visibility CancelRequestVsbl
        {
            get
            {
                // Show Cancel button only when the current user is the sender of a pending request
                var visibility = this.Status == BuyerLinkageStatus.PendingOther ? Visibility.Visible : Visibility.Collapsed;
                System.Diagnostics.Debug.WriteLine($"[CancelRequestVsbl] For buyer {this.LinkedBuyer.Id} ({this.LinkedBuyer.FirstName}): Status={this.Status}, Visibility={visibility}");
                return visibility;
            }
        }

        /// <inheritdoc/>
        public Visibility AcceptVsbl
        {
            get
            {
                // Show Accept button only when the current user is the receiver of a pending request
                var visibility = this.Status == BuyerLinkageStatus.PendingSelf ? Visibility.Visible : Visibility.Collapsed;
                System.Diagnostics.Debug.WriteLine($"[AcceptVsbl] For buyer {this.LinkedBuyer.Id} ({this.LinkedBuyer.FirstName}): Status={this.Status}, Visibility={visibility}");
                return visibility;
            }
        }

        /// <inheritdoc/>
        public Visibility DeclineVsbl
        {
            get
            {
                // Show Decline button only when the current user is the receiver of a pending request
                var visibility = this.Status == BuyerLinkageStatus.PendingSelf ? Visibility.Visible : Visibility.Collapsed;
                System.Diagnostics.Debug.WriteLine($"[DeclineVsbl] For buyer {this.LinkedBuyer.Id} ({this.LinkedBuyer.FirstName}): Status={this.Status}, Visibility={visibility}");
                return visibility;
            }
        }

        /// <inheritdoc/>
        public Visibility UnsyncVsbl
        {
            get
            {
                // Show Unsync button only when the linkage is confirmed
                var visibility = this.Status == BuyerLinkageStatus.Confirmed ? Visibility.Visible : Visibility.Collapsed;
                System.Diagnostics.Debug.WriteLine($"[UnsyncVsbl] For buyer {this.LinkedBuyer.Id} ({this.LinkedBuyer.FirstName}): Status={this.Status}, Visibility={visibility}");
                return visibility;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BuyerLinkageViewModel"/> class.
        /// </summary>
        public BuyerLinkageViewModel()
        {
            this.DisplayName = string.Empty;
        }

        /// <inheritdoc/>
        public async Task RequestSync()
        {
            System.Diagnostics.Debug.WriteLine($"[RequestSync] Starting request sync from {this.UserBuyer.Id} to {this.LinkedBuyer.Id}");
            await this.Service.CreateLinkageRequest(this.UserBuyer, this.LinkedBuyer);
            this.Status = BuyerLinkageStatus.PendingOther;
            await this.LinkageUpdatedCallback.OnBuyerLinkageUpdated();
        }

        /// <inheritdoc/>
        public async Task Accept()
        {
            System.Diagnostics.Debug.WriteLine($"[Accept] Starting accept request from {this.UserBuyer.Id} to {this.LinkedBuyer.Id}");
            await this.Service.AcceptLinkageRequest(this.UserBuyer, this.LinkedBuyer);
            this.Status = BuyerLinkageStatus.Confirmed;
            await this.LinkageUpdatedCallback.OnBuyerLinkageUpdated();
        }

        /// <inheritdoc/>
        public async Task Decline()
        {
            System.Diagnostics.Debug.WriteLine($"[Decline] Starting decline request from {this.UserBuyer.Id} to {this.LinkedBuyer.Id}");
            await this.Service.RefuseLinkageRequest(this.UserBuyer, this.LinkedBuyer);
            this.Status = BuyerLinkageStatus.Possible;
            await this.LinkageUpdatedCallback.OnBuyerLinkageUpdated();
        }

        /// <inheritdoc/>
        public async Task Cancel()
        {
            System.Diagnostics.Debug.WriteLine($"[Cancel] Starting cancel request from {this.UserBuyer.Id} to {this.LinkedBuyer.Id}");
            await this.Service.CancelLinkageRequest(this.UserBuyer, this.LinkedBuyer);
            this.Status = BuyerLinkageStatus.Possible;
            await this.LinkageUpdatedCallback.OnBuyerLinkageUpdated();
        }

        /// <inheritdoc/>
        public async Task Unsync()
        {
            System.Diagnostics.Debug.WriteLine($"[Unsync] Starting unsync between {this.UserBuyer.Id} and {this.LinkedBuyer.Id}");
            await this.Service.BreakLinkage(this.UserBuyer, this.LinkedBuyer);
            this.Status = BuyerLinkageStatus.Possible;
            await this.LinkageUpdatedCallback.OnBuyerLinkageUpdated();
        }

        /// <summary>
        /// Raises the PropertyChanged event.
        /// </summary>
        /// <param name="propertyName">The name of the property that changed.</param>
        protected void OnPropertyChanged(string propertyName)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// Masks a name by keeping only the first letter visible.
        /// </summary>
        /// <param name="name">The name to mask.</param>
        /// <returns>The masked name with only the first letter visible.</returns>
        private static string KeepFirstLetter(string name)
        {
            return name[0].ToString().ToUpper() + new string('*', name.Length - 1);
        }

        /// <summary>
        /// Updates the display name based on the current linkage status.
        /// </summary>
        private void UpdateDisplayName()
        {
            if (this.LinkedBuyer == null)
            {
                this.DisplayName = string.Empty;
                return;
            }

            // Always show full name for confirmed linkages
            if (this.Status == BuyerLinkageStatus.Confirmed)
            {
                this.DisplayName = this.LinkedBuyer.FirstName + " " + this.LinkedBuyer.LastName;
            }
            // Show masked name for pending requests and possible linkages
            else
            {
                this.DisplayName = KeepFirstLetter(this.LinkedBuyer.FirstName) + " " + KeepFirstLetter(this.LinkedBuyer.LastName);
            }

            this.OnPropertyChanged(nameof(this.DisplayName));
        }
    }
}