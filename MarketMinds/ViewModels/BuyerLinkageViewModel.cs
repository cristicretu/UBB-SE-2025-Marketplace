// <copyright file="BuyerLinkageViewModel.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace MarketPlace924.ViewModel
{
    using System.ComponentModel;
    using System.Diagnostics.CodeAnalysis;
    using System.Threading.Tasks;
    using SharedClassLibrary.Domain;
    using SharedClassLibrary.Service;
    using Microsoft.UI.Xaml;

    /// <summary>
    /// View model class for managing buyer linkage operations and UI state.
    /// </summary>
    public class BuyerLinkageViewModel : IBuyerLinkageViewModel
    {
        private BuyerLinkageStatus status = BuyerLinkageStatus.Possible;
        private Visibility requestSyncVsbl = Visibility.Collapsed;
        private Visibility unsyncVsbl = Visibility.Collapsed;
        private Visibility acceptVsbl = Visibility.Collapsed;
        private Visibility declineVsbl = Visibility.Collapsed;

        /// <summary>
        /// Event that is raised when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <inheritdoc/>
        public IBuyerService Service { get; set; } = null!;

        /// <inheritdoc/>
        public Buyer UserBuyer { get; set; } = null!;

        /// <inheritdoc/>
        public Buyer LinkedBuyer { get; set; } = null!;

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
                this.status = value;
                this.UpdateDisplayName();
                this.requestSyncVsbl = Visibility.Collapsed;
                this.unsyncVsbl = Visibility.Collapsed;
                this.acceptVsbl = Visibility.Collapsed;
                this.declineVsbl = Visibility.Collapsed;
                if (this.status == BuyerLinkageStatus.Possible)
                {
                    this.requestSyncVsbl = Visibility.Visible;
                }
                else if (this.status == BuyerLinkageStatus.PendingSelf)
                {
                    this.acceptVsbl = Visibility.Visible;
                    this.declineVsbl = Visibility.Visible;
                }
                else if (this.status == BuyerLinkageStatus.PendingOther || this.status == BuyerLinkageStatus.Confirmed)
                {
                    this.unsyncVsbl = Visibility.Visible;
                }

                this.OnPropertyChanged(nameof(this.Status));
                this.OnPropertyChanged(nameof(this.RequestSyncVsbl));
                this.OnPropertyChanged(nameof(this.UnsyncVsbl));
                this.OnPropertyChanged(nameof(this.AcceptVsbl));
                this.OnPropertyChanged(nameof(this.DeclineVsbl));
            }
        }

        /// <inheritdoc/>
        public Visibility RequestSyncVsbl
        {
            get
            {
                return this.requestSyncVsbl;
            }
        }

        /// <inheritdoc/>
        public Visibility UnsyncVsbl
        {
            get
            {
                return this.unsyncVsbl;
            }
        }

        /// <inheritdoc/>
        public Visibility AcceptVsbl
        {
            get
            {
                return this.acceptVsbl;
            }
        }

        /// <inheritdoc/>
        public Visibility DeclineVsbl
        {
            get
            {
                return this.declineVsbl;
            }
        }

        /// <inheritdoc/>
        public async Task RequestSync()
        {
            await this.Service.CreateLinkageRequest(this.UserBuyer, this.LinkedBuyer);
            this.Status = BuyerLinkageStatus.PendingOther;
        }

        /// <inheritdoc/>
        public async Task Accept()
        {
            await this.Service.AcceptLinkageRequest(this.UserBuyer, this.LinkedBuyer);
            this.Status = BuyerLinkageStatus.Confirmed;
            await this.LinkageUpdatedCallback.OnBuyerLinkageUpdated();
        }

        /// <inheritdoc/>
        public async Task Decline()
        {
            if (this.status == BuyerLinkageStatus.PendingSelf)
            {
                await this.Service.RefuseLinkageRequest(this.UserBuyer, this.LinkedBuyer);
                await this.LinkageUpdatedCallback.OnBuyerLinkageUpdated();
            }

            this.Status = BuyerLinkageStatus.Possible;
        }

        /// <inheritdoc/>
        public async Task Unsync()
        {
            if (this.status == BuyerLinkageStatus.Confirmed)
            {
                await this.Service.BreakLinkage(this.UserBuyer, this.LinkedBuyer);
            }

            if (this.status == BuyerLinkageStatus.PendingOther)
            {
                await this.Service.CancelLinkageRequest(this.UserBuyer, this.LinkedBuyer);
            }

            await this.LinkageUpdatedCallback.OnBuyerLinkageUpdated();
            this.Status = BuyerLinkageStatus.Possible;
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
            if (this.status == BuyerLinkageStatus.Possible || this.status == BuyerLinkageStatus.PendingOther)
            {
                this.DisplayName = KeepFirstLetter(this.LinkedBuyer.FirstName) + " " + KeepFirstLetter(this.LinkedBuyer.LastName);
            }
            else
            {
                this.DisplayName = this.LinkedBuyer.FirstName + " " + this.LinkedBuyer.LastName;
            }

            this.OnPropertyChanged(nameof(this.DisplayName));
        }
    }
}