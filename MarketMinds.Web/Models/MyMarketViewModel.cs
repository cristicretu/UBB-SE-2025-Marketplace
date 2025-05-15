using Microsoft.AspNetCore.Mvc.Rendering;
using SharedClassLibrary.Domain;
using SharedClassLibrary.Service;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;

namespace WebMarketplace.Models
{
    /// <summary>
    /// View model for the "My Market" page, managing products, followed sellers, and UI state.
    /// </summary>
    public class MyMarketViewModel : INotifyPropertyChanged
    {
        private readonly IBuyerService _buyerService;
        private Buyer _buyer;
        private List<Product> _allProducts = new List<Product>();
        private List<Product> _filteredProducts = new List<Product>();
        private List<Seller> _allFollowedSellers = new List<Seller>();
        private List<Seller> _filteredFollowedSellers = new List<Seller>();
        private List<Seller> _allSellers = new List<Seller>();
        private List<Seller> _filteredAllSellers = new List<Seller>();
        private bool _isFollowingListVisible;
        private int _followedSellersCount;
        private string _productSearchTerm = string.Empty;
        private string _followedSellerSearchTerm = string.Empty;
        private string _allSellerSearchTerm = string.Empty;

        /// <summary>
        /// Initializes a new instance of the <see cref="MyMarketViewModel"/> class.
        /// </summary>
        /// <param name="buyerService">Service for interacting with buyer-related data.</param>
        /// <param name="user">The current user.</param>
        public MyMarketViewModel(IBuyerService buyerService, User user)
        {
            _buyerService = buyerService;
            User = user;
            IsFollowingListVisible = false;
        }

        /// <summary>
        /// Event that's raised when a property changes.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Gets or sets the buyer service.
        /// </summary>
        public IBuyerService BuyerService => _buyerService;

        /// <summary>
        /// Gets or sets the current user.
        /// </summary>
        public User User { get; set; }

        /// <summary>
        /// Gets or sets the current buyer.
        /// </summary>
        public Buyer Buyer
        {
            get => _buyer;
            set
            {
                _buyer = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets the filtered products displayed in the "My Market" feed.
        /// </summary>
        public List<Product> MyMarketProducts
        {
            get => _filteredProducts;
            set
            {
                _filteredProducts = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets the filtered followed sellers list.
        /// </summary>
        public List<Seller> MyMarketFollowing
        {
            get => _filteredFollowedSellers;
            set
            {
                _filteredFollowedSellers = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets the number of followed sellers.
        /// </summary>
        public int FollowedSellersCount
        {
            get => _followedSellersCount;
            set
            {
                _followedSellersCount = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets the filtered list of all sellers.
        /// </summary>
        public List<Seller> AllSellersList
        {
            get => _filteredAllSellers;
            set
            {
                _filteredAllSellers = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the list of followers is visible.
        /// </summary>
        public bool IsFollowingListVisible
        {
            get => _isFollowingListVisible;
            set
            {
                if (_isFollowingListVisible != value)
                {
                    _isFollowingListVisible = value;
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
                _productSearchTerm = value;
                FilterProducts(value);
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets the followed seller search term.
        /// </summary>
        [Display(Name = "Search Followed Sellers")]
        public string FollowedSellerSearchTerm
        {
            get => _followedSellerSearchTerm;
            set
            {
                _followedSellerSearchTerm = value;
                FilterFollowing(value);
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets the all seller search term.
        /// </summary>
        [Display(Name = "Search All Sellers")]
        public string AllSellerSearchTerm
        {
            get => _allSellerSearchTerm;
            set
            {
                _allSellerSearchTerm = value;
                FilterAllSellers(value);
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Filters products based on search query.
        /// </summary>
        /// <param name="searchText">The search query.</param>
        public void FilterProducts(string searchText)
        {
            MyMarketProducts = string.IsNullOrEmpty(searchText)
                ? _allProducts.ToList()
                : _allProducts.Where(p => p.Name.Contains(searchText, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        /// <summary>
        /// Filters followed sellers based on search query.
        /// </summary>
        /// <param name="searchText">The search query.</param>
        public void FilterFollowing(string searchText)
        {
            MyMarketFollowing = string.IsNullOrEmpty(searchText)
                ? _allFollowedSellers.ToList()
                : _allFollowedSellers.Where(s => s.StoreName.Contains(searchText, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        /// <summary>
        /// Filters all sellers based on search query.
        /// </summary>
        /// <param name="searchText">The search query.</param>
        public void FilterAllSellers(string searchText)
        {
            AllSellersList = string.IsNullOrEmpty(searchText)
                ? _allSellers.ToList()
                : _allSellers.Where(s => s.StoreName.Contains(searchText, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        /// <summary>
        /// Loads all necessary data for the My Market page.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task InitializeAsync()
        {
            if (User != null)
            {
                Buyer = await _buyerService.GetBuyerByUser(User);
                await LoadFollowing();
                await LoadAllSellers();
                await LoadMyMarketData();
            }
        }

        /// <summary>
        /// Toggles the visibility of the following list.
        /// </summary>
        public void ToggleFollowingListVisibility()
        {
            IsFollowingListVisible = !IsFollowingListVisible;
        }

        /// <summary>
        /// Loads products from followed sellers.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task LoadMyMarketData()
        {
            try
            {
                if (Buyer != null)
                {
                    var products = await _buyerService.GetProductsFromFollowedSellers(Buyer.FollowingUsersIds);
                    _allProducts = products.OrderByDescending(p => p.ProductId).ToList();
                    FilterProducts(ProductSearchTerm);
                }
            }
            catch (Exception ex)
            {
                // Log the exception
                Console.WriteLine($"Error loading market data: {ex.Message}");
            }
        }

        /// <summary>
        /// Loads the list of followed sellers.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task LoadFollowing()
        {
            try
            {
                if (Buyer != null)
                {
                    var sellers = await _buyerService.GetFollowedSellers(Buyer.FollowingUsersIds);
                    _allFollowedSellers = sellers.ToList();
                    FollowedSellersCount = _allFollowedSellers.Count;
                    FilterFollowing(FollowedSellerSearchTerm);
                }
            }
            catch (Exception ex)
            {
                // Log the exception
                Console.WriteLine($"Error loading followed sellers: {ex.Message}");
            }
        }

        /// <summary>
        /// Loads the list of all sellers.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task LoadAllSellers()
        {
            try
            {
                var sellers = await _buyerService.GetAllSellers();
                _allSellers = sellers.ToList();
                FilterAllSellers(AllSellerSearchTerm);
            }
            catch (Exception ex)
            {
                // Log the exception
                Console.WriteLine($"Error loading all sellers: {ex.Message}");
            }
        }

        /// <summary>
        /// Refreshes all data for the My Market page.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task RefreshData()
        {
            try
            {
                if (User != null)
                {
                    Buyer = await _buyerService.GetBuyerByUser(User);
                    await LoadFollowing();
                    await LoadAllSellers();
                    await LoadMyMarketData();
                }
            }
            catch (Exception ex)
            {
                // Log the exception
                Console.WriteLine($"Error refreshing data: {ex.Message}");
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
