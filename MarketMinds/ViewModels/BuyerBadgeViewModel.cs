// <copyright file="BuyerBadgeViewModel.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace MarketPlace924.ViewModel
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using SharedClassLibrary.Domain;
    using SharedClassLibrary.Service;

    /// <summary>
    /// View model class for managing buyer badge data and notifications.
    /// </summary>
    public class BuyerBadgeViewModel : IBuyerBadgeViewModel
    {
        // Constants for UI Progress display logic
        private const int ProgressDisplayThreshold = 95;
        private const int ProgressMinimumValue = 0;
        private const int ProgressDisplayCapValue = 24;
        private const int ProgressDisplayModulo = 25;

        private readonly IBuyerService buyerService;

        /// <summary>
        /// Initializes a new instance of the <see cref="BuyerBadgeViewModel"/> class.
        /// </summary>
        /// <param name="buyerService">The buyer service instance.</param>
        public BuyerBadgeViewModel(IBuyerService buyerService)
        {
            this.buyerService = buyerService;
        }

        /// <summary>
        /// Event that is raised when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <inheritdoc/>
        public Buyer Buyer { get; set; } = null!;

        /// <inheritdoc/>
        public int Progress
        {
            get
            {
                if (this.Buyer == null)
                {
                    return ProgressMinimumValue;
                }

                int progressValue = this.buyerService.GetBadgeProgress(this.Buyer);

                // Apply UI specific mapping using constants
                if (progressValue >= ProgressDisplayThreshold)
                {
                    return ProgressDisplayCapValue;
                }

                return progressValue % ProgressDisplayModulo;
            }
        }

        /// <inheritdoc/>
        public string Discount
        {
            get { return "Discount " + this.Buyer.Discount; }
        }

        /// <inheritdoc/>
        public string BadgeName
        {
            get { return this.Buyer.Badge.ToString().ToLower(); }
        }

        /// <inheritdoc/>
        public string ImageSource
        {
            get { return "ms-appx:///Assets/BuyerIcons/badge-" + this.Buyer.Badge.ToString().ToLower() + ".svg"; }
        }

        /// <inheritdoc/>
        public void Updated()
        {
            if (this.Buyer != null)
            {
                this.OnPropertyChanged(nameof(this.Progress));
                this.OnPropertyChanged(nameof(this.Discount));
                this.OnPropertyChanged(nameof(this.BadgeName));
                this.OnPropertyChanged(nameof(this.ImageSource));
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
    }
}