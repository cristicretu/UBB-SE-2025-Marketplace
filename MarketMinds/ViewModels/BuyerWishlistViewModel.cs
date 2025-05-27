// <copyright file="BuyerWishlistViewModel.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace MarketMinds.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading.Tasks;
    using MarketMinds.Shared.Models;
    using MarketMinds.Shared.Services;
    using MarketMinds.Shared.Services.BuyProductsService;

    /// <summary>
    /// View model class for managing buyer wishlist data and operations.
    /// </summary>
    public partial class BuyerWishlistViewModel : IBuyerWishlistViewModel
    {
        // Remove the unused field
        // private List<IBuyerWishlistItemViewModel>? allItems; -- FIXED: Problem 3
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

        public IBuyProductsService ProductService { get; set; } = null!;

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
                _ = UpdateItemsAsync();
            }
        }

        /// <inheritdoc/>
        public ObservableCollection<IBuyerWishlistItemViewModel> Items
        {
            get
            {
                if (this.items == null)
                {
                    // Initialize with empty collection
                    this.items = new ObservableCollection<IBuyerWishlistItemViewModel>();
                    // Load items asynchronously
                    _ = LoadItemsAsync();
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
            get => this.selectedSort ?? this.SortOptions.FirstOrDefault();
            set
            {
                this.selectedSort = value;
                _ = UpdateItemsAsync();
            }
        }

        /// <inheritdoc/>
        public bool FamilySyncActive
        {
            get => this.familySyncActive;
            set
            {
                this.familySyncActive = value;
                _ = UpdateItemsAsync();
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
                ProductService = this.ProductService,
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
            await LoadItemsAsync();
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

            // FIXED: Problems 1 - Update the code to check for confirmed linkages by accessing buyer instead of status
            var linkedItems = this.Buyer.Linkages
                .Where(link => link.Buyer2 != null) // Make sure there's a valid buyer
                .Select(link => link.Buyer2.Wishlist.Items)
                .SelectMany(list => list)
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
            var product = this.ProductService.GetProductById(wishlistItem.ProductId);
            // merge-nicusor, fetch the product from the wishlistItem.productId
            var item = new BuyerWishlistItemViewModel(App.ShoppingCartService)
            {
                ProductId = wishlistItem.ProductId,
                OwnItem = canDelete,
                RemoveCallback = this,
                Product = product, // FIXED: Problem 4 - Product might be null, but the class hierarchy allows it
                Title = product?.Title ?? "Unknown Product",
                Description = product?.Description ?? string.Empty,
                Price = (decimal)(product?.Price ?? 0)
            };
            return item;
        }

        public async Task LoadItemsAsync()
        {
            var allItems = await ComputeAllItemsAsync();
            this.items = new ObservableCollection<IBuyerWishlistItemViewModel>(allItems);
            this.OnPropertyChanged(nameof(this.Items));
        }

        private async Task<List<IBuyerWishlistItemViewModel>> ComputeAllItemsAsync()
        {
            try
            {
                // Load own items
                var ownItems = await Task.WhenAll(this.Buyer.Wishlist.Items.Select(x => GetWishlistItemDetailsAsync(x, true)));

                // Load linked items if family sync is active
                var linkedItems = new List<IBuyerWishlistItemViewModel>();
                if (this.familySyncActive)
                {
                    // FIXED: Problem 2 - Update the code to check for confirmed linkages by looking at buyer linkages directly
                    Debug.WriteLine(this.Buyer.Linkages.Count + " linkages found for buyer " + this.Buyer.Id);
                    var linkedBuyerIds = this.Buyer.Linkages
                        .Select(link => link.GetOtherBuyerId(this.Buyer.Id))
                        .Where(id => id.HasValue)
                        .Select(id => id.Value);

                    foreach (var linkedBuyerId in linkedBuyerIds)
                    {
                        var linkedBuyerItems = await this.BuyerService.GetWishlistItems(linkedBuyerId);
                        linkedItems.AddRange(await Task.WhenAll(linkedBuyerItems.Select(item => GetWishlistItemDetailsAsync(item))));
                    }
                }

                // Combine and deduplicate items
                var allItems = ownItems.Concat(linkedItems)
                    .GroupBy(x => x.ProductId)
                    .Select(itemsWithSameProduct => itemsWithSameProduct
                        .OrderByDescending(item => item.OwnItem)
                        .First())
                    .ToList();

                return allItems;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error computing wishlist items: {ex.Message}");
                return new List<IBuyerWishlistItemViewModel>();
            }
        }

        private async Task<IBuyerWishlistItemViewModel> GetWishlistItemDetailsAsync(BuyerWishlistItem wishlistItem, bool canDelete = false)
        {
            try
            {
                var product = await ProductService.GetProductByIdAsync(wishlistItem.ProductId) as BuyProduct;
                if (product == null)
                {
                    return new BuyerWishlistItemViewModel(App.ShoppingCartService)
                    {
                        ProductId = wishlistItem.ProductId,
                        OwnItem = canDelete,
                        RemoveCallback = this,
                        Title = "Product Not Found",
                        Description = "This product is no longer available",
                        Price = 0,
                        ImageSource = "ms-appx:///Assets/Products/default-product.png"
                    };
                }

                string imageUrl = product.Images?.FirstOrDefault()?.Url ?? "ms-appx:///Assets/Products/default-product.png";
                return new BuyerWishlistItemViewModel(App.ShoppingCartService)
                {
                    ProductId = wishlistItem.ProductId,
                    OwnItem = canDelete,
                    RemoveCallback = this,
                    Product = product, // FIXED: Problem 4 - Product is not null here as we checked above
                    Title = product.Title ?? "Unknown Product",
                    Description = product.Description ?? string.Empty,
                    Price = (decimal)product.Price,
                    ImageSource = imageUrl
                };
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading wishlist item {wishlistItem.ProductId}: {ex.Message}");
                return new BuyerWishlistItemViewModel(App.ShoppingCartService)
                {
                    ProductId = wishlistItem.ProductId,
                    OwnItem = canDelete,
                    RemoveCallback = this,
                    Title = "Error Loading Product",
                    Description = "There was an error loading this product",
                    Price = 0,
                    ImageSource = "ms-appx:///Assets/Products/default-product.png"
                };
            }
        }

        private async Task UpdateItemsAsync()
        {
            try
            {
                var newItems = await ComputeAllItemsAsync();
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
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error updating wishlist items: {ex.Message}");
                // You might want to show an error message to the user here
            }
        }
    }
}