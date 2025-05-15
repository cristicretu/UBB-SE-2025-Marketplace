using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using SharedClassLibrary.Domain;
using SharedClassLibrary.Service;

namespace WebMarketplace.Models
{
    /// <summary>
    /// View model for the Seller Profile page.
    /// </summary>
    public class SellerProfileViewModel : INotifyPropertyChanged
    {
        private readonly IUserService _userService;
        private readonly ISellerService _sellerService;
        private User _user;
        private Seller _seller;
        private List<Product> _allProducts = new List<Product>();
        private List<Product> _filteredProducts = new List<Product>();
        private string _searchText = string.Empty;
        private bool _isSortedByPrice;

        public SellerProfileViewModel() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="SellerProfileViewModel"/> class.
        /// </summary>
        /// <param name="user">The user associated with the seller.</param>
        /// <param name="userService">Service for user-related operations.</param>
        /// <param name="sellerService">Service for seller-related operations.</param>
        public SellerProfileViewModel(User user, IUserService userService, ISellerService sellerService)
        {
            _user = user;
            _userService = userService;
            _sellerService = sellerService;
        }

        /// <summary>
        /// Event that's raised when a property changes.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Gets or sets the seller.
        /// </summary>
        public Seller Seller
        {
            get => _seller;
            set
            {
                _seller = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets the user associated with the seller.
        /// </summary>
        public User User
        {
            get => _user;
            set
            {
                _user = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets the filtered products.
        /// </summary>
        public List<Product> Products
        {
            get => _filteredProducts;
            set
            {
                _filteredProducts = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets the search text.
        /// </summary>
        [Display(Name = "Search Products")]
        public string SearchText
        {
            get => _searchText;
            set
            {
                if (_searchText != value)
                {
                    _searchText = value;
                    FilterProducts(value);
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the products are sorted by price.
        /// </summary>
        public bool IsSortedByPrice
        {
            get => _isSortedByPrice;
            set
            {
                if (_isSortedByPrice != value)
                {
                    _isSortedByPrice = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Filters products based on search text.
        /// </summary>
        /// <param name="searchText">The search text to filter by.</param>
        public void FilterProducts(string searchText)
        {
            Products = string.IsNullOrEmpty(searchText)
                ? _allProducts.ToList()
                : _allProducts
                    .Where(p => p.Name.Contains(searchText, StringComparison.OrdinalIgnoreCase) ||
                               p.Description.Contains(searchText, StringComparison.OrdinalIgnoreCase))
                    .ToList();
        }

        /// <summary>
        /// Sorts the products by price.
        /// </summary>
        public void SortProducts()
        {
            IsSortedByPrice = !IsSortedByPrice;

            Products = IsSortedByPrice
                ? Products.OrderBy(p => p.Price).ToList()
                : Products.OrderByDescending(p => p.Price).ToList();
        }

        /// <summary>
        /// Initializes the view model.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task InitializeAsync()
        {
            try
            {
                if (User != null && User.Role == UserRole.Seller)
                {
                    Seller = await _sellerService.GetSellerByUser(User);

                    // Add debug logging if possible
                    Console.WriteLine($"Seller loaded: {Seller?.StoreName ?? "NULL"}");

                    if (Seller != null)
                    {
                        await LoadProducts();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in InitializeAsync: {ex.Message}");
                // Initialize empty Seller object to avoid null reference
                Seller = new Seller(User);
            }
        }


        /// <summary>
        /// Loads the seller's products.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        private async Task LoadProducts()
        {
            if (Seller != null)
            {
                try
                {
                    _allProducts = (await _sellerService.GetAllProducts(Seller.Id)).ToList();
                    FilterProducts(SearchText);
                }
                catch (Exception ex)
                {
                    // Log the exception
                    Console.WriteLine($"Error loading products: {ex.Message}");
                    _allProducts = new List<Product>();
                    Products = new List<Product>();
                }
            }
        }

        /// <summary>
        /// Updates the seller profile.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task UpdateSellerProfileAsync()
        {
            if (Seller != null)
            {
                try
                {
                    await _sellerService.UpdateSeller(Seller);
                }
                catch (Exception ex)
                {
                    // Log the exception
                    Console.WriteLine($"Error updating seller: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Notifies that a property has changed.
        /// </summary>
        /// <param name="propertyName">The name of the property that changed.</param>
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
