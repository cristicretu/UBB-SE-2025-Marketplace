// <copyright file="BuyerWishlistViewModel.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace MarketPlace924.ViewModel
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Linq;
    using System.Threading.Tasks;
    using SharedClassLibrary.Domain;
    using SharedClassLibrary.Service;

    /// <summary>
    /// View model class for managing buyer wishlist data and operations.
    /// </summary>
    public partial class BuyerWishlistViewModel : IBuyerWishlistViewModel
    {
        private List<IBuyerWishlistItemViewModel>? allItems;
        private string searchText = string.Empty;
        private bool familySyncActive;
        private string? selectedSort;
        private ObservableCollection<IBuyerWishlistItemViewModel>? items;

        /// <summary>
        /// Event that is raised when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <inheritdoc/>
        public Buyer Buyer { get; set; } = null!;

        /// <inheritdoc/>
        public IBuyerWishlistItemDetailsProvider ItemDetailsProvider { get; set; } = null!;

        /// <inheritdoc/>
        public IBuyerService BuyerService { get; set; } = null!;

        /// <inheritdoc/>
        public string SearchText
        {
            get
            {
                return this.searchText;
            }

            set
            {
                this.searchText = value;
                this.UpdateItems();
            }
        }

        /// <inheritdoc/>
        public ObservableCollection<IBuyerWishlistItemViewModel> Items
        {
            get
            {
                if (this.items == null)
                {
                    this.allItems = this.ComputeAllItems();
                    this.UpdateItems();
                }

                return this.items!;
            }
        }

        /// <inheritdoc/>
        public ObservableCollection<string> SortOptions { get; } = new()
    {
        "Sort by: Price Ascending",
        "Sort by: Price Descending",
    };

        /// <inheritdoc/>
        public string? SelectedSort
        {
            get => this.selectedSort;
            set
            {
                this.selectedSort = value;
                this.UpdateItems();
            }
        }

        /// <inheritdoc/>
        public bool FamilySyncActive
        {
            get => this.familySyncActive;
            set
            {
                this.familySyncActive = value;
                this.UpdateItems();
            }
        }

        /// <inheritdoc/>
        public IBuyerWishlistViewModel Copy()
        {
            return new BuyerWishlistViewModel
            {
                Buyer = this.Buyer,
                ItemDetailsProvider = this.ItemDetailsProvider,
                BuyerService = this.BuyerService,
                searchText = this.searchText,
                familySyncActive = this.familySyncActive,
                selectedSort = this.selectedSort,
            };
        }

        /// <inheritdoc/>
        public async Task OnBuyerWishlistItemRemove(int productId)
        {
            await this.BuyerService.RemoveWishilistItem(this.Buyer, productId);
            this.items = null;
            this.OnPropertyChanged(nameof(this.Items));
        }

        /// <summary>
        /// Raises the PropertyChanged event.
        /// </summary>
        /// <param name="propertyName">The name of the property that changed.</param>
        private void OnPropertyChanged(string propertyName)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// Computes all items in the wishlist, including linked buyers' items.
        /// </summary>
        /// <returns>A list of wishlist item view models.</returns>
        private List<IBuyerWishlistItemViewModel> ComputeAllItems()
        {
            var ownItems = this.Buyer.Wishlist.Items.Select(x => this.GetWishlistItemDetails(x, true));
            var linkedItems = this.Buyer.Linkages.Where(link => link.Status == BuyerLinkageStatus.Confirmed)
                .Select(link => link.Buyer.Wishlist.Items).SelectMany(list => list)
                .Select(wishlistItem => this.GetWishlistItemDetails(wishlistItem));
            return ownItems.Concat(linkedItems).GroupBy(x => x.ProductId)
                .Select(itemsWithSameProduct => itemsWithSameProduct
                    .OrderByDescending(item => item.OwnItem).First()).ToList();
        }

        /// <summary>
        /// Gets the details for a wishlist item.
        /// </summary>
        /// <param name="wishlistItem">The wishlist item to get details for.</param>
        /// <param name="canDelete">Indicates whether the item can be deleted.</param>
        /// <returns>A wishlist item view model with loaded details.</returns>
        private IBuyerWishlistItemViewModel GetWishlistItemDetails(BuyerWishlistItem wishlistItem, bool canDelete = false)
        {
            var item = this.ItemDetailsProvider.LoadWishlistItemDetails(new BuyerWishlistItemViewModel
            {
                ProductId = wishlistItem.ProductId,
                OwnItem = canDelete,
                RemoveCallback = this,
                Product = new Product(
                    wishlistItem.ProductId,
                    "Sample Product Name", // Replace with actual product name
                    "Sample Product Description", // Replace with actual product description
                    10.0, // Replace with actual product price
                    100, // Replace with actual stock
                    1 // Replace with actual seller ID
                )
            });
            return item;
        }

        /// <summary>
        /// Updates the filtered and sorted items collection.
        /// </summary>
        private void UpdateItems()
        {
            var newItems = this.allItems!;
            var enumerable = newItems.AsEnumerable();

            if (!this.familySyncActive)
            {
                enumerable = enumerable.Where(x => x.OwnItem);
            }

            if (this.searchText.Length > 0)
            {
                enumerable = enumerable
                    .Where(x => x.Title.ToUpper().Contains(this.searchText.ToUpper()));
            }

            if (this.selectedSort == "Sort by: Price Descending")
            {
                enumerable = enumerable.OrderByDescending(x => x.Price);
            }
            else if (this.selectedSort == "Sort by: Price Ascending")
            {
                enumerable = enumerable.OrderBy(x => x.Price);
            }

            this.items = new ObservableCollection<IBuyerWishlistItemViewModel>(enumerable.ToList());

            this.OnPropertyChanged(nameof(this.Items));
        }
    }
}