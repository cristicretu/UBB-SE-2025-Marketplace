using SharedClassLibrary.Domain;
using SharedClassLibrary.Service;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;

namespace WebMarketplace.Models
{
    /// <summary>
    /// View model for the "My Market Profile" page, displaying a seller's profile and products.
    /// </summary>
    public class MyMarketProfileViewModel : INotifyPropertyChanged
    {
        private readonly IBuyerService _buyerService;
        private List<Product> _allSellerProducts = new List<Product>();
        private List<Product> _filteredSellerProducts = new List<Product>();
        private bool _isFollowing;
        private string _productSearchTerm = string.Empty;

        /// <summary>
        /// Initializes a new instance of the <see cref="MyMarketProfileViewModel"/> class.
        /// </summary>
        /// <param name="buyerService">Service for buyer-related operations.</param>
        /// <param name="buyer">The current buyer viewing the profile.</param>
        /// <param name="seller">The seller whose profile is being viewed.</param>
        public MyMarketProfileViewModel(IBuyerService buyerService, Buyer buyer, Seller seller)
        {
            _buyerService = buyerService;
            Buyer = buyer;
            Seller = seller;
        }

        /// <summary>
        /// Event that's raised when a property changes.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Gets the buyer service.
        /// </summary>
        public IBuyerService BuyerService => _buyerService;

        /// <summary>
        /// Gets the current buyer.
        /// </summary>
        public Buyer Buyer { get; private set; }

        /// <summary>
        /// Gets the seller whose profile is being viewed.
        /// </summary>
        public Seller Seller { get; private set; }

        /// <summary>
        /// Gets or sets the filtered seller products displayed in the view.
        /// </summary>
        public List<Product> SellerProducts
        {
            get => _filteredSellerProducts;
            set
            {
                _filteredSellerProducts = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the current buyer is following this seller.
        /// </summary>
        public bool IsFollowing
        {
            get => _isFollowing;
            set
            {
                if (_isFollowing != value)
                {
                    _isFollowing = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Gets or sets the product search term.
        /// </summary>
        [Display(Name = "Search Products")]
        public string ProductSearchTerm
        {
            get => _productSearchTerm;
            set
            {
                if (_productSearchTerm != value)
                {
                    _productSearchTerm = value;
                    FilterProducts(value);
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Filters products based on search query.
        /// </summary>
        /// <param name="searchText">The search query.</param>
        public void FilterProducts(string searchText)
        {
            SellerProducts = string.IsNullOrEmpty(searchText)
                ? _allSellerProducts.ToList()
                : _allSellerProducts.Where(p => p.Name.Contains(searchText, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        /// <summary>
        /// Initializes data for the profile view.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task InitializeAsync()
        {
            if (Buyer != null && Seller != null)
            {
                await LoadSellerProducts();
                await CheckFollowingStatus();
            }
        }

        /// <summary>
        /// Loads products from the seller.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task LoadSellerProducts()
        {
            try
            {
                var products = await _buyerService.GetProductsForViewProfile(Seller.Id);
                _allSellerProducts = products.ToList();
                FilterProducts(ProductSearchTerm);
            }
            catch (Exception ex)
            {
                // Log the exception
                Console.WriteLine($"Error loading seller products: {ex.Message}");
            }
        }

        /// <summary>
        /// Checks if the current buyer is following this seller.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task CheckFollowingStatus()
        {
            try
            {
                IsFollowing = await _buyerService.IsFollowing(Buyer.Id, Seller.Id);
            }
            catch (Exception ex)
            {
                // Log the exception
                Console.WriteLine($"Error checking following status: {ex.Message}");
            }
        }

        /// <summary>
        /// Toggles whether the buyer follows the seller.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task ToggleFollowAsync()
        {
            try
            {
                if (IsFollowing)
                {
                    await _buyerService.UnfollowSeller(Buyer.Id, Seller.Id);
                }
                else
                {
                    await _buyerService.FollowSeller(Buyer.Id, Seller.Id);
                }

                await CheckFollowingStatus(); // Refresh the following status
            }
            catch (Exception ex)
            {
                // Log the exception
                Console.WriteLine($"Error toggling follow status: {ex.Message}");
            }
        }

        /// <summary>
        /// Notifies property change.
        /// </summary>
        /// <param name="propertyName">Name of the property that changed.</param>
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
