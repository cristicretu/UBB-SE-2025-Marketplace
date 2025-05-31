using System.ComponentModel;
using System.Runtime.CompilerServices;
using System;
using System.Collections.Generic;
using MarketMinds.Shared.Models;
using MarketMinds.Shared.Services.ImagineUploadService;

namespace MarketMinds.ViewModels
{
    public abstract class CreateListingViewModelBase : INotifyPropertyChanged
    {
        private string selectedListingType;
        private string price;
        private string stockQuantity;
        private DateTimeOffset? startDate;
        private DateTimeOffset? endDate;
        private string dailyRate;
        private string startingBid;
        private DateTimeOffset? auctionEndDate;

        // Common properties
        public string Title { get; set; }
        public Category Category { get; set; }
        public List<ProductTag> Tags { get; set; }
        public string Description { get; set; }
        public List<Image> Images { get; set; }
        public Condition Condition { get; set; }
        private readonly ImageUploadService imageService;

        // Listing type and conditional visibility properties
        public string SelectedListingType
        {
            get => selectedListingType;
            set
            {
                selectedListingType = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsBuyProductType));
                OnPropertyChanged(nameof(IsBorrowProductType));
                OnPropertyChanged(nameof(IsAuctionProductType));
                OnPropertyChanged(nameof(CreateButtonText));
            }
        }

        public bool IsBuyProductType => SelectedListingType == "Buy";
        public bool IsBorrowProductType => SelectedListingType == "Borrow";
        public bool IsAuctionProductType => SelectedListingType == "Auction";

        // Buy product properties
        public string Price
        {
            get => price;
            set
            {
                price = value;
                OnPropertyChanged();
            }
        }

        public string StockQuantity
        {
            get => stockQuantity;
            set
            {
                stockQuantity = value;
                OnPropertyChanged();
            }
        }

        // Borrow product properties
        public DateTimeOffset? StartDate
        {
            get => startDate;
            set
            {
                startDate = value;
                OnPropertyChanged();
            }
        }

        public DateTimeOffset? EndDate
        {
            get => endDate;
            set
            {
                endDate = value;
                OnPropertyChanged();
            }
        }

        public string DailyRate
        {
            get => dailyRate;
            set
            {
                dailyRate = value;
                OnPropertyChanged();
            }
        }

        // Auction product properties
        public string StartingBid
        {
            get => startingBid;
            set
            {
                startingBid = value;
                OnPropertyChanged();
            }
        }

        public DateTimeOffset? AuctionEndDate
        {
            get => auctionEndDate;
            set
            {
                auctionEndDate = value;
                OnPropertyChanged();
            }
        }

        // Dynamic button text
        public string CreateButtonText => SelectedListingType switch
        {
            "Borrow" => "Create Rental",
            "Auction" => "Create Auction",
            "Buy" => "Create Listing"
        };

        public CreateListingViewModelBase()
        {
            imageService = new ImageUploadService(App.Configuration);
            Images = new List<Image>();
        }

        public string ImagesString
        {
            get => imageService.FormatImagesString(Images);
            set => Images = imageService.ParseImagesString(value);
        }

        public abstract void CreateListing(Product product);

        // INotifyPropertyChanged implementation
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

