using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;
using MarketMinds.Shared.Models;
using MarketMinds.Shared.Services;
using MarketMinds.Shared.Services.UserService;
using MarketMinds.Shared.Services.Interfaces;
using MarketMinds.Shared.Services.BorrowProductsService;
using MarketMinds.Shared.Services.BuyProductsService;

namespace WebMarketplace.Models
{
    /// <summary>
    /// View model for the Seller Profile page.
    /// </summary>
    public class SellerProfileViewModel : INotifyPropertyChanged
    {
        private readonly IUserService _userService;
        private readonly ISellerService _sellerService;
        private readonly IAuctionProductService _auctionProductService;
        private readonly IBorrowProductsService _borrowProductsService;
        private readonly IBuyProductsService _buyProductsService;
        private User _user;
        private Seller _seller;
        private List<Product> _allProducts = new List<Product>();
        private List<Product> _filteredProducts = new List<Product>();
        private string _searchText = string.Empty;
        private bool _isSortedByPrice;
        private List<Buyer> _followersList = new List<Buyer>();

        public SellerProfileViewModel() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="SellerProfileViewModel"/> class.
        /// </summary>
        /// <param name="user">The user associated with the seller.</param>
        /// <param name="userService">Service for user-related operations.</param>
        /// <param name="sellerService">Service for seller-related operations.</param>
        /// <param name="auctionProductService">Service for auction products.</param>
        /// <param name="borrowProductsService">Service for borrow products.</param>
        /// <param name="buyProductsService">Service for buy products.</param>
        public SellerProfileViewModel(User user, IUserService userService, ISellerService sellerService, 
            IAuctionProductService auctionProductService, IBorrowProductsService borrowProductsService, 
            IBuyProductsService buyProductsService)
        {
            _user = user;
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
            _sellerService = sellerService ?? throw new ArgumentNullException(nameof(sellerService));
            _auctionProductService = auctionProductService ?? throw new ArgumentNullException(nameof(auctionProductService));
            _borrowProductsService = borrowProductsService ?? throw new ArgumentNullException(nameof(borrowProductsService));
            _buyProductsService = buyProductsService ?? throw new ArgumentNullException(nameof(buyProductsService));
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
        /// Gets or sets the list of followers for this seller.
        /// </summary>
        public List<Buyer> FollowersList
        {
            get => _followersList;
            set
            {
                _followersList = value;
                OnPropertyChanged();
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
                    .Where(p => p.Title.Contains(searchText, StringComparison.OrdinalIgnoreCase) ||
                               p.Description.Contains(searchText, StringComparison.OrdinalIgnoreCase))
                    .ToList();
        }

        /// <summary>
        /// Sorts the products by price.
        /// </summary>
        public void SortProducts()
        {
            IsSortedByPrice = !IsSortedByPrice;
            SortProductsWithDirection(IsSortedByPrice);
        }

        /// <summary>
        /// Sorts the products by price in the specified direction.
        /// </summary>
        /// <param name="ascending">True for ascending order, false for descending order.</param>
        public void SortProductsWithDirection(bool ascending)
        {
            if (ascending)
            {
                // Sort ascending by the appropriate price property for each product type
                Products = Products.OrderBy(p => GetProductPrice(p)).ToList();
            }
            else
            {
                // Sort descending by the appropriate price property for each product type
                Products = Products.OrderByDescending(p => GetProductPrice(p)).ToList();
            }
        }

        /// <summary>
        /// Gets the appropriate price value for a product based on its type.
        /// </summary>
        /// <param name="product">The product to get the price for.</param>
        /// <returns>The price value for sorting.</returns>
        private decimal GetProductPrice(Product product)
        {
            if (product is MarketMinds.Shared.Models.BuyProduct buyProduct)
            {
                return (decimal)buyProduct.Price;
            }
            else if (product is MarketMinds.Shared.Models.AuctionProduct auctionProduct)
            {
                return (decimal)auctionProduct.CurrentPrice;
            }
            else if (product is MarketMinds.Shared.Models.BorrowProduct borrowProduct)
            {
                return (decimal)borrowProduct.DailyRate;
            }
            else
            {
                return (decimal)product.Price; // Fallback to base price
            }
        }

        /// <summary>
        /// Initializes the view model.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task InitializeAsync()
        {
            try
            {
                if (User != null && User.UserType == (int)UserRole.Seller)
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
                    _allProducts = new List<Product>();
                    
                    int sellerId = Seller.Id;
                    int userId = Seller.User?.Id ?? 0;
                    Console.WriteLine($"DEBUG: Loading products for Seller ID: {sellerId}, User ID: {userId}");
                    
                    // Load Buy Products
                    var buyProducts = await _sellerService.GetAllProducts(Seller.Id);
                    _allProducts.AddRange(buyProducts.Cast<Product>().ToList());
                    Console.WriteLine($"DEBUG: Loaded {buyProducts.Count} buy products");
                    
                    // Load Auction Products (these are DTOs, not entities)
                    var allAuctionProducts = await _auctionProductService.GetAllAuctionProductsAsync();
                    Console.WriteLine($"DEBUG: Total auction products in system: {allAuctionProducts.Count}");
                    
                    // Debug: List some auction products with their seller IDs
                    foreach (var auction in allAuctionProducts.Take(10)) // Show first 10 for debugging
                    {
                        Console.WriteLine($"DEBUG: Auction Product ID: {auction.Id}, Title: {auction.Title}, SellerId: {auction.SellerId}, Seller.Id: {auction.Seller?.Id ?? -1}, Seller.Username: {auction.Seller?.Username ?? "null"}");
                    }
                    
                    // Filter auction products for this seller using SellerId
                    var sellerAuctionProducts = allAuctionProducts.Where(auction => 
                        auction.SellerId == sellerId || auction.SellerId == userId)
                        .ToList();
                    
                    Console.WriteLine($"DEBUG: Auction products matching SellerId ({sellerId}) or UserId ({userId}): {sellerAuctionProducts.Count}");
                    
                    // If no matches by SellerId, try by Seller.Id as fallback
                    if (sellerAuctionProducts.Count == 0)
                    {
                        var sellerAuctionProductsBySellerObject = allAuctionProducts.Where(auction => 
                            auction.Seller != null && (auction.Seller.Id == sellerId || auction.Seller.Id == userId))
                            .ToList();
                        Console.WriteLine($"DEBUG: Fallback - Auction products matching Seller.Id ({sellerId}) or ({userId}): {sellerAuctionProductsBySellerObject.Count}");
                        sellerAuctionProducts = sellerAuctionProductsBySellerObject;
                    }
                    
                    // Add auction products directly since they are already AuctionProduct entities
                    _allProducts.AddRange(sellerAuctionProducts.Cast<Product>().ToList());
                    Console.WriteLine($"DEBUG: Using {sellerAuctionProducts.Count} auction products for seller");
                    
                    // Load Borrow Products
                    var allBorrowProducts = await _borrowProductsService.GetAllBorrowProductsAsync();
                    Console.WriteLine($"DEBUG: Total borrow products in system: {allBorrowProducts.Count}");
                    
                    // Debug: List some borrow products with their seller IDs
                    foreach (var borrow in allBorrowProducts.Take(5)) // Show first 5 for debugging
                    {
                        Console.WriteLine($"DEBUG: Borrow Product ID: {borrow.Id}, Title: {borrow.Title}, SellerId: {borrow.SellerId}");
                    }
                    
                    // Filter borrow products for this seller
                    var sellerBorrowProducts = allBorrowProducts.Where(borrow => 
                        borrow.SellerId == sellerId || borrow.SellerId == userId)
                        .ToList();
                    
                    _allProducts.AddRange(sellerBorrowProducts.Cast<Product>().ToList());
                    Console.WriteLine($"DEBUG: Borrow products matching Seller.Id ({sellerId}) or User.Id ({userId}): {sellerBorrowProducts.Count}");
                    Console.WriteLine($"DEBUG: Using {sellerBorrowProducts.Count} borrow products for seller");
                    
                    Console.WriteLine($"DEBUG: TOTAL products loaded for seller {sellerId}: {_allProducts.Count} ({buyProducts.Count} buy + {sellerAuctionProducts.Count} auction + {sellerBorrowProducts.Count} borrow)");
                    
                    // Initialize the filtered products list
                    _filteredProducts = new List<Product>(_allProducts);
                    OnPropertyChanged(nameof(Products));
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error loading products: {ex.Message}");
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
